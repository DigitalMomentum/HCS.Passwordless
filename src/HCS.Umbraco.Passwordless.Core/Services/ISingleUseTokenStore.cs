namespace HCS.Umbraco.Passwordless.Services;

/// <summary>
/// Tracks which tokens have already been consumed so that magic-link tokens
/// cannot be replayed after first use.
/// </summary>
/// <remarks>
/// <para>
/// The default implementation (<c>InMemorySingleUseTokenStore</c>) is process-local.
/// On a single-node Umbraco deployment this is correct and race-free.
/// On a multi-node (load-balanced) deployment each instance has its own in-process
/// store — the same token could be accepted twice if two requests reach different nodes.
/// </para>
/// <para>
/// <strong>Atomicity requirement:</strong> <see cref="TryMarkUsedAsync"/> must be an
/// atomic check-and-set. A read followed by a separate write is not safe because a
/// concurrent caller can pass the check before the first caller's write commits.
/// Redis <c>SET NX EX</c> and SQL unique-constraint <c>INSERT</c> are correct
/// primitives; <c>IDistributedCache</c> is not (its Get+Set is non-atomic).
/// </para>
/// <para>
/// Register a replacement via <c>MagicLinkBuilder.UseSingleUseTokenStore&lt;T&gt;()</c>.
/// See the <em>Multi-Instance Deployments</em> guide for complete Redis and SQL
/// Server reference implementations.
/// </para>
/// </remarks>
public interface ISingleUseTokenStore
{
    /// <summary>
    /// Atomically marks <paramref name="tokenHash"/> as used.
    /// Returns <c>true</c> if this is the first call for this hash (token is valid);
    /// returns <c>false</c> if the hash was already present (token was already consumed).
    /// </summary>
    Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default);
}
