using System.Globalization;
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

        CondicaoFaturamentoBorder.IsVisible = false;
        CondicaoFaturamentoEntry.Text = string.Empty;
    }

    private static string FormatarMoeda(decimal valor)
    {
        var cultura = CultureInfo.GetCultureInfo("pt-BR");
        return $"R$ {valor.ToString("#,##0.00", cultura)}";
    }

    private void AtualizarTotal()
    {
        TotalLabel.Text = FormatarMoeda(_carrinhoService.ValorTotal);

        AtualizarValorParcela();
    }

    private void AtualizarValorParcela()
    {
        if (ParcelasPicker.SelectedIndex < 0)
        {
            ValorParcelaLabel.Text = "Selecione as parcelas para ver o valor.";
            return;
        }

        int quantidade = ParcelasPicker.SelectedIndex + 1;
        decimal valorParcela = decimal.Round(
            _carrinhoService.ValorTotal / quantidade,
            2,
            MidpointRounding.AwayFromZero);

        ValorParcelaLabel.Text =
            $"{quantidade}x de {FormatarMoeda(valorParcela)}";
    }

    private void OnParcelasChanged(object sender, EventArgs e)
    {
        AtualizarValorParcela();
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
        CondicaoFaturamentoBorder.IsVisible = false;
    }

    private void OnPixClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Pix", PixBorder);
        OcultarParcelamento();
        CondicaoFaturamentoBorder.IsVisible = false;
    }

    private void OnFaturadoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Faturado", FaturadoBorder);
        OcultarParcelamento();
        CondicaoFaturamentoBorder.IsVisible = true;
    }

    private void OnCreditoClicked(object sender, TappedEventArgs e)
    {
        SelecionarFormaPagamento("Cartão de crédito", CreditoBorder);
        CondicaoFaturamentoBorder.IsVisible = false;
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

        string formaPagamentoFinal = _formaPagamento;

        if (_formaPagamento == "Faturado")
        {
            string condicao = CondicaoFaturamentoEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(condicao))
            {
                await DisplayAlertAsync(
                    "Condição de faturamento",
                    "Digite o prazo de faturamento. Exemplo: 30/60/90.",
                    "OK");

                return;
            }

            formaPagamentoFinal = $"Faturado ({condicao})";
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
            formaPagamentoFinal,
            _quantidadeParcelas);

        await Shell.Current.GoToAsync(nameof(RevisaoPedidoPage));
    }
}
