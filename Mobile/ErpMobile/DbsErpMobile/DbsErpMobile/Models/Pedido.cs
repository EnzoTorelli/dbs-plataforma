namespace DbsErpMobile.Models;

public class Pedido
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public DateTime DataPedido { get; set; }
    public string Status { get; set; }
    public List<ItemPedido> Itens { get; set; } = new();

    // calculado no app a partir dos itens, não vem do banco
    public decimal ValorTotal => Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
}