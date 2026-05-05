namespace HCS.Umbraco.Passwordless.Services;

public interface ISingleUseTokenStore
{
    /// <summary>Returns false if the token was already marked used (already present in store).</summary>
    Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default);
}
