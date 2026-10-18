using DbsErpMobile.Models;
using DbsErpMobile.Services;
using DbsErpMobile.ViewModels;

namespace DbsErpMobile.Views;

public partial class ClienteListPage : ContentPage
{
    private readonly ClienteListViewModel _viewModel;
    private readonly ClienteService _clienteService;
    private readonly PedidoService _pedidoService;

    public ClienteListPage(
        ClienteService clienteService,
        PedidoService pedidoService)
    {
        InitializeComponent();

        _clienteService = clienteService;
        _pedidoService = pedidoService;

        _viewModel = new ClienteListViewModel();

        ClientesCollectionView.ItemsSource = _viewModel.Clientes;
    }

    private void OnPesquisarCliente(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(texto))
        {
            ClientesCollectionView.ItemsSource = _viewModel.Clientes;
            return;
        }

        var clientesFiltrados = _viewModel.Clientes
            .Where(cliente =>
                cliente.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                cliente.Cpf.Contains(texto, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ClientesCollectionView.ItemsSource = clientesFiltrados;
    }

    private async void OnClienteTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Cliente cliente)
        {
            _clienteService.Selecionar(cliente);
            _pedidoService.DefinirCliente(cliente);

            await Shell.Current.GoToAsync(nameof(NovoPedidoPage));
        }
    }
}
