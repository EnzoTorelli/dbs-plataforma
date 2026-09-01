using DbsErpMobile.Models;
using DbsErpMobile.ViewModels;

namespace DbsErpMobile.Views;

public partial class ProdutoListPage : ContentPage
{
    public ProdutoListPage()
    {
        InitializeComponent();
        var viewModel = new ProdutoListViewModel();
        ProdutosCollectionView.ItemsSource = viewModel.Produtos;
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