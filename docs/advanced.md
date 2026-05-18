---
metaTitle: "Advanced: Custom Services & Events | HCS Passwordless for Umbraco"
metaDescription: "Replace any built-in HCS Passwordless service with your own: custom email senders, token stores, rate limiters, attempt counters, and credential stores. Subscribe to security events."
---

# Advanced: Custom Services and Events

The library is designed to be extensible. Every major service has an interface that you can replace with your own implementation — useful when you want to send OTPs via SMS instead of email, store tokens in a database instead of the cache, or react to security events.

## Replacing built-in services

Each add-on's builder exposes methods to swap out individual services. Pass a configure callback to the registration method:

```csharp
builder.CreateUmbracoBuilder()
    .AddPasswordlessMagicLink(ml => ml
        .UseNotificationSender<MySender>()
    )
    .AddPasswordlessOtp(otp => otp
        .UseNotificationSender<MyOtpSender>()
    )
    .AddPasswordlessWebAuthn(wa => wa
        .UseMemberCredentialStore<MyCredentialStore>()
    )
    .Build();
```

Your replacement services are registered as scoped unless they implement `IDisposable`, in which case they're scoped automatically. You can use constructor injection normally — Umbraco's DI container resolves dependencies for you.

---

## Custom email senders

### Magic link: `IPasswordlessNotificationSender`

Replace the default magic link email sender with any implementation you like — a third-party email API, a logging stub for testing, an SMS gateway, or a custom Razor engine.

```csharp
public interface IPasswordlessNotificationSender
{
    Task SendMagicLinkAsync(
        MemberIdentityUser member,
        Uri magicLink,
        TimeSpan validFor,
        CancellationToken ct = default);
}
```

**Example — log to console instead of sending email (for development)**

```csharp
public class ConsoleMagicLinkSender : IPasswordlessNotificationSender
{
    private readonly ILogger<ConsoleMagicLinkSender> _logger;
    public ConsoleMagicLinkSender(ILogger<ConsoleMagicLinkSender> logger) => _logger = logger;

    public Task SendMagicLinkAsync(MemberIdentityUser member, Uri magicLink, TimeSpan validFor, CancellationToken ct = default)
    {
        _logger.LogInformation("MAGIC LINK for {Email}: {Link}", member.Email, magicLink);
        return Task.CompletedTask;
    }
}
```

Register it:
```csharp
.AddPasswordlessMagicLink(ml => ml.UseNotificationSender<ConsoleMagicLinkSender>())
```

---

### OTP: `IOtpNotificationSender`

```csharp
public interface IOtpNotificationSender
{
    Task SendOtpAsync(
        MemberIdentityUser member,
        string code,
        TimeSpan validFor,
        CancellationToken ct = default);
}
```

**Example — send via SMS using a fictional SMS service**

```csharp
public class SmsOtpSender : IOtpNotificationSender
{
    private readonly ISmsClient _sms;
    public SmsOtpSender(ISmsClient sms) => _sms = sms;

    public Task SendOtpAsync(MemberIdentityUser member, string code, TimeSpan validFor, CancellationToken ct = default)
    {
        // member.PhoneNumber requires your members to have a phone number stored
        var minutes = (int)validFor.TotalMinutes;
        return _sms.SendAsync(
            member.PhoneNumber!,
            $"Your sign-in code is {code}. Valid for {minutes} minutes.",
            ct);
    }
}
```

Register it:
```csharp
.AddPasswordlessOtp(otp => otp.UseNotificationSender<SmsOtpSender>())
```

---

## Custom token stores

### Magic link: `ISingleUseTokenStore`

Replaces the default distributed-cache single-use token store. Implement this if you want tokens tracked in a database, or need a custom expiry strategy.

```csharp
public interface ISingleUseTokenStore
{
    // Returns true if the token hash is new and was successfully marked as used.
    // Returns false if the token hash was already present (already consumed).
    Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default);
}
```

> The `tokenHash` is always a SHA-256 hex string. Never store the raw token.

Register it:
```csharp
.AddPasswordlessMagicLink(ml => ml.UseSingleUseTokenStore<MyTokenStore>())
```

---

### OTP: `IOtpCodeStore`

Stores and retrieves hashed OTP codes.

```csharp
public interface IOtpCodeStore
{
    Task SetAsync(string memberId, string purpose, byte[] hash, TimeSpan ttl, CancellationToken ct = default);
    Task<byte[]?> GetAsync(string memberId, string purpose, CancellationToken ct = default);
    Task DeleteAsync(string memberId, string purpose, CancellationToken ct = default);
}
```

Register it:
```csharp
.AddPasswordlessOtp(otp => otp.UseOtpCodeStore<MyOtpCodeStore>())
```

---

### OTP: `IAttemptCounter`

Tracks wrong-code attempts and manages lockouts.

```csharp
public interface IAttemptCounter
{
    Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId,
        string purpose,
        int maxAttempts,
        TimeSpan lockDuration,
        CancellationToken ct = default);

    Task ResetAsync(string memberId, string purpose, CancellationToken ct = default);
}
```

Register it:
```csharp
.AddPasswordlessOtp(otp => otp.UseAttemptCounter<MyAttemptCounter>())
```

---

### WebAuthn: `IMemberCredentialStore`

