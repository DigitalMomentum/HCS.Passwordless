# HCS.Umbraco.Passwordless.Demo — CLAUDE.md

## Role

Runnable Umbraco 13 demo site for manual verification. Not packaged or shipped. References all three source packages directly (project references, not NuGet).

## What this is for

- Manual end-to-end testing of the full auth flows
- Demonstrating the minimal `Program.cs` setup to users
- Verifying that view overrides work as expected

## What this is NOT for

- Unit tests — those live in `tests/`
- Regression coverage — tests in `tests/` cover that
- Production code — do not add business logic here

## Program.cs is the canonical integration example

`Program.cs` is intentionally minimal. Keep it that way — it documents the recommended setup for NuGet consumers. Do not add configuration or middleware that would confuse users reading it as an example.

## Configuration

`appsettings.json` is the reference configuration showing all available settings with sensible demo defaults. Keep it complete and commented where non-obvious. WebAuthn is disabled by default (`"Enabled": false`) because it requires a real HTTPS origin.

## SQLite database

The database file lands in the project's `App_Data/` folder. It is created on first run after completing the Umbraco installer. Committed to `.gitignore` — do not commit `Umbraco.sqlite.db`.

## Serilog

The demo site logs at `Warning` by default. To trace auth flows during development, lower the level in `appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "HCS": "Debug"
      }
    }
  }
}
```

## Demo controller

`DemoController` has two actions:
- `Login()` — public, renders the login page
- `Member()` — protected with `[Authorize]`, renders member details

The `Member` action is the redirect target after successful sign-in (`PostLoginRedirectPath: /member`).

## View overrides

The demo views live under `Views/Demo/` and `Views/Shared/`. Passwordless email and UI partials from the RCLs can be overridden by adding a view at the same path under `Views/Emails/Passwordless/` or `Views/Shared/Passwordless/`. Use this to test that host-level overrides work correctly.
