using DBS.Models;
using Npgsql;

namespace DBS.Repositories;

/// <summary>
/// Operações de pedido usadas pelo app do vendedor (API).
/// Fica separado do PedidoRepository (web) para não mexer no que já funciona.
/// </summary>
public sealed class PedidoAppRepository
{
    private readonly string _connectionString;

    public PedidoAppRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    /// <summary>
    /// Cria pedido + itens e baixa o estoque, tudo em UMA transação.
    /// Se qualquer passo falhar, nada é gravado.
    /// </summary>
    public async Task<PedidoCriadoResponse> CriarComItensAsync(int idVendedor, NovoPedidoRequest req)
    {
        // Junta linhas repetidas do mesmo produto e ordena por id
        // (ordem fixa evita deadlock quando dois pedidos disputam os mesmos produtos).
        var itens = req.Itens
            .GroupBy(i => i.IdProduto)
            .Select(g => new ItemPedidoRequest(g.Key, g.Sum(x => x.Quantidade)))
            .OrderBy(i => i.IdProduto)
            .ToList();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        // 1) Cliente existe?
        await using (var cmdCli = new NpgsqlCommand("SELECT 1 FROM cliente WHERE id = @id", conn, tx))
        {
            cmdCli.Parameters.AddWithValue("id", req.IdCliente);
            if (await cmdCli.ExecuteScalarAsync() is null)
                throw new RegraNegocioException("Cliente não encontrado.");
        }

        // 2) Baixa estoque e pega o preço atual de cada produto.
        //    O UPDATE só acontece se houver estoque suficiente, de forma atômica.
        var linhas = new List<(int IdProduto, int Qtd, decimal Preco)>();
        foreach (var item in itens)
        {
            await using var cmd = new NpgsqlCommand(@"
                UPDATE produto
                   SET estoque = estoque - @q
                 WHERE id = @id AND estoque >= @q
             RETURNING preco", conn, tx);
            cmd.Parameters.AddWithValue("q", item.Quantidade);
            cmd.Parameters.AddWithValue("id", item.IdProduto);

            var preco = await cmd.ExecuteScalarAsync();
            if (preco is null)
                throw new RegraNegocioException(
                    $"Produto {item.IdProduto} não existe ou não tem estoque suficiente.");

            linhas.Add((item.IdProduto, item.Quantidade, (decimal)preco));
        }

        var total = linhas.Sum(l => l.Qtd * l.Preco);

        // 3) Pedido (nasce "Pendente": a validação interna vira "Pago" pelo sistema web)
        int idPedido;
        await using (var cmdPed = new NpgsqlCommand(@"
            INSERT INTO pedido (id_cliente, id_vendedor, status, valor, forma_pagamento, parcelas)
            VALUES (@cliente, @vendedor, 'Pendente', @valor, @forma, @parcelas)
            RETURNING id", conn, tx))
        {
            cmdPed.Parameters.AddWithValue("cliente", req.IdCliente);
            cmdPed.Parameters.AddWithValue("vendedor", idVendedor);
            cmdPed.Parameters.AddWithValue("valor", total);
            cmdPed.Parameters.AddWithValue("forma", req.FormaPagamento.Trim());
            cmdPed.Parameters.AddWithValue("parcelas", (object?)req.Parcelas ?? DBNull.Value);
            idPedido = (int)(await cmdPed.ExecuteScalarAsync())!;
        }

        // 4) Itens
        foreach (var l in linhas)
        {
            await using var cmdIt = new NpgsqlCommand(@"
                INSERT INTO item_pedido (id_pedido, id_produto, quantidade, preco_unitario)
                VALUES (@pedido, @produto, @qtd, @preco)", conn, tx);
            cmdIt.Parameters.AddWithValue("pedido", idPedido);
            cmdIt.Parameters.AddWithValue("produto", l.IdProduto);
            cmdIt.Parameters.AddWithValue("qtd", l.Qtd);
            cmdIt.Parameters.AddWithValue("preco", l.Preco);
            await cmdIt.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
        return new PedidoCriadoResponse(idPedido, total, "Pendente");
    }

    /// <summary>Pedidos de hoje do vendedor logado (tela "Vendas do dia" do app).</summary>
    public async Task<List<PedidoVendedorResumo>> ListarDoDiaAsync(int idVendedor)
    {
        var lista = new List<PedidoVendedorResumo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(@"
            SELECT p.id,
                   COALESCE(c.nome, '—'),
                   p.data_pedido,
                   p.forma_pagamento,
                   p.parcelas,
                   COALESCE(p.status, 'Pendente'),
                   p.valor
              FROM pedido p
              LEFT JOIN cliente c ON c.id = p.id_cliente
             WHERE p.id_vendedor = @v
               AND p.data_pedido >= CURRENT_DATE
               AND p.data_pedido <  CURRENT_DATE + 1
             ORDER BY p.data_pedido DESC", conn);
        cmd.Parameters.AddWithValue("v", idVendedor);

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new PedidoVendedorResumo(
                Id: r.GetInt32(0),
                ClienteNome: r.GetString(1),
                DataPedido: r.GetDateTime(2),
                FormaPagamento: r.IsDBNull(3) ? null : r.GetString(3),
                Parcelas: r.IsDBNull(4) ? null : r.GetInt32(4),
                Status: r.GetString(5),
                ValorTotal: r.GetDecimal(6)));
        }
        return lista;
    }
}
