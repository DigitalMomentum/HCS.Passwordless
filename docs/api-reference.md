# API Reference

All endpoints are conventional HTTP JSON APIs registered on your Umbraco site. They share these characteristics:

- **Base URL:** relative to your site root (e.g. `https://yoursite.com/auth/magic-link/request`)
- **Content-Type:** `application/json` for all POST/PATCH requests
- **Antiforgery:** POST/PATCH endpoints require the Umbraco antiforgery token in the `RequestVerificationToken` header (read it from the `@Html.AntiForgeryToken()` hidden input)
- **Authentication:** Most endpoints are public. Endpoints that require a signed-in member are marked (**requires auth**)

---

## Magic Link

### POST `/auth/magic-link/request`

Sends a magic link to the given email address.

**Request**
```json
{
    "email": "member@example.com",
    "returnUrl": "/member"
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `email` | Yes | The member's email address |
| `returnUrl` | No | Where to redirect after successful sign-in. Must be a local path (e.g. `/member`). Defaults to the configured `PostLoginRedirectPath`. |

**Response** — always `200 OK`, even if the email is unknown

```json
{ "ok": true }
```

Never rely on this response to determine whether an email address is registered. The server deliberately hides this information.

**Error responses**
| Status | Meaning |
|--------|---------|
| `429` | Rate limit exceeded — too many requests from this IP or for this email address |
| `400` | Malformed request (missing or invalid email) |

---

### GET `/auth/magic-link/verify`

Verifies a magic link token and signs the member in. This endpoint is accessed when the member clicks the link in their email.

**Query parameters**
| Parameter | Description |
|-----------|-------------|
| `email` | The member's email address |
| `token` | The single-use token from the link |
| `returnUrl` | Where to redirect after sign-in |

**Responses**
| Outcome | Response |
|---------|----------|
| Valid token | `302` redirect to `returnUrl` (member is now signed in) |
| Invalid/expired/used token | `302` redirect to login page with `?error=token_invalid` (or `token_expired`, `token_used`) |
| Member not found | `302` redirect to login page with `?error=member_not_found` |

---

## One-Time Password (OTP)

### POST `/auth/otp/request`

Sends a one-time code to the given email address.

**Request**
```json
{
    "email": "member@example.com",
    "returnUrl": "/member"
}
```

**Response** — always `200 OK`

```json
{ "ok": true }
```

**Error responses**
| Status | Meaning |
|--------|---------|
| `429` | Rate limit exceeded |

---

### POST `/auth/otp/verify`

Verifies a one-time code and signs the member in.

**Request**
```json
{
    "email": "member@example.com",
    "code": "847291",
    "returnUrl": "/member"
}
```

**Success response** — `200 OK`
```json
{
    "success": true,
    "redirectTo": "/member",
    "error": null
}
```

**Failure response** — `200 OK` (or `401`)
```json
{
    "success": false,
    "redirectTo": null,
    "error": "invalid_code"
}
```

| Error code | Meaning |
|------------|---------|
| `invalid_code` | Code is wrong |
| `expired` | Code has expired |
| `locked` | Too many failed attempts — member is locked out until `LockoutDuration` elapses |
| `member_not_found` | No approved member found for this email |

**Error responses**
| Status | Meaning |
|--------|---------|
| `429` | Rate limit exceeded (separate from the attempt counter lockout) |

---

## WebAuthn — Sign-In

### POST `/auth/webauthn/signin/options`

Starts a WebAuthn sign-in ceremony. Call this to get the challenge options to pass to `navigator.credentials.get()`.

**Request**
```json
{
    "email": "member@example.com"
}
```

`email` is optional. If provided, the server returns a hint list of the member's registered credentials. If omitted, the browser will present all available passkeys for your domain (discoverable credentials).

**Success response** — `200 OK`
```json
{
    "ceremonyId": "pwl:webauthn:sig:3fa85f64-...",
    "options": {
        "challenge": "base64url-encoded-challenge",
        "timeout": 60000,
        "rpId": "yoursite.com",
        "allowCredentials": [ ... ],
        "userVerification": "required"
    }
}
```

The `ceremonyId` must be included in the subsequent `/signin/complete` call.

---

### POST `/auth/webauthn/signin/complete`

Completes a WebAuthn sign-in ceremony. Call this after `navigator.credentials.get()` resolves.

**Request**
```json
{
    "ceremonyId": "pwl:webauthn:sig:3fa85f64-...",
    "assertion": {
        "id": "credential-id",
        "rawId": "base64url-encoded-raw-id",
        "type": "public-key",
        "response": {
            "authenticatorData": "base64url...",
            "clientDataJson": "base64url...",
            "signature": "base64url...",
            "userHandle": "base64url-or-null"
        }
    }
}
```

**Success response** — `200 OK`
```json
{ "ok": true }
```

**Error responses**
| Status | Body | Meaning |
|--------|------|---------|
| `400` | `{ "error": "invalid_ceremony" }` | Ceremony ID not found or expired |
| `400` | `{ "error": "assertion_failed", "detail": "Reference: ..." }` | Signature verification failed. Check server logs for the reference GUID. |
| `401` | — | Member not found, credentials mismatch, or decoy ceremony |
| `429` | — | Rate limit exceeded (5 attempts per IP per minute) |

---

## WebAuthn — Registration

Both registration endpoints **require the member to be signed in**.

### POST `/auth/webauthn/register/options`

Starts a passkey registration ceremony.

**Request**
```json
{
    "nickname": "My MacBook"
}
```

`nickname` is optional but recommended — it helps members identify which device a passkey belongs to.

**Success response** — `200 OK`
```json
{
    "ceremonyId": "pwl:webauthn:reg:member-guid:ceremony-guid",
    "options": {
        "rp": { "name": "Your Site", "id": "yoursite.com" },
        "user": { "id": "base64url-member-key", "name": "member@example.com", "displayName": "Member Name" },
        "challenge": "base64url-challenge",
        "pubKeyCredParams": [ ... ],
        "timeout": 60000,
        "excludeCredentials": [ ... ],
        "authenticatorSelection": { ... },
        "attestation": "none"
    }
}
```

---

### POST `/auth/webauthn/register/complete`

Completes the passkey registration ceremony.

**Request**
```json
{
    "ceremonyId": "pwl:webauthn:reg:...",
    "attestation": {
        "id": "credential-id",
        "rawId": "base64url...",
        "type": "public-key",
        "response": {
            "attestationObject": "base64url...",
            "clientDataJSON": "base64url...",
            "transports": [ "internal", "hybrid" ]
        }
    }
}
```

**Success response** — `200 OK`
```json
{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nickname": "My MacBook",
    "friendlyName": null,
    "aaGuid": "adce0002-35bc-c60a-648b-0b25f1f05503",
    "createdUtc": "2025-05-09T12:00:00Z",
    "lastUsedUtc": null,
    "backupEligible": true,
    "backupState": false,
    "transports": "internal,hybrid"
}
```

**Error responses**
| Status | Body | Meaning |
|--------|------|---------|
| `400` | `{ "error": "invalid_ceremony" }` | Ceremony expired or belongs to a different member |
| `400` | `{ "error": "attestation_failed", "detail": "Reference: ..." }` | Attestation verification failed |

---

## WebAuthn — Credential Management

All endpoints **require the member to be signed in**.

### GET `/auth/webauthn/credentials`

Returns all passkeys registered by the current member.

**Response** — `200 OK`
```json
[
    {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "nickname": "My MacBook",
        "friendlyName": null,
        "aaGuid": "adce0002-35bc-c60a-648b-0b25f1f05503",
        "createdUtc": "2025-01-15T10:30:00Z",
        "lastUsedUtc": "2025-05-09T08:15:00Z",
        "backupEligible": true,
        "backupState": true,
        "transports": "internal,hybrid"
    }
]
```

---

### PATCH `/auth/webauthn/credentials/{id}`

Renames a passkey.

**Request**
```json
{ "nickname": "My new name" }
```

| Status | Meaning |
|--------|---------|
| `200` | Renamed successfully |
| `400` | Nickname exceeds 64 characters |
| `404` | Credential not found (or belongs to a different member) |

---

### DELETE `/auth/webauthn/credentials/{id}`

Deletes a passkey.

| Status | Meaning |
|--------|---------|
| `204` | Deleted successfully |
| `404` | Credential not found |
| `409` | Cannot delete — this is the member's last sign-in factor and `RequireAtLeastOneNonPasskeyFactor` is `true` |
