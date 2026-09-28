using DBS.ViewModels;
using Npgsql;

namespace DBS.Repositories
{
    /// <summary>
    /// Consultas agregadas do Dashboard. Receita considera só pedidos com status 'Pago'
    /// (mesma regra do card "Receita do mês" que já existia).
    /// Datas usam o relógio do banco (CURRENT_DATE), igual ao ReceitaMes() original.
    /// </summary>
    public class DashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Períodos aceitos → expressão SQL do primeiro dia (lista fechada, nada vem do usuário)
        public static readonly string[] PeriodosValidos = { "7", "30", "90", "mes" };

        private static string InicioSql(string periodo) => periodo switch
        {
            "7"   => "(CURRENT_DATE - 6)",
            "90"  => "(CURRENT_DATE - 89)",
            "mes" => "date_trunc('month', CURRENT_DATE)::date",
            _     => "(CURRENT_DATE - 29)"   // "30"
        };

        // ---------- Série diária (gráfico principal e sparkline) ----------

        /// <summary>Um ponto por dia do período, incluindo dias sem venda (valor 0).</summary>
        public List<PontoDiario> ReceitaDiaria(string periodo)
        {
            var lista = new List<PontoDiario>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand($@"
                SELECT d::date AS dia,
                       COALESCE(SUM(p.valor) FILTER (WHERE p.status = 'Pago'), 0) AS receita,
                       COUNT(p.id)           FILTER (WHERE p.status = 'Pago')      AS pedidos
                FROM generate_series({InicioSql(periodo)}::timestamp, CURRENT_DATE::timestamp, interval '1 day') AS d
                LEFT JOIN pedido p
                       ON p.data_pedido >= d
                      AND p.data_pedido <  d + interval '1 day'
                GROUP BY d
                ORDER BY d", conn);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add(new PontoDiario(r.GetDateTime(0), r.GetDecimal(1), (int)r.GetInt64(2)));
            return lista;
        }

        // ---------- Top produtos ----------

        public List<ProdutoVendido> TopProdutos(string periodo, int limite = 5)
        {
            var lista = new List<ProdutoVendido>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand($@"
                SELECT pr.nome,
                       SUM(i.quantidade)                                   AS qtd,
                       SUM(i.quantidade * COALESCE(i.preco_unitario, 0))   AS receita
                FROM item_pedido i
                JOIN pedido  p  ON p.id  = i.id_pedido
                JOIN produto pr ON pr.id = i.id_produto
                WHERE p.status = 'Pago'
                  AND p.data_pedido >= {InicioSql(periodo)}
                  AND p.data_pedido <  CURRENT_DATE + 1
                GROUP BY pr.id, pr.nome
                ORDER BY qtd DESC, receita DESC
                LIMIT @limite", conn);
            cmd.Parameters.AddWithValue("@limite", limite);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add(new ProdutoVendido(r.GetString(0), (int)r.GetInt64(1), r.GetDecimal(2)));
            return lista;
        }

        // ---------- Pedidos por status ----------

        public List<StatusResumo> PedidosPorStatus(string periodo)
        {
            var lista = new List<StatusResumo>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand($@"
                SELECT COALESCE(status, 'Pendente') AS status,
                       COUNT(*)                     AS qtd,
                       COALESCE(SUM(valor), 0)      AS valor
                FROM pedido
                WHERE data_pedido >= {InicioSql(periodo)}
                  AND data_pedido <  CURRENT_DATE + 1
                GROUP BY 1
                ORDER BY 2 DESC", conn);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add(new StatusResumo(r.GetString(0), (int)r.GetInt64(1), r.GetDecimal(2)));
            return lista;
        }

        // ---------- Cards ----------

        /// <summary>
        /// Receita e pedidos pagos do mês atual, e a receita do mês anterior
        /// nos MESMOS dias (1 até o dia de hoje) — comparação justa no meio do mês.
        /// </summary>
        public (decimal receitaMes, int pedidosMes, decimal receitaMesAnteriorParcial) ResumoMes()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                WITH ref AS (
                    SELECT date_trunc('month', CURRENT_DATE)                        AS ini_mes,
                           date_trunc('month', CURRENT_DATE) - interval '1 month'   AS ini_ant,
                           (CURRENT_DATE - date_trunc('month', CURRENT_DATE)::date + 1) AS dias
                )
                SELECT
                    COALESCE(SUM(p.valor) FILTER (WHERE p.data_pedido >= ref.ini_mes
                                                   AND p.data_pedido <  CURRENT_DATE + 1), 0),
                    COUNT(*)             FILTER (WHERE p.data_pedido >= ref.ini_mes
                                                   AND p.data_pedido <  CURRENT_DATE + 1),
                    COALESCE(SUM(p.valor) FILTER (WHERE p.data_pedido >= ref.ini_ant
                                                   AND p.data_pedido <  LEAST(ref.ini_ant + ref.dias * interval '1 day',
                                                                              ref.ini_mes)), 0)
                FROM ref
                LEFT JOIN pedido p ON p.status = 'Pago'
                GROUP BY ref.ini_mes, ref.ini_ant, ref.dias", conn);

            using var r = cmd.ExecuteReader();
            r.Read();
            return (r.GetDecimal(0), (int)r.GetInt64(1), r.GetDecimal(2));
        }

        public (int total, int novosMes) ResumoClientes()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(@"
                SELECT COUNT(*),
                       COUNT(*) FILTER (WHERE data_cadastro >= date_trunc('month', CURRENT_DATE))
                FROM cliente", conn);
            using var r = cmd.ExecuteReader();
            r.Read();
            return ((int)r.GetInt64(0), (int)r.GetInt64(1));
        }

        public (int produtos, int unidades, int estoqueBaixo) ResumoEstoque(int limiteBaixo = 5)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(@"
                SELECT COUNT(*),
                       COALESCE(SUM(estoque), 0),
                       COUNT(*) FILTER (WHERE COALESCE(estoque, 0) <= @limite)
                FROM produto", conn);
            cmd.Parameters.AddWithValue("@limite", limiteBaixo);
            using var r = cmd.ExecuteReader();
            r.Read();
            return ((int)r.GetInt64(0), (int)r.GetInt64(1), (int)r.GetInt64(2));
        }
    }
}
