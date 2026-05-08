# HCS.Umbraco.Passwordless — CLAUDE.md

## Role

Core RCL package. All other packages in this solution depend on it. Ships via NuGet.

## Key namespaces

| Namespace | Contents |
|-----------|---------|
| `Auth` | `IPasswordlessAuthFactor`, `MagicLinkAuthFactor` |
| `Configuration` | `PasswordlessOptions`, `MagicLinkOptions`, `NotificationOptions`, `RateLimitOptions`, `BrandingOptions` |
| `DependencyInjection` | `PasswordlessBuilderExtensions` — the single public entry point |
| `Endpoints` | Minimal API endpoints for magic link request/verify |
| `Notifications` | `EmailNotificationSender`, `RazorViewRenderer` |
| `RateLimiting` | `SlidingWindowRateLimiter` |
| `Security` | `ConstantTime`, `Sha256`, `MagicLinkTokenProvider` |
| `Services` | `ISingleUseTokenStore`, `IMemberLookupService`, `IPasswordlessSignInService` |

## Configuration binding

Options bind from `HCS:Authentication`. The section name constant is `PasswordlessOptions.SectionName`. Never use `Umbraco:*`.

## Critical security rules

- **Always** use `ConstantTime.Equals` for token comparison. Avoid `==` on token strings.
- `FakeWork` must be awaited on both the happy path and error path so timing is uniform.
- `ReturnUrlValidator` must validate every redirect target before issuing a `Location` header.
- Tokens are SHA-256 hashed before storage — store the hash, compare the hash.
- `ISingleUseTokenStore.ConsumeAsync` must be atomic — if it returns false, deny access.

## Service registration

All services use `TryAdd*` so host projects can override. The public API surface is `AddPasswordlessMembers()` on `IUmbracoBuilder` and `MapPasswordlessMembers()` on `IEndpointRouteBuilder`.

## Email templates

Views shipped inside the RCL at `Views/Emails/Passwordless/` and `Views/Shared/Passwordless/`. Host overrides take precedence because Umbraco's view engine checks the host project first. Use `RazorViewRenderer` to render views to strings — do not call `IEmailService` directly.

## Token provider

`MagicLinkTokenProvider` is registered as an ASP.NET Core Identity token provider under the name `TokenProviderNames.MagicLink`. Its lifespan is configured from `MagicLinkOptions.TokenLifespan` at startup, not at runtime.

## Add-on extension points

Add-ons extend the system by:
1. Implementing `IPasswordlessAuthFactor` and registering it with `TryAddEnumerable`.
2. Adding their own endpoints via a `With*()` fluent method on the endpoint builder.
3. Optionally implementing their own `ISingleUseTokenStore` variant.

## What not to change without careful review

- `ConstantTime.cs` — any change here is a security regression risk.
- `FakeWork.cs` — removing the await on the error path removes timing protection.
- `DistributedCacheSingleUseTokenStore` — the consume operation must remain atomic.
- `ReturnUrlValidator` — all allowlist/denylist logic must stay in sync with tests.
