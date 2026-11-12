
using DBS.Models;
using Npgsql;

namespace DBS.Repositories
{
    public class PedidoRepository
    {
        private readonly string _connectionString;

        public PedidoRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<Pedido> GetAll()
        {
            var pedidos = new List<Pedido>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT p.id, p.id_cliente, c.nome AS cliente_nome,
                       p.data_pedido, p.status, p.valor AS valor_total,
                       p.forma_pagamento, p.parcelas,
                       p.condicao_pagamento
                FROM pedido p
                LEFT JOIN cliente c ON c.id = p.id_cliente
                ORDER BY p.data_pedido DESC", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                pedidos.Add(MapPedido(reader));

            return pedidos;
        }

        public List<Pedido> GetUltimas(int quantidade = 5)
        {
            var pedidos = new List<Pedido>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT p.id, p.id_cliente, c.nome AS cliente_nome,
                       p.data_pedido, p.status, p.valor AS valor_total,
                       p.forma_pagamento, p.parcelas,
                       p.condicao_pagamento
                FROM pedido p
                LEFT JOIN cliente c ON c.id = p.id_cliente
                ORDER BY p.data_pedido DESC
                LIMIT @qtd", conn);

            cmd.Parameters.AddWithValue("@qtd", quantidade);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                pedidos.Add(MapPedido(reader));

            return pedidos;
        }

        public List<Pedido> GetByVendedor(int idVendedor)
        {
            var pedidos = new List<Pedido>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT p.id, p.id_cliente, c.nome AS cliente_nome,
                       p.data_pedido, p.status, p.valor AS valor_total,
                       p.forma_pagamento, p.parcelas,
                       p.condicao_pagamento
                FROM pedido p
                LEFT JOIN cliente c ON c.id = p.id_cliente
                WHERE p.id_vendedor = @idVendedor
                ORDER BY p.data_pedido DESC", conn);

            cmd.Parameters.AddWithValue("@idVendedor", idVendedor);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                pedidos.Add(MapPedido(reader));

            return pedidos;
        }

        public List<Pedido> GetUltimasPorVendedor(
            int idVendedor,
            int quantidade = 5)
        {
            var pedidos = new List<Pedido>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT p.id, p.id_cliente, c.nome AS cliente_nome,
                       p.data_pedido, p.status, p.valor AS valor_total,
                       p.forma_pagamento, p.parcelas,
                       p.condicao_pagamento
                FROM pedido p
                LEFT JOIN cliente c ON c.id = p.id_cliente
                WHERE p.id_vendedor = @idVendedor
                ORDER BY p.data_pedido DESC
                LIMIT @qtd", conn);

            cmd.Parameters.AddWithValue("@idVendedor", idVendedor);
            cmd.Parameters.AddWithValue("@qtd", quantidade);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                pedidos.Add(MapPedido(reader));

            return pedidos;
        }

        public Pedido? GetByIdVendedor(int id, int idVendedor)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT p.id, p.id_cliente, c.nome AS cliente_nome,
                       p.data_pedido, p.status, p.valor AS valor_total,
                       p.forma_pagamento, p.parcelas,
                       p.condicao_pagamento
                FROM pedido p
                LEFT JOIN cliente c ON c.id = p.id_cliente
                WHERE p.id = @id
                  AND p.id_vendedor = @idVendedor", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@idVendedor", idVendedor);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
                return MapPedido(reader);

