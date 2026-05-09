# Concepts: What is Passwordless Authentication?

## The problem with passwords

When a member creates an account on your site, they normally pick a password and store it in their head (or, realistically, in a browser or password manager). That password then gets stored in your database as a hash. Everything works fine until it doesn't:

- Members forget their passwords and ask you to reset them
- Members reuse passwords from other sites, so a breach elsewhere compromises your site too
- Phishing attacks trick members into typing their password on a fake page
- Weak passwords are vulnerable to brute-force guessing

Passwordless authentication sidesteps all of this. Instead of sharing a secret (the password), the member **proves their identity** through a channel they already control.

## The three approaches

### Magic Links — "email is the password"

You send the member a special link in an email. They click it, and they're signed in. No password needed.

The link contains a short-lived, single-use token. If someone intercepts the link (unlikely if your member's email is secure), it can only be used once and expires after 15 minutes by default. The member's email account acts as the proof of identity — if they can receive mail there, they are who they say they are.

**Great for:** Sites where your members are comfortable with email, and where you want the lowest-friction onboarding experience.

**Analogy:** Like a hotel key card that's texted to you — it lets you in once and then stops working.

---

### One-Time Passwords (OTP) — "a code in your inbox"

Similar to magic links, but instead of clicking a link the member receives a short numeric code (e.g. `847 291`) and types it into a form on your site.

The code is valid for a few minutes and the member only gets a small number of attempts before it expires. This approach is familiar to anyone who has used two-factor authentication on another site.

**Great for:** Members who find clicking links in emails awkward (e.g. on mobile), or when you want a sign-in flow that stays entirely on your site.

**Analogy:** Like the PIN code your bank texts you to confirm a transfer.

---

### Passkeys (WebAuthn) — "your device is the key"

Passkeys are a newer technology that lets members sign in using their device's built-in authentication — Face ID, fingerprint scanner, Windows Hello, or a USB security key. No email involved at all.

When a member registers a passkey, their device creates a unique cryptographic key pair. The private key never leaves the device. When they sign in, the device signs a challenge from your server using that private key — proving identity without ever sending a password or code over the network.

Passkeys are phishing-proof by design, because the signature is tied to your exact domain. A fake site can't trick your members into authenticating, because the cryptographic proof simply wouldn't match.

**Great for:** High-security sites, members who are tech-savvy, or as an additional fast sign-in option alongside magic links or OTP.

**Analogy:** Like a physical lock that only one specific key can open — and the key never leaves your keychain.

---

## Which should you use?

You can offer all three at once. Many sites do. Here's a quick guide:

| Factor | Setup complexity | Member familiarity | Phishing resistant | No email needed |
|--------|-----------------|-------------------|--------------------|-----------------|
| Magic Link | Low | High — everyone has email | Mostly | No |
| OTP | Low | High — like 2FA | Mostly | No |
| Passkeys | Medium | Growing fast | Yes ✓ | Yes ✓ |

A common pattern is to start with **Magic Link** (easiest to deploy), optionally add **OTP** for members who prefer codes, and layer in **Passkeys** as a fast "sign in with your fingerprint" shortcut once the member has already set one up.

## How the library fits in

All three factors work with Umbraco's existing member system. Members still appear in the Members section of the backoffice exactly as before. The library adds sign-in endpoints and email notifications on top of your existing setup — it doesn't replace your member schema or require database migrations beyond an optional WebAuthn credentials table.

Configuration is done entirely through `appsettings.json` under the `HCS:Authentication` section, so there's nothing to configure in the backoffice.
