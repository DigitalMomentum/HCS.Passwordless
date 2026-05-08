# HCS.Umbraco.Passwordless.Tests — CLAUDE.md

## Role

xUnit test project for the core, magic link, OTP, and WebAuthn packages.

## Test frameworks

- **xUnit** — test runner
- **FluentAssertions** — all assertions (`result.Should().Be(...)`, not `Assert.Equal`)
- **NSubstitute** — all mocks/stubs (`Substitute.For<IFoo>()`)

## What to test here vs. not

**Test here:**
- All logic in `Services/`, `RateLimiting/`, `Security/`, `Configuration/` classes
- Endpoint behaviour via `Microsoft.AspNetCore.Mvc.Testing` when the logic is non-trivial
- Options validators (valid config, invalid config, boundary values)

**Do not test here:**
- Umbraco internals (CMS, database, member store) — substitute all Umbraco interfaces
- Email delivery — substitute `IPasswordlessNotificationSender` / `IOtpNotificationSender`
- Full FIDO2 hardware attestation/assertion flows — use `DistributedCacheChallengeStoreTests` and `WebAuthnOptionsValidatorTests` for the testable seams instead

## Naming convention

```
MethodName_Scenario_ExpectedResult
// e.g.: ConsumeAsync_TokenAlreadyUsed_ReturnsFalse
```

## No real infrastructure in unit tests

Do not spin up a real `IDistributedCache` from a running server. Use `new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))` as the in-process stand-in.

## Security tests are mandatory for security primitives

Any change to `ConstantTime`, `Sha256`, `ReturnUrlValidator`, `SlidingWindowRateLimiter`, or `DistributedCacheSingleUseTokenStore` **must** have corresponding test coverage. If tests for these classes are failing, do not suppress or skip them — fix the implementation.

## Current coverage baseline

~121 tests, all passing. Coverage includes: Core infrastructure, MagicLink options/auth factor, OTP flow, WebAuthn options validator, WebAuthn auth factor, challenge store single-use invariant, and member-deleted credential cleanup. Do not merge code that breaks existing tests or reduces coverage on the security primitives listed above.
