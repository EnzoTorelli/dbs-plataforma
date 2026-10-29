namespace DbsErpMobile.Models;

public class VendaDia
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
}
