using System.Collections.ObjectModel;
using DbsErpMobile.Models;

namespace DbsErpMobile.Services;

public class PedidoService
{
    public Cliente Cliente { get; private set; }

    public string FormaPagamento { get; private set; }

    public int? QuantidadeParcelas { get; private set; }

    public ObservableCollection<VendaDia> Pedidos { get; } = new();

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

    public void RegistrarPedido(Cliente cliente, decimal valorTotal)
    {
        string pagamento = FormaPagamento ?? "Não informado";

        if (QuantidadeParcelas.HasValue)
        {
            pagamento += $" em {QuantidadeParcelas.Value}x";
        }

        Pedidos.Add(new VendaDia
        {
            Id = Pedidos.Count + 1,
            ClienteNome = cliente?.Nome ?? "Cliente não identificado",
            DataPedido = DateTime.Now,
            FormaPagamento = pagamento,
            Status = "Aguardando validação interna",
            ValorTotal = valorTotal
        });
    }

    public void Limpar()
    {
        Cliente = null;
        FormaPagamento = null;
        QuantidadeParcelas = null;
    }
}
