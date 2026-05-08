
using Microsoft.Extensions.Logging;

namespace HCS.Umbraco.Passwordless.WebAuthn.Controllers;

public partial class WebAuthnController
{
    [LoggerMessage(Level = LogLevel.Error, Message = "WebAuthn: unhandled exception - {reference}")]
    private partial void LogError(Exception ex, Guid reference);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WebAuthn RegisterOptions: member={memberKey} excluding {excludeCount} existing credential(s)")]
    private partial void LogRegisterOptionsStarted(Guid memberKey, int excludeCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "WebAuthn RegisterOptions: ceremony created for member={memberKey}")]
    private partial void LogRegisterOptionsCeremonyCreated(Guid memberKey);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn RegisterComplete: invalid or expired ceremony id={ceremonyId} (state null or member key mismatch)")]
    private partial void LogRegisterCompleteInvalidCeremony(string ceremonyId);

    [LoggerMessage(Level = LogLevel.Information, Message = "WebAuthn RegisterComplete: credential saved for member={memberKey} aaGuid={aaGuid} format={format}")]
    private partial void LogRegisterCredentialSaved(Guid memberKey, Guid aaGuid, string? format);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WebAuthn SignInOptions: hasEmail={hasEmail} memberFound={memberFound} credentialCount={credentialCount} isDecoy={isDecoy}")]
    private partial void LogSignInOptionsCreated(bool hasEmail, bool memberFound, int credentialCount, bool isDecoy);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: rate limited ip={ip}")]
    private partial void LogSignInRateLimited(string ip);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: ceremony not found or expired ceremonyId={ceremonyId}")]
    private partial void LogSignInCeremonyNotFound(string ceremonyId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: decoy ceremony detected — returning 401")]
    private partial void LogSignInDecoy();

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: credential not found by rawId")]
    private partial void LogSignInCredentialNotFound();

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: member not found (memberKey={memberKey} hasUserHandle={hasUserHandle})")]
    private partial void LogSignInMemberNotFound(Guid? memberKey, bool hasUserHandle);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: member key mismatch (state memberKey={stateMemberKey} actual memberKey={actualMemberKey})")]
    private partial void LogSignInMemberKeyMismatch(Guid stateMemberKey, Guid actualMemberKey);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: credential belongs to different member (credential memberKey={credentialMemberKey} authed memberKey={authenticatedMemberKey})")]
    private partial void LogSignInCredentialOwnerMismatch(Guid credentialMemberKey, Guid authenticatedMemberKey);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebAuthn SignInComplete: counter regression (stored={stored} received={received}) memberKey={memberKey}")]
    private partial void LogSignInCounterRegression(long stored, uint received, Guid memberKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "WebAuthn SignInComplete: sign-in successful for member={memberKey}")]
    private partial void LogSignInSuccess(Guid memberKey);
}
