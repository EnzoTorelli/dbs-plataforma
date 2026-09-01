namespace DbsErpMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.ProdutoDetailPage), typeof(Views.ProdutoDetailPage));
            Routing.RegisterRoute(nameof(Views.CarrinhoPage), typeof(Views.CarrinhoPage));
        }
    }
}