Replaces the default Umbraco database credential store. Useful if you want credentials stored in an external database, or to add custom auditing.

```csharp
public interface IMemberCredentialStore
{
    Task<IReadOnlyList<StoredCredential>> GetByMemberAsync(Guid memberKey, CancellationToken ct = default);
    Task<StoredCredential?> GetByCredentialIdAsync(byte[] credentialId, CancellationToken ct = default);
    Task<StoredCredential> AddAsync(StoredCredential credential, CancellationToken ct = default);
    Task UpdateAfterAssertionAsync(byte[] credentialId, uint newCounter, DateTime lastUsedUtc, bool hasEverIncremented, CancellationToken ct = default);
    Task<bool> RenameAsync(Guid memberKey, Guid credentialRowId, string nickname, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid memberKey, Guid credentialRowId, CancellationToken ct = default);
    Task<int> CountForMemberAsync(Guid memberKey, CancellationToken ct = default);
    Task RemoveAllForMemberAsync(Guid memberKey, CancellationToken ct = default);
}
```

Register it:
```csharp
.AddPasswordlessWebAuthn(wa => wa.UseMemberCredentialStore<MyCredentialStore>())
```

---

### WebAuthn: `IWebAuthnChallengeStore`

Stores and retrieves single-use ceremony challenges.

```csharp
public interface IWebAuthnChallengeStore
{
    Task PutAsync<T>(string key, T payload, TimeSpan ttl, CancellationToken ct = default);
    Task<T?> TakeAsync<T>(string key, CancellationToken ct = default);
}
```

`TakeAsync` must be **atomic** — it should retrieve and delete in a single operation to ensure challenges are single-use even under concurrent requests.

Register it:
```csharp
.AddPasswordlessWebAuthn(wa => wa.UseChallengeStore<MyChallengeStore>())
```

---

## Custom rate limiter

The rate limiter is shared across all factors. Replace it in any one add-on and it applies everywhere (since it's registered as a singleton via `TryAddSingleton`).

```csharp
public interface IPasswordlessRateLimiter
{
    // Returns true if the request is allowed, false if the limit has been exceeded.
    Task<bool> TryAcquireAsync(string key, TimeSpan window, int limit, CancellationToken ct = default);
}
```

Register it:
```csharp
.AddPasswordlessMagicLink(ml => ml.UseRateLimiter<MyRateLimiter>())
```

---

## Subscribing to events

The library publishes Umbraco notifications that you can subscribe to from your own code.

### `PasskeyCounterRegressionNotification`

Published when a WebAuthn sign-in attempt is detected with a signature counter lower than the stored value — a possible sign of a cloned or duplicated credential.

```csharp
public sealed class PasskeyCounterRegressionNotification : INotification
{
    public Guid MemberKey { get; init; }       // The affected member
    public byte[] CredentialId { get; init; }  // The credential involved
    public uint StoredCounter { get; init; }   // The last-known counter
    public uint ReceivedCounter { get; init; } // The counter from the assertion
}
```

**Subscribing**

Create a handler class:

```csharp
using Umbraco.Cms.Core.Notifications;

public class PasskeySecurityAlerter
    : INotificationAsyncHandler<PasskeyCounterRegressionNotification>
{
    private readonly ILogger<PasskeySecurityAlerter> _logger;

    public PasskeySecurityAlerter(ILogger<PasskeySecurityAlerter> logger) => _logger = logger;

    public async Task HandleAsync(
        PasskeyCounterRegressionNotification notification,
        CancellationToken ct)
    {
        // The sign-in has already been rejected by the library.
        // Here you can alert your team, lock the account, or flag for review.
        _logger.LogCritical(
            "Possible passkey cloning detected for member {MemberKey}. " +
            "Stored counter: {Stored}, Received: {Received}",
            notification.MemberKey,
            notification.StoredCounter,
            notification.ReceivedCounter);

        // Optionally: lock the member account, send an admin alert, etc.
        await Task.CompletedTask;
    }
}
```

Register the handler in a Composer or directly in `Program.cs`:

```csharp
// In Program.cs, before builder.Build():
builder.Services.AddScoped<
    INotificationAsyncHandler<PasskeyCounterRegressionNotification>,
    PasskeySecurityAlerter>();
```

Or in a Composer:

```csharp
public class SecurityComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<
            PasskeyCounterRegressionNotification,
            PasskeySecurityAlerter>();
    }
}
```

---

## Checking which factors are enabled

You can inject `IEnumerable<IPasswordlessAuthFactor>` to discover which factors are currently active. This is useful for building a login page that adapts to your configuration.

```csharp
@inject IEnumerable<IPasswordlessAuthFactor> AuthFactors

@foreach (var factor in AuthFactors.Where(f => f.IsEnabled))
{
    switch (factor.Name)
    {
        case "magic-link":
            <div>@await Html.PartialAsync("_MagicLinkForm")</div>
            break;
        case "otp":
            <div>@await Html.PartialAsync("_OtpForm")</div>
            break;
        case "webauthn":
            <div>@await Html.PartialAsync("_PasskeyButton")</div>
            break;
    }
}
```

`IPasswordlessAuthFactor.IsEnabled` reflects live configuration changes — if you toggle a factor in `appsettings.json` and your app supports hot reload, the UI updates without a restart.
