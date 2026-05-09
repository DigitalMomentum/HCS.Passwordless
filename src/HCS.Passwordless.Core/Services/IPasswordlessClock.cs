namespace HCS.Passwordless.Services;

/// <summary>Abstracts the system clock so that tests can control time.</summary>
public interface IPasswordlessClock
{
    /// <summary>The current UTC date and time.</summary>
    DateTimeOffset UtcNow { get; }
}

internal sealed class SystemClock : IPasswordlessClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
