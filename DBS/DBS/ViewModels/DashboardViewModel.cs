using DBS.Models;

namespace DBS.ViewModels
{
    public record PontoDiario(DateTime Dia, decimal Receita, int Pedidos);
    public record ProdutoVendido(string Nome, int Quantidade, decimal Receita);
    public record StatusResumo(string Status, int Quantidade, decimal Valor);

    public class DashboardViewModel
    {
        // Receita (só pedidos Pagos)
        public decimal ReceitaMes { get; set; }
        public int PedidosPagosMes { get; set; }
        public decimal ReceitaMesAnteriorParcial { get; set; }

        public decimal TicketMedio => PedidosPagosMes > 0 ? ReceitaMes / PedidosPagosMes : 0;

        /// <summary>% vs mesmos dias do mês anterior. Null quando não há base de comparação.</summary>
        public decimal? VariacaoReceitaPct => ReceitaMesAnteriorParcial > 0
            ? (ReceitaMes - ReceitaMesAnteriorParcial) / ReceitaMesAnteriorParcial * 100
            : null;

        // Clientes
        public int TotalClientes { get; set; }
        public int ClientesNovosMes { get; set; }

        // Ordens
        public int TotalOrdensAbertas { get; set; }

        // Estoque
        public int TotalProdutos { get; set; }
        public int TotalUnidadesEstoque { get; set; }
        public int ProdutosEstoqueBaixo { get; set; }
        public List<Produto> EstoqueBaixo { get; set; } = new();

        // Listas
        public List<PontoDiario> ReceitaDiariaMes { get; set; } = new();
        public List<Pedido> UltimasOrdens { get; set; } = new();

        public DateTime AtualizadoEm { get; set; } = DateTime.Now;
    }
}
