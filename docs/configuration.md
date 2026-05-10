---
nodeName: "Configuration"
metaTitle: "Configuration Reference | HCS Passwordless for Umbraco"
metaDescription: "Complete appsettings.json reference for HCS Passwordless. All keys with defaults for magic link, OTP, WebAuthn, rate limits, and notifications."
articleTitle: "Configuration Reference"
tagline: "Every setting, its default, and what it does."
---

# Configuration Reference

All configuration lives under the `HCS:Authentication` section in `appsettings.json`. There are no backoffice settings — everything is file-based.

## Full example

```json
{
  "HCS": {
    "Authentication": {
      "LoginPath": "/login",
      "PostLoginRedirectPath": "/member",

      "RateLimits": {
        "PerIpRequestsPerMinute": 10,
        "PerEmailRequestsPerHour": 5,
        "VerifyPerIpPerMinute": 20,
        "FakeWorkDelay": "00:00:00.250"
      },

      "Notifications": {
        "FromAddress": "noreply@yoursite.com",
        "FromName": "Your Site",
        "MagicLinkSubject": "Sign in to Your Site",
        "MagicLinkPartial": "Emails/Passwordless/MagicLink",
        "Branding": {
          "ProductName": "Your Site",
          "LogoUrl": "https://yoursite.com/images/logo.png",
          "AccentColor": "#2d6cdf",
          "FooterHtml": "<p>© 2025 Your Company Ltd</p>"
        }
      },

      "MagicLink": {
        "Enabled": true,
        "TokenLifespan": "00:15:00",
        "SingleUse": true
      },

      "Otp": {
        "Enabled": true,
        "TokenLifespan": "00:05:00",
        "CodeLength": 6,
        "MaxAttempts": 5,
        "LockoutDuration": "00:15:00",
        "NotificationSubject": "Your sign-in code",
        "NotificationPartial": "Emails/Passwordless/Otp"
      },

      "WebAuthn": {
        "Enabled": true,
        "RpId": "yoursite.com",
        "RpName": "Your Site",
        "Origins": [ "https://yoursite.com" ],
        "TimeoutMs": 60000,
        "UserVerification": "Required",
        "AttestationPreference": "None",
        "ResidentKey": "Required",
        "AuthenticatorAttachment": null,
        "ChallengeTtl": "00:05:00",
        "RequireAtLeastOneNonPasskeyFactor": true
      }
    }
  }
}
```

---

## Core settings

These apply to all factors.

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `LoginPath` | string | `/login` | Path to your login page. Used for redirect-on-failure (e.g. expired magic link). |
| `PostLoginRedirectPath` | string | `/` | Default redirect destination after a successful sign-in. |

---

## Rate limits (`HCS:Authentication:RateLimits`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `PerIpRequestsPerMinute` | int | `10` | Max token-request calls per IP address per minute |
| `PerEmailRequestsPerHour` | int | `5` | Max token-request calls per email address per hour |
| `VerifyPerIpPerMinute` | int | `20` | Max verify/complete calls per IP address per minute |
| `FakeWorkDelay` | TimeSpan | `00:00:00.250` | Base delay added when the email address isn't registered, to prevent timing attacks. **Must be tuned to your email delivery latency** — see note below. |

> **FakeWorkDelay tuning:** The delay is randomised with ±50% jitter. A value of `250ms` produces actual delays between `125ms` and `375ms`.
>
> The purpose of this delay is to make the "member not found" path take the same wall-clock time as the "member found, email sent" path. If your transactional email provider is slow — common for cross-region SMTP or shared relay services — real email delivery can take 500ms or more, which is well outside the default delay range. An attacker making many requests can exploit this timing difference to determine whether a given email address is registered.
>
> **Recommendation:** Measure the 95th-percentile end-to-end delivery time of your transactional email provider and set `FakeWorkDelay` to at least that value. For many providers 1–2 seconds is appropriate. A higher value increases the response time for all unauthenticated requests, which is an acceptable trade-off for preventing email enumeration.

---

## Notifications (`HCS:Authentication:Notifications`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `FromAddress` | string | `no-reply@example.com` | The `From:` email address for all outgoing emails |
| `FromName` | string | `Example` | The `From:` display name |
| `MagicLinkSubject` | string | `Your sign-in link` | Subject line for magic link emails |
| `MagicLinkPartial` | string | `Emails/Passwordless/MagicLink` | Razor partial path for the HTML magic link email body |

### Branding (`HCS:Authentication:Notifications:Branding`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `ProductName` | string | `Your Site` | Your site or product name. Appears in email copy. |
| `LogoUrl` | string? | `null` | Optional logo image URL. Rendered in email templates if present. |
| `AccentColor` | string | `#2d6cdf` | Hex colour used for buttons and links in email templates |
| `FooterHtml` | string? | `null` | Optional HTML appended to the email footer. **Must be trusted, hardcoded HTML only** — see security note below. |

> **FooterHtml security note:** This value is rendered with `Html.Raw` — it is inserted into the email body as-is, with no HTML encoding. Only use hardcoded, trusted HTML strings here. Never source this value from user input, a database field, or a backoffice setting without first sanitising it with a library such as [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer). If you are unsure, leave it `null` and use the default footer text.

---

