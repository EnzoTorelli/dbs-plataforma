using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class RevisaoPedidoPage : ContentPage
{
    private readonly CarrinhoService _carrinhoService;
    private readonly ClienteService _clienteService;
    private readonly PedidoService _pedidoService;

    public RevisaoPedidoPage(
        CarrinhoService carrinhoService,
        ClienteService clienteService,
        PedidoService pedidoService)
    {
        InitializeComponent();

        _carrinhoService = carrinhoService;
        _clienteService = clienteService;
        _pedidoService = pedidoService;

        CarregarDados();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CarregarDados();
    }

    private void CarregarDados()
    {
        var cliente = _clienteService.ClienteSelecionado;

        if (cliente != null)
        {
            NomeClienteLabel.Text = cliente.Nome;
            CpfClienteLabel.Text = $"CPF/CNPJ: {cliente.Cpf}";
        }
        else
        {
            NomeClienteLabel.Text = "Nenhum cliente selecionado";
            CpfClienteLabel.Text = string.Empty;
        }

        ItensCollectionView.ItemsSource = _carrinhoService.Itens;
        TotalLabel.Text = $"R$ {_carrinhoService.ValorTotal:F2}";

        if (!string.IsNullOrWhiteSpace(_pedidoService.FormaPagamento))
        {
            string pagamento = _pedidoService.FormaPagamento;

            if (_pedidoService.QuantidadeParcelas.HasValue)
            {
                pagamento += $" em {_pedidoService.QuantidadeParcelas.Value}x";
            }

            PagamentoLabel.Text = pagamento;
        }
        else
        {
            PagamentoLabel.Text = "Não selecionado";
        }
    }

    private async void OnEnviarClicked(object sender, EventArgs e)
    {
        if (_carrinhoService.Itens.Count == 0)
        {
            await DisplayAlertAsync(
                "Carrinho vazio",
                "Adicione produtos antes de enviar o pedido.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(_pedidoService.FormaPagamento))
        {
            await DisplayAlertAsync(
                "Pagamento",
                "Selecione uma forma de pagamento antes de enviar.",
                "OK");

            return;
        }

        bool confirmar = await DisplayAlertAsync(
            "Enviar pedido",
            "Deseja realmente enviar este pedido para validação interna?",
            "Enviar",
            "Voltar");

        if (!confirmar)
            return;

        _pedidoService.RegistrarPedido(
            _clienteService.ClienteSelecionado,
            _carrinhoService.ValorTotal);

        _carrinhoService.Limpar();
        _clienteService.Limpar();
        _pedidoService.Limpar();

        await DisplayAlertAsync(
            "Pedido registrado",
            "Pedido registrado e encaminhado simbolicamente para validação interna. Você poderá consultá-lo em Vendas do dia.",
            "OK");

        await Shell.Current.GoToAsync("//VendasDia");
    }
}
