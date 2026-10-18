using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class PagamentoPage : ContentPage
{
    private readonly CarrinhoService _carrinhoService;
    private readonly PedidoService _pedidoService;

    private string? _formaPagamento;
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        AtualizarTotal();

        _formaPagamento = null;
        _quantidadeParcelas = null;

        LimparSelecao();
        ParcelamentoBorder.IsVisible = false;
        ParcelasPicker.SelectedIndex = -1;
    }

    private void AtualizarTotal()
    {
        TotalLabel.Text = $"R$ {_carrinhoService.ValorTotal:F2}";
    }

    private void LimparSelecao()
    {
        DinheiroBorder.Stroke = Color.FromArgb("#DCE3EC");
        PixBorder.Stroke = Color.FromArgb("#DCE3EC");
        FaturadoBorder.Stroke = Color.FromArgb("#DCE3EC");
        CreditoBorder.Stroke = Color.FromArgb("#DCE3EC");

        DinheiroBorder.StrokeThickness = 1;
        PixBorder.StrokeThickness = 1;
        FaturadoBorder.StrokeThickness = 1;
        CreditoBorder.StrokeThickness = 1;
    }

    private void SelecionarFormaPagamento(string forma, Border border)
    {
        _formaPagamento = forma;

        LimparSelecao();

        border.Stroke = Color.FromArgb("#1E88E5");
        border.StrokeThickness = 2;
    }

    private void OnDinheiroClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Dinheiro", DinheiroBorder);
        OcultarParcelamento();
    }

    private void OnPixClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Pix", PixBorder);
        OcultarParcelamento();
    }

    private void OnFaturadoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Faturado", FaturadoBorder);
        OcultarParcelamento();
    }

    private void OnCreditoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Cartão de crédito", CreditoBorder);
        ParcelamentoBorder.IsVisible = true;
    }

    private void OcultarParcelamento()
    {
        ParcelamentoBorder.IsVisible = false;
        ParcelasPicker.SelectedIndex = -1;
        _quantidadeParcelas = null;
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

        if (_formaPagamento == "Cartão de crédito")
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
