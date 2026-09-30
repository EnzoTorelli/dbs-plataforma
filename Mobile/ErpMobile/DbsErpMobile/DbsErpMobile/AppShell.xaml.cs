namespace DbsErpMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(Views.ProdutoDetailPage),
            typeof(Views.ProdutoDetailPage));

        Routing.RegisterRoute(
            nameof(Views.CarrinhoPage),
            typeof(Views.CarrinhoPage));

        Routing.RegisterRoute(
            nameof(Views.NovoPedidoPage),
            typeof(Views.NovoPedidoPage));

        Routing.RegisterRoute(
            nameof(Views.PagamentoPage),
            typeof(Views.PagamentoPage));
    }
}
