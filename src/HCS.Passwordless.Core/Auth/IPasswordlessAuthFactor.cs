namespace HCS.Passwordless.Auth;

/// <summary>Reports whether a specific passwordless authentication factor is active.</summary>
public interface IPasswordlessAuthFactor
{
    /// <summary>Unique name identifying this authentication factor.</summary>
    string Name { get; }

    /// <summary><c>true</c> when this authentication factor is configured and enabled.</summary>
    bool IsEnabled { get; }
}
