# Email Templates

The library ships with default email templates for both magic link and OTP sign-in. They're functional and branded with your `ProductName`, but you'll probably want to match your site's design. This page explains how to override them.

## How override works

Templates are shipped as Razor partial views inside the NuGet package's Razor Class Library (RCL). Umbraco's view engine checks your project's `Views` folder first, so you can override any template simply by creating a file at the same path.

Default template paths:

| Template | Path |
|----------|------|
| Magic link email (HTML) | `Views/Emails/Passwordless/MagicLink.cshtml` |
| OTP email (HTML) | `Views/Emails/Passwordless/Otp.cshtml` |
| Shared layout | `Views/Emails/Passwordless/_Layout.cshtml` |

To override a template, create the same file in **your** project's `Views` folder. Umbraco will use yours instead of the library's default.

```
YourProject/
  Views/
    Emails/
      Passwordless/
        MagicLink.cshtml     ← your override
        Otp.cshtml           ← your override
```

You only need to create the files you want to change — any file you don't override falls back to the library's default.

## Template models

Each template receives a strongly-typed model with all the information you need to build the email.

### MagicLinkEmailModel

Used by `Views/Emails/Passwordless/MagicLink.cshtml`.

| Property | Type | Description |
|----------|------|-------------|
| `MemberName` | string | The member's display name, or their email address if no name is set |
| `Link` | Uri | The full magic link URL — this is what the member clicks |
| `Expiry` | TimeSpan | How long the link is valid (e.g. 15 minutes) |
| `Branding` | BrandingOptions | Your site's branding (see below) |

### OtpEmailModel

Used by `Views/Emails/Passwordless/Otp.cshtml`.

| Property | Type | Description |
|----------|------|-------------|
| `MemberName` | string | The member's display name, or their email address if no name is set |
| `Code` | string | The one-time code (e.g. `"847291"`) |
| `Expiry` | TimeSpan | How long the code is valid (e.g. 5 minutes) |
| `Branding` | BrandingOptions | Your site's branding (see below) |

### BrandingOptions

The `Branding` property on both models is populated from your `HCS:Authentication:Notifications:Branding` configuration.

| Property | Type | Description |
|----------|------|-------------|
| `ProductName` | string | Your site or product name — shown in email copy and subject lines |
| `LogoUrl` | string? | Optional URL to your logo image |
| `AccentColor` | string | Primary button/link colour, as a hex value (default `#2d6cdf`) |
| `FooterHtml` | string? | Optional raw HTML to append to the email footer |

## Example: Magic link template override

Here's a simple custom magic link template you can drop straight in. Adjust the HTML to match your brand.

```razor
@model HCS.Umbraco.Passwordless.MagicLink.Notifications.Models.MagicLinkEmailModel
@{
    Layout = null;
    var expiryMinutes = (int)Model.Expiry.TotalMinutes;
}
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sign in to @Model.Branding.ProductName</title>
    <style>
        body { font-family: sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }
        .card { background: white; border-radius: 8px; padding: 32px; max-width: 480px; margin: 0 auto; }
        .btn {
            display: inline-block; background: @Model.Branding.AccentColor;
            color: white; padding: 14px 28px; border-radius: 6px;
            text-decoration: none; font-weight: bold; margin: 24px 0;
        }
        .footer { font-size: 12px; color: #888; margin-top: 24px; }
    </style>
</head>
<body>
    <div class="card">
        @if (Model.Branding.LogoUrl != null)
        {
            <img src="@Model.Branding.LogoUrl" alt="@Model.Branding.ProductName" height="40" />
        }

        <h1>Sign in to @Model.Branding.ProductName</h1>
        <p>Hi @Model.MemberName,</p>
        <p>Click the button below to sign in. This link expires in @expiryMinutes minutes.</p>

        <a href="@Model.Link" class="btn">Sign in now</a>

        <p style="word-break:break-all">
            Or copy this link: <a href="@Model.Link">@Model.Link</a>
        </p>

        <div class="footer">
            <p>If you didn't request this, you can safely ignore this email.</p>
            @if (Model.Branding.FooterHtml != null)
            {
                @Html.Raw(Model.Branding.FooterHtml)
            }
        </div>
    </div>
</body>
</html>
```

## Example: OTP template override

```razor
@model HCS.Umbraco.Passwordless.Otp.Notifications.Models.OtpEmailModel
@{
    Layout = null;
    var expiryMinutes = (int)Model.Expiry.TotalMinutes;
}
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Your sign-in code for @Model.Branding.ProductName</title>
    <style>
        body { font-family: sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }
        .card { background: white; border-radius: 8px; padding: 32px; max-width: 480px; margin: 0 auto; }
        .code {
            font-size: 40px; font-weight: bold; letter-spacing: 8px;
            color: @Model.Branding.AccentColor; padding: 16px 0;
        }
        .footer { font-size: 12px; color: #888; margin-top: 24px; }
    </style>
</head>
<body>
    <div class="card">
        @if (Model.Branding.LogoUrl != null)
        {
            <img src="@Model.Branding.LogoUrl" alt="@Model.Branding.ProductName" height="40" />
        }

        <h1>Your sign-in code</h1>
        <p>Hi @Model.MemberName,</p>
        <p>Enter this code to sign in to @Model.Branding.ProductName. It expires in @expiryMinutes minutes.</p>

        <div class="code">@Model.Code</div>

        <div class="footer">
            <p>If you didn't request this code, you can safely ignore this email.</p>
            @if (Model.Branding.FooterHtml != null)
            {
                @Html.Raw(Model.Branding.FooterHtml)
            }
        </div>
    </div>
</body>
</html>
```

## Branding configuration

Set your branding once in `appsettings.json` and it flows through to all templates:

```json
"Notifications": {
    "FromAddress": "noreply@yoursite.com",
    "FromName": "Your Site",
    "MagicLinkSubject": "Sign in to Your Site",
    "Branding": {
        "ProductName": "Your Site",
        "LogoUrl": "https://yoursite.com/images/logo.png",
        "AccentColor": "#e85d04",
        "FooterHtml": "<p>© 2025 Your Company Ltd</p>"
    }
}
```

## Using a completely custom email sender

If you need more control — for example, to use a transactional email service's templating engine instead of Razor views — you can replace the email sender entirely.

See [Advanced → Custom email senders](advanced.md#custom-email-senders) for details.
