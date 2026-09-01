using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class CarrinhoPage : ContentPage
{
    private readonly CarrinhoService _carrinhoService;

    public CarrinhoPage(CarrinhoService carrinhoService)
    {
        InitializeComponent();
        _carrinhoService = carrinhoService;
        ItensCollectionView.ItemsSource = _carrinhoService.Itens;
        AtualizarTotal();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AtualizarTotal(); // recalcula toda vez que a tela reabre, caso o carrinho tenha mudado
    }

    private void AtualizarTotal()
    {
        TotalLabel.Text = $"Total: R$ {_carrinhoService.ValorTotal:F2}";
    }

    private async void OnFinalizarClicked(object sender, EventArgs e)
    {
        if (_carrinhoService.Itens.Count == 0)
        {
            await DisplayAlertAsync("Carrinho vazio", "Adicione produtos antes de finalizar.", "OK");
            return;
        }

        // TODO: aqui vai a chamada real pra API/Supabase quando estiver definida
        await DisplayAlertAsync("Pedido enviado", "Seu pedido foi enviado com sucesso! (simulação)", "OK");
        _carrinhoService.Limpar();
        await Shell.Current.GoToAsync("..");
    }
}