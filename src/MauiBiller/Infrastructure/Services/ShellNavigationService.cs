using MauiBiller.Core.Services;

namespace MauiBiller.Infrastructure.Services;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
}
