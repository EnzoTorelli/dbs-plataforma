namespace DBS.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int? IdCliente { get; set; }
        public string? ClienteNome { get; set; }
        public DateTime DataPedido { get; set; }
        public string? Status { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal Valor { get; set; }

        /// <summary>Produtos do pedido. Preenchido por PedidoRepository.CarregarItens().</summary>
        public List<ItemPedidoResumo> Itens { get; set; } = new();

        public int TotalUnidades => Itens.Sum(i => i.Quantidade);

        /// <summary>Ex.: "2× RTX 3060, 1× Mouse Gamer REDRAGON +1"</summary>
        public string ResumoItens(int maxProdutos = 2)
        {
            if (Itens.Count == 0) return "Sem itens registrados";
            var partes = Itens.Take(maxProdutos).Select(i => $"{i.Quantidade}× {i.NomeProduto}");
            var resto = Itens.Count - maxProdutos;
            return string.Join(", ", partes) + (resto > 0 ? $" +{resto}" : "");
        }
    }
}
