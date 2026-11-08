namespace DBS.Models;

/// <summary>Um item do carrinho enviado pelo app. O preço NÃO vem do app: o servidor lê do banco.</summary>
public sealed record ItemPedidoRequest(int IdProduto, int Quantidade);

/// <summary>Corpo do POST api/pedidos.</summary>
public sealed record NovoPedidoRequest(
    int IdCliente,
    string FormaPagamento,
    int? Parcelas,
    List<ItemPedidoRequest> Itens);

/// <summary>Resposta do POST api/pedidos.</summary>
public sealed record PedidoCriadoResponse(int Id, decimal ValorTotal, string Status);

/// <summary>Linha do GET api/pedidos/hoje (formato parecido com VendaDia do app).</summary>
public sealed record PedidoVendedorResumo(
    int Id,
    string ClienteNome,
    DateTime DataPedido,
    string? FormaPagamento,
    int? Parcelas,
    string Status,
    decimal ValorTotal);

/// <summary>Erro de regra de negócio (produto sem estoque, cliente inexistente...). Vira HTTP 409.</summary>
public sealed class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
