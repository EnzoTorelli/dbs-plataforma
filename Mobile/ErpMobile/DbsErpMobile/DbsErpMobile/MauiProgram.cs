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
            builder.Services.AddTransient<Views.ProdutoDetailPage>();
            builder.Services.AddTransient<Views.CarrinhoPage>();

            return builder.Build();
        }
    }
}