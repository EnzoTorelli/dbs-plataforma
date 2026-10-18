using DbsErpMobile.Models;
using DbsErpMobile.ViewModels;

namespace DbsErpMobile.Views;

public partial class ProdutoListPage : ContentPage
{
    private readonly ProdutoListViewModel _viewModel;

    public ProdutoListPage()
    {
        InitializeComponent();

        _viewModel = new ProdutoListViewModel();

        ProdutosCollectionView.ItemsSource = _viewModel.Produtos;
    }

    private void OnPesquisarProduto(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(texto))
        {
            ProdutosCollectionView.ItemsSource = _viewModel.Produtos;
            return;
        }

        var produtosFiltrados = _viewModel.Produtos
            .Where(produto =>
                produto.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                produto.Id.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ProdutosCollectionView.ItemsSource = produtosFiltrados;
    }

    private async void OnProdutoTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Produto produto)
        {
            var parametros = new Dictionary<string, object>
            {
                { "produto", produto }
            };

            await Shell.Current.GoToAsync(nameof(ProdutoDetailPage), parametros);
        }
    }

    private async void OnCarrinhoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CarrinhoPage));
    }
}
