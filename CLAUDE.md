# HCS Passwordless — Solution CLAUDE.md

## What this is

A HCS-branded NuGet library suite for passwordless Umbraco 13 member authentication. Three Razor Class Library (RCL) packages: core magic-link, OTP add-on, WebAuthn add-on. All delivered via NuGet; the demo site is for manual verification only.

## Solution layout

```
src/HCS.Umbraco.Passwordless          # core — magic link, token store, rate limiter
src/HCS.Umbraco.Passwordless.Otp      # add-on — email OTP
src/HCS.Umbraco.Passwordless.WebAuthn # add-on — FIDO2 passkeys
tests/HCS.Umbraco.Passwordless.Tests  # xUnit suite (~104 tests)
demo/HCS.Umbraco.Passwordless.Demo    # runnable Umbraco 13 site
```

## Build & test

```bash
dotnet build
dotnet test                          # all tests
dotnet test --filter "Category=Otp"  # subset
```

## Configuration namespace

**All** `IOptions<T>` bindings use the `HCS:Authentication` section — never `Umbraco:*`. Section name constant lives at `PasswordlessOptions.SectionName`.

```json
{
  "HCS": {
    "Authentication": { ... }
  }
}
```

## Dependency rules

- The core package (`HCS.Umbraco.Passwordless`) has **no dependency** on the add-ons.
- Both add-ons depend on the core. The OTP and WebAuthn packages must **not** depend on each other.
- The demo may reference all three.

## Registration pattern

Services are wired via `IUmbracoBuilder` extension methods. Each package has one entry-point method:

```csharp
builder.CreateUmbracoBuilder()
    .AddPasswordlessMembers()   // core (PasswordlessBuilderExtensions)
    .AddPasswordlessOtp()       // OTP  (OtpBuilderExtensions)
    .AddPasswordlessWebAuthn()  // WebAuthn (WebAuthnBuilderExtensions)
    .Build();
```

Endpoints are mapped separately after `BootUmbracoAsync`:

```csharp
app.MapPasswordlessMembers()
    .WithOtp()
    .WithWebAuthn();
```

## Key abstractions

| Interface | Purpose |
|-----------|---------|
| `IPasswordlessAuthFactor` | Reports whether an auth method is enabled |
| `ISingleUseTokenStore` | Stores/consumes single-use tokens (distributed cache) |
| `IPasswordlessRateLimiter` | Sliding-window per-IP and per-email rate limiting |
| `IPasswordlessNotificationSender` | Sends magic-link emails |
| `IOtpNotificationSender` | Sends OTP emails |
| `IMemberCredentialStore` | Stores WebAuthn credentials in Umbraco DB |

## Security invariants — do not break

- Token comparison **must** use `ConstantTime.Equals` — never `==` or `string.Equals`.
- All auth endpoints apply `FakeWork` delay regardless of hit/miss.
- `ReturnUrl` must pass `ReturnUrlValidator` before any redirect.
- Magic link and OTP tokens are hashed (SHA-256) before storage.
- Tokens are invalidated on first successful use (`SingleUse` enforced by `ISingleUseTokenStore`).

## Email templates

Default templates ship inside the RCL at `Views/Emails/Passwordless/`. Host projects override by placing templates at the same relative path under their own `/Views/` folder. Templates use strongly-typed models (`MagicLinkEmailModel`, `OtpEmailModel`).

## Target framework / SDK

- All packages: `net8.0`, `Microsoft.NET.Sdk.Razor`
- Umbraco version range: `[13.0, 14.0)` — do not bump to Umbraco 14 without a separate branch/package.

## Packaging

All `src/` projects are packable (`<IsPackable>true</IsPackable>`). Build props live in `Directory.Build.props`; centralized package versions in `Directory.Packages.props`. No floating versions — pin everything.

## What lives in the demo vs. the packages

The demo site (`demo/`) is for interactive verification only — it is not shipped. Any feature that needs to be testable should have a unit test in `tests/`, not just a demo route.
