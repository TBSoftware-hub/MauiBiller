using MauiBiller.Core.Services;
using MauiBiller.Navigation;

namespace MauiBiller.Infrastructure.Services;

public sealed class ShellNavigationService(IAuthSessionService authSessionService) : INavigationService
{
    public Task GoToAsync(string route)
    {
        var targetRoute = route;

        if (!authSessionService.IsAuthenticated && !AppRoutes.IsAuthenticationRoute(route))
        {
            targetRoute = AppRoutes.Login;
        }
        else if (authSessionService.IsAuthenticated && AppRoutes.IsAuthenticationRoute(route))
        {
            targetRoute = AppRoutes.Clients;
        }

        return Shell.Current.GoToAsync(AppRoutes.IsTopLevelRoute(targetRoute)
            ? AppRoutes.AsRoot(targetRoute)
            : targetRoute);
    }
}
