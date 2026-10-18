namespace DbsErpMobile.Models;

public class ItemPedido
{
    public int Id { get; set; }
    public int IdPedido { get; set; }
    public int IdProduto { get; set; }

    public string NomeProduto { get; set; }

    public string Imagem { get; set; }

    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
