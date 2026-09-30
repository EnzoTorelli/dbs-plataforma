using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class CarrinhoPage : ContentPage
{
    private readonly CarrinhoService _carrinhoService;
    private readonly ClienteService _clienteService;

    public CarrinhoPage(
        CarrinhoService carrinhoService,
        ClienteService clienteService)
    {
        InitializeComponent();

        _carrinhoService = carrinhoService;
        _clienteService = clienteService;

        ItensCollectionView.ItemsSource = _carrinhoService.Itens;

        CarregarCliente();
        AtualizarTotal();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CarregarCliente();
        AtualizarTotal();
    }

    private void CarregarCliente()
    {
        var cliente = _clienteService.ClienteSelecionado;

        if (cliente == null)
        {
            NomeClienteLabel.Text = "Nenhum cliente selecionado";
            CpfClienteLabel.Text = string.Empty;
            return;
        }

        NomeClienteLabel.Text = cliente.Nome;
        CpfClienteLabel.Text = $"CPF/CNPJ: {cliente.Cpf}";
    }

    private void AtualizarTotal()
    {
        TotalLabel.Text = $"Total: R$ {_carrinhoService.ValorTotal:F2}";
    }

    private async void OnFinalizarClicked(object sender, EventArgs e)
    {
        if (_carrinhoService.Itens.Count == 0)
        {
            await DisplayAlertAsync(
                "Carrinho vazio",
                "Adicione produtos antes de finalizar.",
                "OK");

            return;
        }

        // TODO:
        // Aqui futuramente vamos abrir a etapa de pagamento
        // e envio para o vendedor interno.

        await DisplayAlertAsync(
            "Pedido enviado",
            "Seu pedido foi enviado com sucesso! (simulação)",
            "OK");

        _carrinhoService.Limpar();

        await Shell.Current.GoToAsync("..");
    }
}
