using MauiBiller.Core.Models;

namespace MauiBiller.Core.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthenticationResult> RegisterAsync(string displayName, string email, string password, CancellationToken cancellationToken = default);

    Task<AuthenticationResult> SendPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}

public interface IAuthSessionService
{
    bool IsAuthenticated
    {
        get;
    }

    AuthenticatedUser? CurrentUser
    {
        get;
    }

    event EventHandler<AuthSessionChangedEventArgs>? SessionChanged;

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<string?> GetIdTokenAsync(CancellationToken cancellationToken = default);
}

public interface ISecureValueStore
{
    Task SetAsync(string key, string value);

    Task<string?> GetAsync(string key);

    Task RemoveAsync(string key);
}
