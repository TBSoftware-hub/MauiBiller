using MauiBiller.Configuration;

namespace MauiBiller.Extensions;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder RegisterAppConfiguration(this MauiAppBuilder builder)
    {
        var appConfiguration = AppConfigurationLoader.LoadCurrent();
        builder.Services.AddSingleton(appConfiguration);

        return builder;
    }

    public static MauiAppBuilder RegisterApplicationServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AppShell>();

        return builder;
    }
}
