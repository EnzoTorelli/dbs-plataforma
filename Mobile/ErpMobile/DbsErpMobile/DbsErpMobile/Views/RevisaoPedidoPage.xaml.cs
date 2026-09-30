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
                pagamento += $" em {_pedidoService.QuantidadeParcelas}x";
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
        bool confirmar = await DisplayAlertAsync(
            "Enviar pedido",
            "Deseja realmente enviar este pedido para validação?",
            "Enviar",
            "Voltar");

        if (!confirmar)
        {
            return;
        }

        await DisplayAlertAsync(
            "Pedido pronto",
            "O pedido foi preparado para envio e deverá ser validado pelo responsável interno.",
            "OK");
    }
}
