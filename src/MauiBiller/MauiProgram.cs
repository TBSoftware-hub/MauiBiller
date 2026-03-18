using MauiBiller.Extensions;
using Microsoft.Extensions.Logging;

namespace MauiBiller;

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
            })
            .RegisterAppConfiguration()
            .RegisterApplicationServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
