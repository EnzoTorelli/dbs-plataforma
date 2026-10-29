using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class VendasDiaPage : ContentPage
{
    private readonly PedidoService _pedidoService;

    public VendasDiaPage(PedidoService pedidoService)
    {
        InitializeComponent();
        _pedidoService = pedidoService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var vendasDeHoje = _pedidoService.Pedidos
            .Where(p => p.DataPedido.Date == DateTime.Today)
            .OrderByDescending(p => p.DataPedido)
            .ToList();

        VendasCollectionView.ItemsSource = vendasDeHoje;

        ResumoLabel.Text = vendasDeHoje.Count == 0
            ? "Nenhum pedido registrado hoje."
            : $"{vendasDeHoje.Count} pedido(s) registrado(s) hoje.";
    }
}