## Magic Link (`HCS:Authentication:MagicLink`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `true` | Enable or disable magic link sign-in |
| `TokenLifespan` | TimeSpan | `00:15:00` | How long a link remains valid after it's sent. Maximum: `01:00:00` (1 hour). |
| `SingleUse` | bool | `true` | When `true`, each link can only be used once. Strongly recommended. |

---

## OTP (`HCS:Authentication:Otp`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `true` | Enable or disable OTP sign-in |
| `TokenLifespan` | TimeSpan | `00:05:00` | How long a code remains valid after it's sent |
| `CodeLength` | int | `6` | Number of digits in the code. Allowed range: 4–10. |
| `MaxAttempts` | int | `5` | Wrong attempts allowed before the member is locked out |
| `LockoutDuration` | TimeSpan | `00:15:00` | How long the lockout lasts after exceeding `MaxAttempts` |
| `NotificationSubject` | string | `Your sign-in code` | Subject line for OTP emails |
| `NotificationPartial` | string | `Emails/Passwordless/Otp` | Razor partial path for the OTP email body |

---

## WebAuthn (`HCS:Authentication:WebAuthn`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `true` | Enable or disable WebAuthn sign-in |
| `RpId` | string | `localhost` | Your domain name, e.g. `yoursite.com`. **No** `https://` prefix, no port. Must exactly match the origin. |
| `RpName` | string | `Umbraco Site` | Human-readable name shown in passkey prompts on the member's device |
| `Origins` | string[] | `[]` | Full origins that are allowed to participate in WebAuthn ceremonies, e.g. `["https://yoursite.com"]`. Required in production. |
| `TimeoutMs` | uint | `60000` | Milliseconds before the browser's WebAuthn prompt times out. The browser may enforce its own minimum. |
| `UserVerification` | string | `Required` | Biometric/PIN requirement. `Required` = always; `Preferred` = if available; `Discouraged` = skip if possible. |
| `AttestationPreference` | string | `None` | How much information about the authenticator hardware to request. `None` is appropriate for most sites. |
| `ResidentKey` | string | `Required` | Whether to require a resident (discoverable) credential. `Required` enables username-free sign-in. |
| `AuthenticatorAttachment` | string? | `null` | `Platform` = built-in biometric only; `CrossPlatform` = USB/NFC keys only; `null` = accept any. |
| `ChallengeTtl` | TimeSpan | `00:05:00` | How long a registration or sign-in challenge is valid before it expires |
| `RequireAtLeastOneNonPasskeyFactor` | bool | `true` | Prevents deleting the last passkey when no other sign-in factor (magic link or OTP) is enabled |
| `TimestampDriftToleranceMs` | int | `300000` | Allowed clock-skew window for WebAuthn timestamp validation in milliseconds. Allowed range: 30 000–600 000 ms (30 s to 10 min). |
| `SignInOptionsPerIpPerMinute` | int | `10` | Maximum sign-in options requests per IP address per minute |
| `SignInCompletePerIpPerMinute` | int | `5` | Maximum sign-in complete requests per IP address per minute |

> **Restart required for WebAuthn changes:** The library resolves `RpId`, `Origins`, and other WebAuthn settings once at application startup and holds them for the lifetime of the process. This is intentional — origin pinning must not change mid-flight. If you update any value under `HCS:Authentication:WebAuthn`, restart the application for the change to take effect.

### Notes on `RpId` and `Origins`

These two settings work together and must be correct for WebAuthn to work at all.

- `RpId` is the **effective domain** — just the hostname, no scheme or port
- `Origins` is the **full set of allowed origins** — scheme + hostname + optional port

For a site at `https://www.yoursite.com`:
```json
"RpId": "yoursite.com",
"Origins": [ "https://www.yoursite.com" ]
```

Note that `RpId` is `yoursite.com` (parent domain) — this allows passkeys to work across subdomains if needed. If you only have one subdomain and want to be strict:
```json
"RpId": "www.yoursite.com",
"Origins": [ "https://www.yoursite.com" ]
```

For local development with HTTPS:
```json
"RpId": "localhost",
"Origins": [ "https://localhost:44350" ]
```

> **Important:** Any mismatch between `RpId`, `Origins`, and the actual origin of the page calling `navigator.credentials.get()` will cause the browser to silently reject the ceremony. If passkeys aren't working, this is the first thing to check.

---

## Environment-specific configuration

Use `appsettings.Development.json` to override settings for local development without touching your production configuration:

```json
// appsettings.Development.json
{
  "HCS": {
    "Authentication": {
      "WebAuthn": {
        "RpId": "localhost",
        "Origins": [ "https://localhost:44350" ]
      }
    }
  }
}
```

To see the WebAuthn log output during debugging, add the controller namespace to your logging config:

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "HCS.Passwordless": "Debug"
    }
  }
}
```

This will show you exactly which step in the sign-in flow is failing, with a Warning-level message for every early return and an Information-level message for successful sign-ins.

> **Security note:** Debug-level log messages include the member's **security stamp** — the value ASP.NET Core Identity uses to invalidate all active sessions for that member. Do not enable `Debug` logging for `HCS.Passwordless` in production, and ensure your log storage and shipping pipeline does not retain Debug output. If you use a centralised logging service (Seq, Elastic, Application Insights), confirm that the minimum level ingested is `Information` or higher in non-development environments.
