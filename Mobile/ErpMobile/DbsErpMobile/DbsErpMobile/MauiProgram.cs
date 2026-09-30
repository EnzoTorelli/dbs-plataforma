using DbsErpMobile.Services;
using Microsoft.Extensions.Logging;

namespace DbsErpMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<CarrinhoService>();
            builder.Services.AddSingleton<ClienteService>();
            builder.Services.AddSingleton<PedidoService>();
            builder.Services.AddTransient<Views.ProdutoDetailPage>();
            builder.Services.AddTransient<Views.CarrinhoPage>();
            builder.Services.AddTransient<Views.NovoPedidoPage>();
            builder.Services.AddTransient<Views.PagamentoPage>();
            builder.Services.AddTransient<Views.RevisaoPedidoPage>();

            return builder.Build();
        }
    }
}