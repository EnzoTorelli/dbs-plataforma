using DbsErpMobile.Models;

namespace DbsErpMobile.Services;

public class PedidoService
{
    public Cliente Cliente { get; private set; }

    public string FormaPagamento { get; private set; }

    public int? QuantidadeParcelas { get; private set; }

    public void DefinirCliente(Cliente cliente)
    {
        Cliente = cliente;
    }

    public void DefinirPagamento(
        string formaPagamento,
        int? quantidadeParcelas = null)
    {
        FormaPagamento = formaPagamento;
        QuantidadeParcelas = quantidadeParcelas;
    }

    public void Limpar()
    {
        Cliente = null;
        FormaPagamento = null;
        QuantidadeParcelas = null;
    }
}
