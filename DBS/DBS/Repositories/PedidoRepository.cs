using DBS.Models;
using Npgsql;

namespace DBS.Repositories
{
    public class PedidoRepository
    {
        private readonly string _connectionString;

        public PedidoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<Pedido> GetAll()
        {
            var pedidos = new List<Pedido>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
    SELECT p.id, p.id_cliente, c.nome AS cliente_nome, p.data_pedido, p.status,
           p.valor AS valor_total
    FROM pedido p
    LEFT JOIN cliente c ON c.id = p.id_cliente
    ORDER BY p.data_pedido DESC", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pedidos.Add(MapPedido(reader));
            }
            return pedidos;
        }

        public List<Pedido> GetUltimas(int quantidade = 5)
        {
            var pedidos = new List<Pedido>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
    SELECT p.id, p.id_cliente, c.nome AS cliente_nome, p.data_pedido, p.status,
           p.valor AS valor_total
    FROM pedido p
    LEFT JOIN cliente c ON c.id = p.id_cliente
    ORDER BY p.data_pedido DESC
    LIMIT @qtd", conn);
            cmd.Parameters.AddWithValue("@qtd", quantidade);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pedidos.Add(MapPedido(reader));
            }
            return pedidos;
        }

        public int CountAbertos()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM pedido WHERE status = 'Pendente' OR status IS NULL", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public decimal ReceitaMes()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            // MySQL: MONTH()/YEAR()/CURDATE() -> Postgres: EXTRACT(... FROM ...) / CURRENT_DATE
            var cmd = new NpgsqlCommand(@"
        SELECT COALESCE(SUM(valor), 0)
        FROM pedido
        WHERE EXTRACT(MONTH FROM data_pedido) = EXTRACT(MONTH FROM CURRENT_DATE)
          AND EXTRACT(YEAR FROM data_pedido)  = EXTRACT(YEAR FROM CURRENT_DATE)
          AND status = 'Pago'", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public void Insert(Pedido pedido)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(
                "INSERT INTO pedido (id_cliente, status, valor) VALUES (@idCliente, @status, @valor)",
                conn);
            cmd.Parameters.AddWithValue("@idCliente", (object?)pedido.IdCliente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", pedido.Status ?? "Pendente");
            cmd.Parameters.AddWithValue("@valor", pedido.Valor);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            // Remove itens primeiro (FK)
            var cmdItens = new NpgsqlCommand("DELETE FROM item_pedido WHERE id_pedido = @id", conn);
            cmdItens.Parameters.AddWithValue("@id", id);
            cmdItens.ExecuteNonQuery();

            var cmd = new NpgsqlCommand("DELETE FROM pedido WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static Pedido MapPedido(NpgsqlDataReader reader) => new Pedido
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            IdCliente = reader.IsDBNull(reader.GetOrdinal("id_cliente")) ? null : reader.GetInt32(reader.GetOrdinal("id_cliente")),
            ClienteNome = reader.IsDBNull(reader.GetOrdinal("cliente_nome")) ? "—" : reader.GetString(reader.GetOrdinal("cliente_nome")),
            DataPedido = reader.IsDBNull(reader.GetOrdinal("data_pedido")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("data_pedido")),
            Status = reader.IsDBNull(reader.GetOrdinal("status")) ? "Pendente" : reader.GetString(reader.GetOrdinal("status")),
            ValorTotal = reader.GetDecimal(reader.GetOrdinal("valor_total"))
        };

        public Pedido? GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(@"
        SELECT p.id, p.id_cliente, c.nome AS cliente_nome, p.data_pedido, p.status,
               p.valor AS valor_total
        FROM pedido p
        LEFT JOIN cliente c ON c.id = p.id_cliente
        WHERE p.id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapPedido(reader);
            return null;
        }

        public void Update(Pedido pedido)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(
                "UPDATE pedido SET id_cliente=@idCliente, status=@status, valor=@valor WHERE id=@id",
                conn);
            cmd.Parameters.AddWithValue("@idCliente", (object?)pedido.IdCliente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", pedido.Status ?? "Pendente");
            cmd.Parameters.AddWithValue("@valor", pedido.Valor);
            cmd.Parameters.AddWithValue("@id", pedido.Id);
            cmd.ExecuteNonQuery();
        }
    }
}