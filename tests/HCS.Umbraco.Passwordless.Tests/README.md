# HCS.Umbraco.Passwordless.Tests

xUnit test suite for the HCS Passwordless packages. Covers core magic-link logic, OTP add-on, security utilities, and configuration validation.

## Running Tests

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific area
dotnet test --filter "FullyQualifiedName~Otp"
dotnet test --filter "FullyQualifiedName~MagicLink"
```

## Test Structure

```
tests/HCS.Umbraco.Passwordless.Tests/
├── Common/                          # Core shared utilities
│   ├── ConstantTimeTests.cs
│   ├── DistributedCacheSingleUseTokenStoreTests.cs
│   ├── MagicLinkAuthFactorTests.cs
│   ├── PasswordlessOptionsValidatorTests.cs
│   ├── ReturnUrlValidatorTests.cs
│   ├── Sha256HelperTests.cs
│   └── SlidingWindowRateLimiterTests.cs
├── MagicLink/                       # Magic link flow
│   ├── EmailNotificationSenderTests.cs
│   ├── MemberLookupServiceTests.cs
│   └── PasswordlessSignInServiceTests.cs
└── Otp/                             # OTP add-on
    ├── DistributedCacheAttemptCounterTests.cs
    ├── DistributedCacheOtpCodeStoreTests.cs
    ├── EmailOtpNotificationSenderTests.cs
    ├── OtpAuthFactorTests.cs
    ├── OtpOptionsValidatorTests.cs
    └── OtpTokenProviderTests.cs
```

## Dependencies

| Package | Purpose |
|---------|---------|
| `xunit` | Test framework |
| `FluentAssertions` | Assertion library |
| `NSubstitute` | Mocking / substitution |
| `Microsoft.AspNetCore.Mvc.Testing` | In-process web app tests |
| `coverlet.collector` | Code coverage collection |

## Conventions

- Test class names match the class under test: `FooTests` tests `Foo`.
- Use `NSubstitute` for all dependencies — no concrete Umbraco infrastructure in unit tests.
- Arrange/Act/Assert structure; descriptive method names in the form `MethodName_Scenario_ExpectedResult`.
- Use `FluentAssertions` for all assertions rather than `Assert.*`.
