namespace HCS.Umbraco.Passwordless.Services;

public interface IPasswordlessClock
{
    DateTimeOffset UtcNow { get; }
}

internal sealed class SystemClock : IPasswordlessClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
