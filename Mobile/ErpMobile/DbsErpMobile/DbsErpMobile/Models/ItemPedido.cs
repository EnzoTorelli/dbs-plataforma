namespace DbsErpMobile.Models;

public class ItemPedido
{
    public int Id { get; set; }
    public int IdPedido { get; set; }
    public int IdProduto { get; set; }
    public string NomeProduto { get; set; } // usado só na exibição do app, não vem do banco
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;
}