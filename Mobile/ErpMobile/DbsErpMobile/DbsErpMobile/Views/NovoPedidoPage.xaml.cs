using DbsErpMobile.Services;

namespace DbsErpMobile.Views;

public partial class NovoPedidoPage : ContentPage
{
    private readonly ClienteService _clienteService;

    public NovoPedidoPage(ClienteService clienteService)
    {
        InitializeComponent();

        _clienteService = clienteService;

        CarregarCliente();
    }

    private void CarregarCliente()
    {
        var cliente = _clienteService.ClienteSelecionado;

        if (cliente == null)
        {
            NomeClienteLabel.Text = "Nenhum cliente selecionado";
            CpfClienteLabel.Text = string.Empty;
            TelefoneClienteLabel.Text = string.Empty;
            return;
        }

        NomeClienteLabel.Text = cliente.Nome;
        CpfClienteLabel.Text = $"CPF/CNPJ: {cliente.Cpf}";
        TelefoneClienteLabel.Text = $"Telefone: {cliente.Telefone}";
    }

    private async void OnAdicionarProdutosClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProdutoListPage));
    }
}
