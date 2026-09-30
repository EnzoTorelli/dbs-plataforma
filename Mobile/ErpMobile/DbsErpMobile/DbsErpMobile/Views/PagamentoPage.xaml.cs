using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class PagamentoPage : ContentPage
{
    private readonly CarrinhoService _carrinhoService;
    private readonly PedidoService _pedidoService;

    private string _formaPagamento;
    private int? _quantidadeParcelas;

    public PagamentoPage(
        CarrinhoService carrinhoService,
        PedidoService pedidoService)
    {
        InitializeComponent();

        _carrinhoService = carrinhoService;
        _pedidoService = pedidoService;

        AtualizarTotal();
    }

    private void AtualizarTotal()
    {
        TotalLabel.Text = $"R$ {_carrinhoService.ValorTotal:F2}";
    }

    private void LimparSelecao()
    {
        PixBorder.Stroke = Color.FromArgb("#DCE3EC");
        DebitoBorder.Stroke = Color.FromArgb("#DCE3EC");
        CreditoBorder.Stroke = Color.FromArgb("#DCE3EC");
        FaturamentoBorder.Stroke = Color.FromArgb("#DCE3EC");
    }

    private void OnPixClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("PIX", PixBorder);

        ParcelamentoBorder.IsVisible = false;
        ParcelasPicker.SelectedIndex = -1;
        _quantidadeParcelas = null;
    }

    private void OnDebitoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Débito", DebitoBorder);

        ParcelamentoBorder.IsVisible = false;
        ParcelasPicker.SelectedIndex = -1;
        _quantidadeParcelas = null;
    }

    private void OnCreditoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Crédito", CreditoBorder);

        ParcelamentoBorder.IsVisible = true;
    }

    private void OnFaturamentoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Faturamento", FaturamentoBorder);

        ParcelamentoBorder.IsVisible = false;
        ParcelasPicker.SelectedIndex = -1;
        _quantidadeParcelas = null;
    }

    private void SelecionarFormaPagamento(string forma, Border border)
    {
        _formaPagamento = forma;

        LimparSelecao();

        border.Stroke = Color.FromArgb("#1E88E5");
        border.StrokeThickness = 2;
    }

    private async void OnContinuarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_formaPagamento))
        {
            await DisplayAlertAsync(
                "Forma de pagamento",
                "Selecione uma forma de pagamento para continuar.",
                "OK");

            return;
        }

        if (_formaPagamento == "Crédito")
        {
            if (ParcelasPicker.SelectedIndex < 0)
            {
                await DisplayAlertAsync(
                    "Parcelamento",
                    "Selecione a quantidade de parcelas.",
                    "OK");

                return;
            }

            _quantidadeParcelas = ParcelasPicker.SelectedIndex + 1;
        }

        _pedidoService.DefinirPagamento(
            _formaPagamento,
            _quantidadeParcelas);

        await Shell.Current.GoToAsync(nameof(RevisaoPedidoPage));
    }
}
