using MauiBiller.Core.Services;

namespace MauiBiller.Infrastructure.Services;

public sealed class SecureValueStore : ISecureValueStore
{
    public Task SetAsync(string key, string value)
    {
        return SecureStorage.Default.SetAsync(key, value);
    }

    public Task<string?> GetAsync(string key)
    {
        return SecureStorage.Default.GetAsync(key);
    }

    public Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);
        return Task.CompletedTask;
    }
}
