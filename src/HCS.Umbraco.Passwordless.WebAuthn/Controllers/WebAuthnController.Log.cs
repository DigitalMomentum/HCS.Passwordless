
using Microsoft.Extensions.Logging;

namespace HCS.Umbraco.Passwordless.WebAuthn.Controllers;

public partial class WebAuthnController
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Passwordless: An error occured during WebAuthn - {reference}")]
    private partial void LogError(Exception ex, Guid reference);
}