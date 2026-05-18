# HCS.Passwordless.Demo

Full Umbraco 17 demo site that exercises all three authentication methods — magic links, OTP, and WebAuthn passkeys. For local development and manual verification only; not shipped as a NuGet package.

## Prerequisites

- .NET 10.0 SDK
- No other dependencies — uses SQLite embedded database

## Running

```bash
cd demo/HCS.Passwordless.Demo
dotnet run
```

Open the URL shown in the console (typically `https://localhost:44391`). On first run, complete the Umbraco installer to create the admin account and database.

## Demo Routes

| Route | Description |
|-------|-------------|
| `/login` | Login page — shows magic link and OTP forms; passkey button if WebAuthn is enabled |
| `/member` | Protected member dashboard — requires sign-in, shows member details |
| `/umbraco` | Umbraco backoffice — manage members and content |

## Enabling WebAuthn

WebAuthn is disabled in `appsettings.json` (`Enabled: false`) but is already enabled in `appsettings.Development.json` for local development. When the `Development` environment is active, WebAuthn will be available on `https://localhost:61188`.

If the port differs on your machine, update `Origins` in `appsettings.Development.json`:

```json
{
  "HCS": {
    "Authentication": {
      "WebAuthn": {
        "Origins": [ "https://localhost:YOUR_PORT" ]
      }
    }
  }
}
```

Restart the demo site after changing the port.

## Creating a Test Member

1. Sign in to the backoffice at `/umbraco`.
2. Go to **Members** and create a new member with your email address.
3. Set the member as approved.
4. Return to `/login` and request a magic link or OTP to that email.

The demo logs emails to the console (Serilog) rather than sending real mail.

## Configuration

All settings are in `appsettings.json` under `HCS:Authentication`. See the individual package READMEs for full option references. The demo uses SQLite so no database server is required.