            return null;
        }

        public int RegistrarPedidoMobile(
            int idCliente,
            int idVendedor,
            string formaPagamento,
            int? parcelas,
            List<ItemPedidoResumo> itens)
        {
            if (idCliente <= 0)
                throw new InvalidOperationException("Cliente inválido.");

            if (idVendedor <= 0)
                throw new InvalidOperationException("Vendedor inválido.");

            if (string.IsNullOrWhiteSpace(formaPagamento))
                throw new InvalidOperationException(
                    "Forma de pagamento não informada.");

            if (itens == null || itens.Count == 0)
                throw new InvalidOperationException(
                    "O pedido não possui produtos.");

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                var itensValidados =
                    new List<(int IdProduto, int Quantidade, decimal Preco)>();

                decimal valorTotal = 0;

                foreach (var item in itens)
                {
                    if (!item.IdProduto.HasValue ||
                        item.IdProduto.Value <= 0)
                    {
                        throw new InvalidOperationException(
                            "Produto inválido.");
                    }

                    if (item.Quantidade <= 0)
                    {
                        throw new InvalidOperationException(
                            "Quantidade inválida.");
                    }

                    using var cmdProduto = new NpgsqlCommand(@"
                        SELECT preco
                        FROM produto
                        WHERE id = @idProduto",
                        conn, transaction);

                    cmdProduto.Parameters.AddWithValue(
                        "@idProduto",
                        item.IdProduto.Value);

                    var resultado = cmdProduto.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "Um dos produtos não foi encontrado.");
                    }

                    decimal preco = Convert.ToDecimal(resultado);

                    itensValidados.Add((
                        item.IdProduto.Value,
                        item.Quantidade,
                        preco));

                    valorTotal += preco * item.Quantidade;
                }

                using var cmdPedido = new NpgsqlCommand(@"
                    INSERT INTO pedido
                        (id_cliente, id_vendedor, status, valor,
                         forma_pagamento, parcelas)
                    VALUES
                        (@idCliente, @idVendedor, 'Pendente',
                         @valor, @formaPagamento, @parcelas)
                    RETURNING id", conn, transaction);

                cmdPedido.Parameters.AddWithValue(
                    "@idCliente", idCliente);

                cmdPedido.Parameters.AddWithValue(
                    "@idVendedor", idVendedor);

                cmdPedido.Parameters.AddWithValue(
                    "@valor", valorTotal);

                cmdPedido.Parameters.AddWithValue(
                    "@formaPagamento", formaPagamento);

                cmdPedido.Parameters.AddWithValue(
                    "@parcelas",
                    (object?)parcelas ?? DBNull.Value);

                int idPedido = Convert.ToInt32(
                    cmdPedido.ExecuteScalar());

                foreach (var item in itensValidados)
                {
                    using var cmdItem = new NpgsqlCommand(@"
                        INSERT INTO item_pedido
                            (id_pedido, id_produto,
                             quantidade, preco_unitario)
                        VALUES
                            (@idPedido, @idProduto,
                             @quantidade, @preco)",
                        conn, transaction);

                    cmdItem.Parameters.AddWithValue(
                        "@idPedido", idPedido);

                    cmdItem.Parameters.AddWithValue(
                        "@idProduto", item.IdProduto);

                    cmdItem.Parameters.AddWithValue(
                        "@quantidade", item.Quantidade);

                    cmdItem.Parameters.AddWithValue(
                        "@preco", item.Preco);

                    cmdItem.ExecuteNonQuery();
                }

                transaction.Commit();

                return idPedido;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int CountAbertos()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT COUNT(*)
                FROM pedido
                WHERE LOWER(status) = 'pendente'
                   OR status IS NULL", conn);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public decimal ReceitaMes()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT COALESCE(SUM(valor), 0)
                FROM pedido
                WHERE EXTRACT(MONTH FROM data_pedido) =
                      EXTRACT(MONTH FROM CURRENT_DATE)
                  AND EXTRACT(YEAR FROM data_pedido) =
                      EXTRACT(YEAR FROM CURRENT_DATE)
                  AND status = 'Pago'", conn);

            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public void Insert(Pedido pedido)
        {
            ValidarPagamento(pedido);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO pedido
                    (id_cliente, status, valor,
                     forma_pagamento, parcelas, condicao_pagamento)
                VALUES
                    (@idCliente, @status, @valor,
                     @formaPagamento, @parcelas, @condicaoPagamento)",
                conn);

            cmd.Parameters.AddWithValue(
                "@idCliente",
                (object?)pedido.IdCliente ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@status",
                pedido.Status ?? "Pendente");

            cmd.Parameters.AddWithValue("@valor", pedido.Valor);

            cmd.Parameters.AddWithValue(
                "@formaPagamento",
                (object?)pedido.FormaPagamento ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@parcelas",
                (object?)pedido.Parcelas ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@condicaoPagamento",
                (object?)pedido.CondicaoPagamento ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                var cmdItens = new NpgsqlCommand(
                    "DELETE FROM item_pedido WHERE id_pedido = @id",
                    conn, transaction);

                cmdItens.Parameters.AddWithValue("@id", id);
                cmdItens.ExecuteNonQuery();

                var cmd = new NpgsqlCommand(
                    "DELETE FROM pedido WHERE id = @id",
                    conn, transaction);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void CarregarItens(IEnumerable<Pedido> pedidos)
        {
            var porId = pedidos.ToDictionary(p => p.Id);

            if (porId.Count == 0)
                return;

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT i.id_pedido,
                       i.id_produto,
                       COALESCE(pr.nome, 'Produto removido') AS nome,
                       i.quantidade,
                       COALESCE(i.preco_unitario, 0) AS preco
                FROM item_pedido i
                LEFT JOIN produto pr ON pr.id = i.id_produto
                WHERE i.id_pedido = ANY(@ids)
                ORDER BY i.id_pedido, i.id", conn);

            cmd.Parameters.AddWithValue(
                "@ids", porId.Keys.ToArray());

            foreach (var p in porId.Values)
                p.Itens.Clear();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var idPedido = reader.GetInt32(0);

                porId[idPedido].Itens.Add(new ItemPedidoResumo
                {
                    IdPedido = idPedido,
                    IdProduto = reader.IsDBNull(1)
                        ? null
                        : reader.GetInt32(1),
                    NomeProduto = reader.GetString(2),
                    Quantidade = reader.GetInt32(3),
                    PrecoUnitario = reader.GetDecimal(4)
                });
            }
        }

        public Pedido? GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT p.id, p.id_cliente, c.nome AS cliente_nome,
                       p.data_pedido, p.status, p.valor AS valor_total,
                       p.forma_pagamento, p.parcelas,
                       p.condicao_pagamento
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
            ValidarPagamento(pedido);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                UPDATE pedido
                SET id_cliente = @idCliente,
                    status = @status,
                    valor = @valor,
                    forma_pagamento = @formaPagamento,
                    parcelas = @parcelas,
                    condicao_pagamento = @condicaoPagamento
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue(
                "@idCliente",
                (object?)pedido.IdCliente ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@status",
                pedido.Status ?? "Pendente");

            cmd.Parameters.AddWithValue("@valor", pedido.Valor);

            cmd.Parameters.AddWithValue(
                "@formaPagamento",
                (object?)pedido.FormaPagamento ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@parcelas",
                (object?)pedido.Parcelas ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@condicaoPagamento",
                (object?)pedido.CondicaoPagamento ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@id", pedido.Id);

            cmd.ExecuteNonQuery();
        }

        public void AtualizarComItens(Pedido pedido)
        {
            if (pedido.Id <= 0)
                throw new InvalidOperationException("Pedido inválido.");

            if (!pedido.IdCliente.HasValue ||
                pedido.IdCliente.Value <= 0)
            {
                throw new InvalidOperationException("Selecione um cliente.");
            }

            if (pedido.Itens == null || pedido.Itens.Count == 0)
            {
                throw new InvalidOperationException(
                    "Adicione pelo menos um produto ao pedido.");
            }

            string status = string.IsNullOrWhiteSpace(pedido.Status)
                ? "Pendente"
                : pedido.Status.Trim();

            if (status != "Pendente" &&
                status != "Pago" &&
                status != "Cancelado")
            {
                throw new InvalidOperationException("Status inválido.");
            }

            ValidarPagamento(pedido);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                using (var cmdCliente = new NpgsqlCommand(@"
                    SELECT EXISTS(
                        SELECT 1 FROM cliente WHERE id = @id
                    )", conn, transaction))
                {
                    cmdCliente.Parameters.AddWithValue(
                        "@id", pedido.IdCliente.Value);

                    bool clienteExiste =
                        Convert.ToBoolean(cmdCliente.ExecuteScalar());

                    if (!clienteExiste)
                        throw new InvalidOperationException(
                            "O cliente selecionado não foi encontrado.");
                }

                var itensValidados =
                    new List<(int IdProduto, int Quantidade, decimal Preco)>();

                var produtosAdicionados = new HashSet<int>();
                decimal valorTotal = 0;

                foreach (var item in pedido.Itens)
                {
                    if (!item.IdProduto.HasValue ||
                        item.IdProduto.Value <= 0)
                    {
                        throw new InvalidOperationException(
                            "Existe um produto inválido no pedido.");
                    }

                    if (item.Quantidade <= 0)
                    {
                        throw new InvalidOperationException(
                            "A quantidade dos produtos deve ser maior que zero.");
                    }

                    int idProduto = item.IdProduto.Value;

                    if (!produtosAdicionados.Add(idProduto))
                    {
                        throw new InvalidOperationException(
                            "O mesmo produto não pode ser adicionado duas vezes. Ajuste a quantidade.");
                    }

                    using var cmdProduto = new NpgsqlCommand(@"
                        SELECT preco
                        FROM produto
                        WHERE id = @idProduto
                        FOR SHARE", conn, transaction);

                    cmdProduto.Parameters.AddWithValue(
                        "@idProduto", idProduto);

                    var resultado = cmdProduto.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "Um dos produtos selecionados não foi encontrado.");
                    }

                    decimal preco = Convert.ToDecimal(resultado);

                    itensValidados.Add((
                        idProduto,
                        item.Quantidade,
                        preco));

                    valorTotal += preco * item.Quantidade;
                }

                using (var cmdPedido = new NpgsqlCommand(@"
                    UPDATE pedido
                    SET id_cliente = @idCliente,
                        status = @status,
                        valor = @valor,
                        forma_pagamento = @formaPagamento,
                        parcelas = @parcelas,
                        condicao_pagamento = @condicaoPagamento
                    WHERE id = @id", conn, transaction))
                {
                    cmdPedido.Parameters.AddWithValue(
                        "@idCliente", pedido.IdCliente.Value);

                    cmdPedido.Parameters.AddWithValue(
                        "@status", status);

                    cmdPedido.Parameters.AddWithValue(
                        "@valor", valorTotal);

                    cmdPedido.Parameters.AddWithValue(
                        "@formaPagamento", pedido.FormaPagamento!);

                    cmdPedido.Parameters.AddWithValue(
                        "@parcelas",
                        (object?)pedido.Parcelas ?? DBNull.Value);

                    cmdPedido.Parameters.AddWithValue(
                        "@condicaoPagamento",
                        (object?)pedido.CondicaoPagamento ?? DBNull.Value);

                    cmdPedido.Parameters.AddWithValue(
                        "@id", pedido.Id);

                    int linhasAfetadas = cmdPedido.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                        throw new InvalidOperationException(
                            "O pedido não foi encontrado.");
                }

                using (var cmdExcluir = new NpgsqlCommand(@"
                    DELETE FROM item_pedido
                    WHERE id_pedido = @idPedido",
                    conn, transaction))
                {
                    cmdExcluir.Parameters.AddWithValue(
                        "@idPedido", pedido.Id);

                    cmdExcluir.ExecuteNonQuery();
                }

                foreach (var item in itensValidados)
                {
                    using var cmdItem = new NpgsqlCommand(@"
                        INSERT INTO item_pedido
                            (id_pedido, id_produto, quantidade, preco_unitario)
                        VALUES
                            (@idPedido, @idProduto, @quantidade, @preco)",
                        conn, transaction);

                    cmdItem.Parameters.AddWithValue(
                        "@idPedido", pedido.Id);

                    cmdItem.Parameters.AddWithValue(
                        "@idProduto", item.IdProduto);

                    cmdItem.Parameters.AddWithValue(
                        "@quantidade", item.Quantidade);

                    cmdItem.Parameters.AddWithValue(
                        "@preco", item.Preco);

                    cmdItem.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void ValidarPagamento(Pedido pedido)
        {
            if (string.IsNullOrWhiteSpace(pedido.FormaPagamento))
            {
                throw new InvalidOperationException(
                    "Selecione uma forma de pagamento.");
            }

            string forma = pedido.FormaPagamento.Trim();

            if (forma.Equals("Dinheiro", StringComparison.OrdinalIgnoreCase))
                pedido.FormaPagamento = "Dinheiro";
            else if (forma.Equals("Pix", StringComparison.OrdinalIgnoreCase))
                pedido.FormaPagamento = "Pix";
            else if (forma.Equals("Cartão de crédito", StringComparison.OrdinalIgnoreCase))
                pedido.FormaPagamento = "Cartão de crédito";
            else if (forma.Equals("Faturado", StringComparison.OrdinalIgnoreCase))
                pedido.FormaPagamento = "Faturado";
            else
                throw new InvalidOperationException(
                    "Forma de pagamento inválida.");

            if (pedido.FormaPagamento == "Cartão de crédito")
            {
                if (!pedido.Parcelas.HasValue ||
                    pedido.Parcelas.Value < 1 ||
                    pedido.Parcelas.Value > 6)
                {
                    throw new InvalidOperationException(
                        "Selecione uma quantidade de parcelas entre 1x e 6x.");
                }

                pedido.CondicaoPagamento = null;
            }
            else if (pedido.FormaPagamento == "Faturado")
            {
                pedido.Parcelas = null;

                if (string.IsNullOrWhiteSpace(pedido.CondicaoPagamento))
                {
                    throw new InvalidOperationException(
                        "Informe a condição de pagamento faturado.");
                }

                pedido.CondicaoPagamento =
                    pedido.CondicaoPagamento.Trim();
            }
            else
            {
                pedido.Parcelas = null;
                pedido.CondicaoPagamento = null;
            }
        }

        private static Pedido MapPedido(NpgsqlDataReader reader)
        {
            return new Pedido
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("id")),

                IdCliente = reader.IsDBNull(
                    reader.GetOrdinal("id_cliente"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("id_cliente")),

                ClienteNome = reader.IsDBNull(
                    reader.GetOrdinal("cliente_nome"))
                    ? "—"
                    : reader.GetString(
                        reader.GetOrdinal("cliente_nome")),

                DataPedido = reader.IsDBNull(
                    reader.GetOrdinal("data_pedido"))
                    ? DateTime.MinValue
                    : reader.GetDateTime(
                        reader.GetOrdinal("data_pedido")),

                Status = reader.IsDBNull(
                    reader.GetOrdinal("status"))
                    ? "Pendente"
                    : reader.GetString(
                        reader.GetOrdinal("status")),

                ValorTotal = reader.GetDecimal(
                    reader.GetOrdinal("valor_total")),

                FormaPagamento = reader.IsDBNull(
                    reader.GetOrdinal("forma_pagamento"))
                    ? string.Empty
                    : reader.GetString(
                        reader.GetOrdinal("forma_pagamento")),

                Parcelas = reader.IsDBNull(
                    reader.GetOrdinal("parcelas"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("parcelas")),

                CondicaoPagamento = reader.IsDBNull(
                    reader.GetOrdinal("condicao_pagamento"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("condicao_pagamento"))
            };
        }
    }
}
