namespace HCS.Umbraco.Passwordless.Otp.Services;

/// <summary>
/// Tracks per-member OTP attempt counts and enforces brute-force lockout.
/// </summary>
/// <remarks>
/// <para>
/// The default implementation (<c>InMemoryAttemptCounter</c>) is process-local.
/// On a single-node Umbraco deployment this is correct and race-free.
/// On a multi-node (load-balanced) deployment each instance maintains its own
/// counter — an attacker can spread guesses across nodes to exceed
/// <c>MaxAttempts</c> without triggering lockout on any single node.
/// </para>
/// <para>
/// <strong>Atomicity requirement:</strong> <see cref="IncrementAndCheckAsync"/>
/// must atomically increment and return the new count so that every concurrent
/// caller receives a unique value. Redis <c>INCR</c> and SQL <c>UPDATE … OUTPUT</c>
/// with a row-level lock are correct primitives.
/// </para>
/// <para>
/// Register a replacement via <c>OtpBuilder.UseAttemptCounter&lt;T&gt;()</c>.
/// See the <em>Multi-Instance Deployments</em> guide for complete Redis and SQL
/// Server reference implementations.
/// </para>
/// </remarks>
public interface IAttemptCounter
{
    /// <summary>
    /// Atomically increments the attempt counter for the given member and purpose,
    /// then returns the new count and whether the member is now locked out.
    /// </summary>
    Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose, int maxAttempts, TimeSpan lockDuration, CancellationToken ct = default);

    /// <summary>Resets the counter after a successful sign-in.</summary>
    Task ResetAsync(string memberId, string purpose, CancellationToken ct = default);
}
