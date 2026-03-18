using Microsoft.Extensions.DependencyInjection;

namespace MauiBiller.Navigation;

public sealed class ServiceRouteFactory<TPage>(IServiceProvider serviceProvider) : RouteFactory
    where TPage : Element
{
    public override Element GetOrCreate()
    {
        return serviceProvider.GetRequiredService<TPage>();
    }

    public override Element GetOrCreate(IServiceProvider services)
    {
        return serviceProvider.GetRequiredService<TPage>();
    }
}
