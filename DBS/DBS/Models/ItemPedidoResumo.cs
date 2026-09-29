namespace DBS.Models
{
    /// <summary>Um produto dentro de um pedido (item_pedido + nome do produto).</summary>
    public class ItemPedidoResumo
    {
        public int IdPedido { get; set; }
        public int? IdProduto { get; set; }
        public string NomeProduto { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal => Quantidade * PrecoUnitario;
    }
}
