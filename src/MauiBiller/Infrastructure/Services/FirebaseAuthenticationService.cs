using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MauiBiller.Configuration;
using MauiBiller.Core.Models;
using MauiBiller.Core.Services;

namespace MauiBiller.Infrastructure.Services;

public sealed class FirebaseAuthenticationService(
    HttpClient httpClient,
    AppConfiguration appConfiguration,
    ISecureValueStore secureValueStore) : IAuthenticationService, IAuthSessionService
{
    private const string IdentityToolkitBaseUrl = "https://identitytoolkit.googleapis.com/v1";
    private const string SecureTokenBaseUrl = "https://securetoken.googleapis.com/v1";
    private const string SessionStorageKey = "mauibiller.firebase.session";
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim sessionLock = new(1, 1);

    private AuthSession? currentSession;
    private bool isInitialized;

    public bool IsAuthenticated => currentSession is not null;

    public AuthenticatedUser? CurrentUser => currentSession?.User;

    public event EventHandler<AuthSessionChangedEventArgs>? SessionChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken);

        try
        {
            if (isInitialized)
            {
                return;
            }

            currentSession = await LoadStoredSessionAsync();

            if (currentSession is not null && SessionNeedsRefresh(currentSession))
            {
                var refreshSucceeded = await TryRefreshCurrentSessionAsync(cancellationToken);

                if (!refreshSucceeded)
                {
                    await ClearSessionAsync(raiseEvent: false);
                }
            }

            isInitialized = true;
        }
        finally
        {
            sessionLock.Release();
        }

        RaiseSessionChanged();
    }

    public async Task<string?> GetIdTokenAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await sessionLock.WaitAsync(cancellationToken);

        try
        {
            if (currentSession is null)
            {
                return null;
            }

            if (SessionNeedsRefresh(currentSession))
            {
                var refreshSucceeded = await TryRefreshCurrentSessionAsync(cancellationToken);

                if (!refreshSucceeded)
                {
                    await ClearSessionAsync();
                    return null;
                }
            }

            return currentSession?.IdToken;
        }
        finally
        {
            sessionLock.Release();
        }
    }

    public async Task<AuthenticationResult> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthenticationResult.Failure("Enter your email address.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResult.Failure("Enter your password.");
        }

        if (!TryGetApiKey(out var apiKey, out var configurationError))
        {
            return AuthenticationResult.Failure(configurationError);
        }

        var request = new EmailPasswordRequest(email.Trim(), password);
        var response = await PostAsync<EmailPasswordRequest, FirebaseAuthResponse>(
            $"{IdentityToolkitBaseUrl}/accounts:signInWithPassword?key={apiKey}",
            request,
            cancellationToken);

        if (!response.IsSuccessful)
        {
            return AuthenticationResult.Failure(response.ErrorMessage!);
        }

        await PersistSessionAsync(response.Payload!.ToSession(string.Empty));

        return AuthenticationResult.Success();
    }

    public async Task<AuthenticationResult> RegisterAsync(string displayName, string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return AuthenticationResult.Failure("Enter your name.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthenticationResult.Failure("Enter your email address.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResult.Failure("Enter a password.");
        }

        if (!TryGetApiKey(out var apiKey, out var configurationError))
        {
            return AuthenticationResult.Failure(configurationError);
        }

        var request = new EmailPasswordRequest(email.Trim(), password);
        var response = await PostAsync<EmailPasswordRequest, FirebaseAuthResponse>(
            $"{IdentityToolkitBaseUrl}/accounts:signUp?key={apiKey}",
            request,
            cancellationToken);

        if (!response.IsSuccessful)
        {
            return AuthenticationResult.Failure(response.ErrorMessage!);
        }

        var session = response.Payload!.ToSession(displayName.Trim());

        var updateProfileResponse = await PostAsync<UpdateProfileRequest, FirebaseProfileResponse>(
            $"{IdentityToolkitBaseUrl}/accounts:update?key={apiKey}",
            new UpdateProfileRequest(session.IdToken, session.User.DisplayName),
            cancellationToken);

        if (updateProfileResponse.IsSuccessful && updateProfileResponse.Payload is not null)
        {
            session = new AuthSession(
                session.User with
                {
                    DisplayName = updateProfileResponse.Payload.DisplayName ?? session.User.DisplayName
                },
                session.IdToken,
                session.RefreshToken,
                session.ExpiresAtUtc);
        }

        await PersistSessionAsync(session);

        return AuthenticationResult.Success();
    }

    public async Task<AuthenticationResult> SendPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthenticationResult.Failure("Enter your email address.");
        }

        if (!TryGetApiKey(out var apiKey, out var configurationError))
        {
            return AuthenticationResult.Failure(configurationError);
        }

        var response = await PostAsync<PasswordResetRequest, FirebasePasswordResetResponse>(
            $"{IdentityToolkitBaseUrl}/accounts:sendOobCode?key={apiKey}",
            new PasswordResetRequest(email.Trim()),
            cancellationToken);

        return response.IsSuccessful
            ? AuthenticationResult.Success()
            : AuthenticationResult.Failure(response.ErrorMessage!);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await sessionLock.WaitAsync(cancellationToken);

        try
        {
            await ClearSessionAsync();
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (!isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }
    }

    private async Task PersistSessionAsync(AuthSession session)
    {
        currentSession = session;
        var serializedSession = JsonSerializer.Serialize(StoredSession.FromSession(session), serializerOptions);
        await secureValueStore.SetAsync(SessionStorageKey, serializedSession);
        RaiseSessionChanged();
    }

    private async Task<AuthSession?> LoadStoredSessionAsync()
    {
        var serializedSession = await secureValueStore.GetAsync(SessionStorageKey);

        if (string.IsNullOrWhiteSpace(serializedSession))
        {
            return null;
        }

        var storedSession = JsonSerializer.Deserialize<StoredSession>(serializedSession, serializerOptions);

        return storedSession?.ToSession();
    }

    private async Task ClearSessionAsync(bool raiseEvent = true)
    {
        currentSession = null;
        await secureValueStore.RemoveAsync(SessionStorageKey);

        if (raiseEvent)
        {
            RaiseSessionChanged();
        }
    }

    private bool SessionNeedsRefresh(AuthSession session)
    {
        return session.ExpiresAtUtc <= DateTimeOffset.UtcNow.AddMinutes(1);
    }

    private async Task<bool> TryRefreshCurrentSessionAsync(CancellationToken cancellationToken)
    {
        if (currentSession is null)
        {
            return false;
        }

        if (!TryGetApiKey(out var apiKey, out _))
        {
            return false;
        }

        var formContent = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", currentSession.RefreshToken)
        ]);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{SecureTokenBaseUrl}/token?key={apiKey}")
        {
            Content = formContent
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<FirebaseRefreshResponse>(serializerOptions, cancellationToken);

        if (!response.IsSuccessStatusCode || payload is null || string.IsNullOrWhiteSpace(payload.IdToken))
        {
            return false;
        }

        currentSession = new AuthSession(
            currentSession.User,
            payload.IdToken,
            payload.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(ParseLifetimeSeconds(payload.ExpiresIn)));

        var serializedSession = JsonSerializer.Serialize(StoredSession.FromSession(currentSession), serializerOptions);
        await secureValueStore.SetAsync(SessionStorageKey, serializedSession);
        RaiseSessionChanged();

        return true;
    }

    private bool TryGetApiKey(out string apiKey, out string errorMessage)
    {
        apiKey = appConfiguration.Firebase.ApiKey.Trim();

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = "Firebase Auth is not configured yet. Add Firebase:ApiKey to appsettings before signing in.";
        return false;
    }

    private void RaiseSessionChanged()
    {
        SessionChanged?.Invoke(this, new AuthSessionChangedEventArgs(currentSession));
    }

    private static int ParseLifetimeSeconds(string? expiresIn)
    {
        return int.TryParse(expiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds)
            ? seconds
            : 3600;
    }

    private async Task<FirebaseResult<TResponse>> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class
    {
        using var response = await httpClient.PostAsJsonAsync(requestUri, request, serializerOptions, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var successPayload = await response.Content.ReadFromJsonAsync<TResponse>(serializerOptions, cancellationToken);

            if (successPayload is null)
            {
                return FirebaseResult<TResponse>.Failure("Firebase returned an empty response.");
            }

            return FirebaseResult<TResponse>.Success(successPayload);
        }

        var errorPayload = await response.Content.ReadFromJsonAsync<FirebaseErrorEnvelope>(serializerOptions, cancellationToken);
        return FirebaseResult<TResponse>.Failure(MapFirebaseError(errorPayload?.Error?.Message));
    }

    private static string MapFirebaseError(string? errorCode)
    {
        return errorCode switch
        {
            "EMAIL_EXISTS" => "That email address is already registered.",
            "EMAIL_NOT_FOUND" => "We could not find an account with that email address.",
            "INVALID_PASSWORD" => "The password is incorrect.",
            "USER_DISABLED" => "This account has been disabled.",
            "INVALID_EMAIL" => "Enter a valid email address.",
            "WEAK_PASSWORD : Password should be at least 6 characters" => "Choose a password with at least 6 characters.",
            "MISSING_PASSWORD" => "Enter your password.",
            "OPERATION_NOT_ALLOWED" => "Email/password sign-in is not enabled for this Firebase project.",
            _ when !string.IsNullOrWhiteSpace(errorCode) => $"Firebase Auth error: {errorCode}.",
            _ => "Firebase Auth request failed."
        };
    }

    private sealed record EmailPasswordRequest(
        string Email,
        string Password,
        bool ReturnSecureToken = true);

    private sealed record UpdateProfileRequest(
        string IdToken,
        string DisplayName,
        bool ReturnSecureToken = true);

    private sealed record PasswordResetRequest(
        string Email,
        string RequestType = "PASSWORD_RESET");

    private sealed record FirebaseAuthResponse(
        string LocalId,
        string Email,
        string IdToken,
        string RefreshToken,
        string ExpiresIn,
        string? DisplayName)
    {
        public AuthSession ToSession(string fallbackDisplayName)
        {
            return new AuthSession(
                new AuthenticatedUser(
                    LocalId,
                    Email,
                    string.IsNullOrWhiteSpace(DisplayName) ? fallbackDisplayName : DisplayName),
                IdToken,
                RefreshToken,
                DateTimeOffset.UtcNow.AddSeconds(ParseLifetimeSeconds(ExpiresIn)));
        }
    }

    private sealed record FirebaseProfileResponse(string? DisplayName);

    private sealed record FirebasePasswordResetResponse(string Email);

    private sealed record FirebaseRefreshResponse(
        string ExpiresIn,
        string RefreshToken,
        string IdToken);

    private sealed record FirebaseErrorEnvelope(FirebaseError? Error);

    private sealed record FirebaseError(string? Message);

    private sealed record FirebaseResult<T>(bool IsSuccessful, T? Payload, string? ErrorMessage)
        where T : class
    {
        public static FirebaseResult<T> Success(T payload) => new(true, payload, null);

        public static FirebaseResult<T> Failure(string errorMessage) => new(false, null, errorMessage);
    }

    private sealed record StoredSession(
        string UserId,
        string Email,
        string DisplayName,
        string IdToken,
        string RefreshToken,
        DateTimeOffset ExpiresAtUtc)
    {
        public AuthSession ToSession()
        {
            return new AuthSession(
                new AuthenticatedUser(UserId, Email, DisplayName),
                IdToken,
                RefreshToken,
                ExpiresAtUtc);
        }

        public static StoredSession FromSession(AuthSession session)
        {
            return new StoredSession(
                session.User.UserId,
                session.User.Email,
                session.User.DisplayName,
                session.IdToken,
                session.RefreshToken,
                session.ExpiresAtUtc);
        }
    }
}
