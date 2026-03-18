namespace MauiBiller.Core.Models;

public sealed record AuthenticatedUser(
    string UserId,
    string Email,
    string DisplayName);

public sealed record AuthSession(
    AuthenticatedUser User,
    string IdToken,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc);

public sealed record AuthenticationResult(
    bool IsSuccessful,
    string? ErrorMessage)
{
    public static AuthenticationResult Success() => new(true, null);

    public static AuthenticationResult Failure(string errorMessage) => new(false, errorMessage);
}

public sealed class AuthSessionChangedEventArgs(AuthSession? session) : EventArgs
{
    public AuthSession? Session
    {
        get;
    } = session;

    public AuthenticatedUser? User => Session?.User;

    public bool IsAuthenticated => Session is not null;
}
