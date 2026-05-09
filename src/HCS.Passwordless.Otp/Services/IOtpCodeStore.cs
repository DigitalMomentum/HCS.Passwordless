namespace HCS.Passwordless.Otp.Services;

public interface IOtpCodeStore
{
    Task SetAsync(string memberId, string purpose, byte[] hash, TimeSpan ttl, CancellationToken ct = default);
    Task<byte[]?> GetAsync(string memberId, string purpose, CancellationToken ct = default);
    Task DeleteAsync(string memberId, string purpose, CancellationToken ct = default);
}
