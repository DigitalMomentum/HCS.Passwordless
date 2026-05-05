namespace HCS.Umbraco.Passwordless.Auth;

public interface IPasswordlessAuthFactor
{
    string Name { get; }
    bool IsEnabled { get; }
}
