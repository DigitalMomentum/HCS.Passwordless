namespace HCS.Umbraco.Passwordless.Otp.Services;

public interface IAttemptCounter
{
    Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose, int maxAttempts, TimeSpan lockDuration, CancellationToken ct = default);

    Task ResetAsync(string memberId, string purpose, CancellationToken ct = default);
}
