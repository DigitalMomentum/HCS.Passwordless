# Multi-Instance Deployments

> **Startup warning:** When the application starts, the library checks whether `IDistributedCache` resolves to the default `MemoryDistributedCache`. If it does, a `Warning`-level log message is emitted to remind you to configure a shared cache before deploying to a multi-node environment. If you are intentionally running a single instance you can safely ignore this message (or suppress it by setting the log level for `HCS.Passwordless` to `Error`).

By default, the library uses in-process memory for two things: tracking whether a token has already been used, and counting wrong OTP attempts. This works correctly for a single Umbraco instance, but if you run multiple instances behind a load balancer — Azure App Service with scale-out, a Kubernetes deployment, or any setup where more than one web process serves traffic — each instance has its own memory and they cannot coordinate.

The practical consequence is:

| Scenario | Risk |
|----------|------|
| Two instances receive the same magic link token simultaneously | Both pass the single-use check independently — same token signs in twice |
| An attacker submits OTP guesses spread across instances | Each instance maintains its own count — the lockout never triggers |

This page explains how to replace these two services with an implementation that coordinates across instances. You have two main options: **Redis** and **SQL Server**.

---

## What you are replacing

Two interfaces need replacing for a multi-instance deployment:

### `ISingleUseTokenStore` (magic link replay protection)

```csharp
public interface ISingleUseTokenStore
{
    // Returns true the first time a token hash is seen — marks it used.
    // Returns false if the hash has already been marked used.
    // This must be atomic: concurrent calls with the same hash must only return true once.
    Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default);
}
```

The key constraint is **atomicity** — checking and marking in a single indivisible operation. Reading, checking, and then writing as three separate steps is not safe under concurrent load.

### `IWebAuthnChallengeStore` (WebAuthn ceremony replay protection)

```csharp
public interface IWebAuthnChallengeStore
{
    Task PutAsync<T>(string key, T payload, TimeSpan ttl, CancellationToken ct = default);

    // Must atomically retrieve and remove — only one caller can succeed per key.
    Task<T?> TakeAsync<T>(string key, CancellationToken ct = default);
}
```

The default implementation stores challenges in `IDistributedCache`. A shared Redis `IDistributedCache` resolves the multi-instance concern (challenges are visible across nodes), but the `GetAsync` + `RemoveAsync` sequence still has a narrow TOCTOU window: two simultaneous `TakeAsync` calls with the same ceremony ID can both retrieve the challenge before either deletes it.

For most deployments the window is narrow enough and the FIDO2 library's signature uniqueness check provides a second layer of defence. If you want to eliminate the window entirely, replace the store with the Redis `GETDEL` implementation below.

### `IAttemptCounter` (OTP brute-force lockout)

```csharp
public interface IAttemptCounter
{
    // Increments the attempt count for this member and purpose.
    // Returns the new count and whether the member is now locked out.
    // The increment must be atomic: concurrent calls must each receive a unique count.
    Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose,
        int maxAttempts, TimeSpan lockDuration,
        CancellationToken ct = default);

    // Resets the counter after a successful sign-in.
    Task ResetAsync(string memberId, string purpose, CancellationToken ct = default);
}
```

The same atomicity rule applies: every concurrent increment must produce a unique, accurate count.

---

## Option A: Redis

Redis provides native atomic commands that map directly onto both requirements.

### Add the StackExchange.Redis package

```bash
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

### Register the connection

In `Program.cs`, before `builder.Build()`:

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
```

And in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-host:6379"
  }
}
```

### Implement `ISingleUseTokenStore` with Redis `SET NX`

The Redis `SET key value NX EX seconds` command sets a key **only if it does not already exist**, and returns `true` if the key was set. This is an atomic check-and-set — exactly what single-use enforcement requires.

```csharp
using HCS.Passwordless.Services;
using StackExchange.Redis;

public sealed class RedisSingleUseTokenStore : ISingleUseTokenStore
{
    private readonly IConnectionMultiplexer _redis;

    public RedisSingleUseTokenStore(IConnectionMultiplexer redis) => _redis = redis;

    public async Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"pwl:token-used:{tokenHash}";

        // SET NX — only sets if absent. Returns true if the key was newly set (first use).
        return await db.StringSetAsync(key, 1, ttl.Add(TimeSpan.FromSeconds(60)), When.NotExists);
    }
}
```

> **Why the extra 60 seconds on the TTL?** The token itself expires after `TokenLifespan`. The store entry is kept slightly longer so that a token right at the boundary can't be replayed in the gap between token expiry and store expiry.

### Implement `IAttemptCounter` with Redis `INCR`

Redis `INCR` atomically increments a key and returns the new value. Because it is atomic, 50 concurrent increments will always produce the values 1 through 50 — no two callers share a count.

```csharp
using HCS.Passwordless.Services;
using StackExchange.Redis;

public sealed class RedisAttemptCounter : IAttemptCounter
{
    private readonly IConnectionMultiplexer _redis;

    public RedisAttemptCounter(IConnectionMultiplexer redis) => _redis = redis;

    public async Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose,
        int maxAttempts, TimeSpan lockDuration,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var countKey = $"pwl:attempts:{memberId}:{purpose}";
        var lockKey  = $"pwl:attempts-lock:{memberId}:{purpose}";

        // Check lock first — if already locked, return max count without incrementing.
        if (await db.KeyExistsAsync(lockKey))
            return (maxAttempts, true);

        // Atomic increment. Each concurrent caller receives a unique new value.
        var count = (int)await db.StringIncrementAsync(countKey);

        // Set TTL on the first increment so the counter self-expires.
        if (count == 1)
            await db.KeyExpireAsync(countKey, lockDuration);

        if (count >= maxAttempts)
        {
            // SET NX so multiple threads racing here don't reset the TTL.
            await db.StringSetAsync(lockKey, 1, lockDuration, When.NotExists);
            return (count, true);
        }

        return (count, false);
    }

    public async Task ResetAsync(string memberId, string purpose, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"pwl:attempts:{memberId}:{purpose}");
        await db.KeyDeleteAsync($"pwl:attempts-lock:{memberId}:{purpose}");
    }
}
```

### Implement `IWebAuthnChallengeStore` with Redis `GETDEL` (optional hardening)

> **This step is optional.** If you configure a shared `IDistributedCache` (see below), the default challenge store already coordinates across nodes. This implementation closes the residual TOCTOU window for deployments that require it.

Redis 6.2+ provides `GETDEL`, which atomically retrieves a key and deletes it in a single round-trip — there is no window between the get and the delete.

```csharp
using System.Text.Json;
using HCS.Passwordless.WebAuthn.Services;
using StackExchange.Redis;

public sealed class RedisWebAuthnChallengeStore : IWebAuthnChallengeStore
{
    private readonly IConnectionMultiplexer _redis;

    public RedisWebAuthnChallengeStore(IConnectionMultiplexer redis) => _redis = redis;

    public async Task PutAsync<T>(string key, T payload, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        await db.StringSetAsync($"pwl:challenge:{key}", json, ttl);
    }

    public async Task<T?> TakeAsync<T>(string key, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        // GETDEL atomically retrieves and removes the key — no TOCTOU window.
        var result = await db.ExecuteAsync("GETDEL", $"pwl:challenge:{key}");
        if (result.IsNull) return default;
        return JsonSerializer.Deserialize<T>((byte[])result!);
    }
}
```

> **Redis version requirement:** `GETDEL` was added in Redis 6.2. If you are on an older Redis, replace the `ExecuteAsync("GETDEL", ...)` call with `StringGetDeleteAsync(key)` (available in StackExchange.Redis 2.6+), which uses a Lua script as a fallback on older servers.

### Register your implementations

```csharp
builder.CreateUmbracoBuilder()
    .AddPasswordlessMagicLink(ml => ml
        .UseSingleUseTokenStore<RedisSingleUseTokenStore>()
    )
    .AddPasswordlessOtp(otp => otp
        .UseAttemptCounter<RedisAttemptCounter>()
    )
    .AddPasswordlessWebAuthn(wa => wa
        .UseChallengeStore<RedisWebAuthnChallengeStore>()  // optional — only if closing TOCTOU window
    )
    .Build();
```

The builder methods call `AddScoped`, which overrides the default in-memory singletons. Your Redis implementation is created per-request but shares the underlying `IConnectionMultiplexer` singleton.

---

## Option B: SQL Server

If your application already uses SQL Server and you'd rather not introduce Redis as a dependency, you can use the database instead.

### `ISingleUseTokenStore` with a unique-constraint table

Create a table with a unique constraint on the token hash. An `INSERT` that violates the constraint means the token was already used.

```sql
CREATE TABLE PasswordlessUsedTokens (
    TokenHash   NVARCHAR(64)     NOT NULL,
    ExpiresUtc  DATETIME2        NOT NULL,
    CONSTRAINT PK_PasswordlessUsedTokens PRIMARY KEY (TokenHash)
);

-- Optional: index for the cleanup job
CREATE INDEX IX_PasswordlessUsedTokens_Expires ON PasswordlessUsedTokens (ExpiresUtc);
```

```csharp
using HCS.Passwordless.Services;
using Microsoft.Data.SqlClient;

public sealed class SqlSingleUseTokenStore : ISingleUseTokenStore
{
    private readonly string _connectionString;

    public SqlSingleUseTokenStore(IConfiguration config)
        => _connectionString = config.GetConnectionString("umbracoDbDSN")!;

    public async Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        // First clean up expired rows to keep the table small.
        await using (var del = new SqlCommand(
            "DELETE FROM PasswordlessUsedTokens WHERE ExpiresUtc < GETUTCDATE()", conn))
            await del.ExecuteNonQueryAsync(ct);

        // Attempt insert. Duplicate key = already used.
        await using var cmd = new SqlCommand(
            "INSERT INTO PasswordlessUsedTokens (TokenHash, ExpiresUtc) VALUES (@hash, @exp)", conn);
        cmd.Parameters.AddWithValue("@hash", tokenHash);
        cmd.Parameters.AddWithValue("@exp", DateTime.UtcNow.Add(ttl).AddSeconds(60));

        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
            return true;  // Insert succeeded — first use
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return false;  // Duplicate key — already used
        }
    }
}
```

> SQL error 2627 is a primary-key violation; 2601 is a unique-index violation. Catching both covers all constraint configurations.

### `IAttemptCounter` with an atomic SQL UPDATE

```sql
CREATE TABLE PasswordlessAttempts (
    MemberId    NVARCHAR(100)    NOT NULL,
    Purpose     NVARCHAR(100)    NOT NULL,
    Count       INT              NOT NULL DEFAULT 1,
    LockedUntil DATETIME2        NULL,
    ExpiresUtc  DATETIME2        NOT NULL,
    CONSTRAINT PK_PasswordlessAttempts PRIMARY KEY (MemberId, Purpose)
);
```

```csharp
using HCS.Passwordless.Services;
using Microsoft.Data.SqlClient;

public sealed class SqlAttemptCounter : IAttemptCounter
{
    private readonly string _connectionString;

    public SqlAttemptCounter(IConfiguration config)
        => _connectionString = config.GetConnectionString("umbracoDbDSN")!;

    public async Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose,
        int maxAttempts, TimeSpan lockDuration,
        CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var expires = DateTime.UtcNow.Add(lockDuration);

        // Upsert and atomic increment in one statement.
        // MERGE acquires a row lock, so concurrent callers serialize on the same row.
        const string sql = """
            MERGE PasswordlessAttempts WITH (HOLDLOCK) AS target
            USING (VALUES (@memberId, @purpose)) AS src (MemberId, Purpose)
                ON target.MemberId = src.MemberId AND target.Purpose = src.Purpose
            WHEN MATCHED AND target.LockedUntil IS NULL AND target.ExpiresUtc > GETUTCDATE() THEN
                UPDATE SET Count = target.Count + 1
            WHEN MATCHED AND (target.LockedUntil IS NOT NULL OR target.ExpiresUtc <= GETUTCDATE()) THEN
                UPDATE SET Count = 1, LockedUntil = NULL, ExpiresUtc = @expires
            WHEN NOT MATCHED THEN
                INSERT (MemberId, Purpose, Count, ExpiresUtc)
                VALUES (@memberId, @purpose, 1, @expires)
            OUTPUT inserted.Count, inserted.LockedUntil;
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@memberId", memberId);
        cmd.Parameters.AddWithValue("@purpose",  purpose);
        cmd.Parameters.AddWithValue("@expires",  expires);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);

        var count    = reader.GetInt32(0);
        var locked   = !reader.IsDBNull(1);

        if (!locked && count >= maxAttempts)
        {
            // Set the lock flag in a follow-up update.
            await reader.CloseAsync();
            await using var lockCmd = new SqlCommand(
                "UPDATE PasswordlessAttempts SET LockedUntil = @exp WHERE MemberId = @m AND Purpose = @p",
                conn);
            lockCmd.Parameters.AddWithValue("@exp", expires);
            lockCmd.Parameters.AddWithValue("@m",   memberId);
            lockCmd.Parameters.AddWithValue("@p",   purpose);
            await lockCmd.ExecuteNonQueryAsync(ct);
            return (count, true);
        }

        return (count, locked);
    }

    public async Task ResetAsync(string memberId, string purpose, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "DELETE FROM PasswordlessAttempts WHERE MemberId = @m AND Purpose = @p", conn);
        cmd.Parameters.AddWithValue("@m", memberId);
        cmd.Parameters.AddWithValue("@p", purpose);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
```

Register the same way:

```csharp
builder.CreateUmbracoBuilder()
    .AddPasswordlessMagicLink(ml => ml
        .UseSingleUseTokenStore<SqlSingleUseTokenStore>()
    )
    .AddPasswordlessOtp(otp => otp
        .UseAttemptCounter<SqlAttemptCounter>()
    )
    .Build();
```

---

## Other stores that are also instance-local

`ISingleUseTokenStore` and `IAttemptCounter` are the two stores with atomicity requirements. There are other stores worth reviewing for a multi-instance setup, though they have softer failure modes:

| Store | Interface | Default | Multi-instance risk |
|-------|-----------|---------|---------------------|
| OTP code store | `IOtpCodeStore` | Distributed cache (`MemoryDistributedCache`) | A code issued on one instance may not be verifiable on another |
| WebAuthn challenge store | `IWebAuthnChallengeStore` | Distributed cache | A ceremony started on one instance cannot be completed on another; residual TOCTOU window even with shared cache |
| Rate limiter | `IPasswordlessRateLimiter` | In-process memory | Each instance has its own window — effective limit is `instances × configured limit` |

The distributed cache implementation of `IOtpCodeStore` will coordinate correctly once you configure a real shared `IDistributedCache`. `IWebAuthnChallengeStore` likewise coordinates across nodes with a shared `IDistributedCache`, but retains a narrow TOCTOU window — see the [Redis `GETDEL` implementation above](#implement-iwebauthnchallengstore-with-redis-getdel-optional-hardening) if you need to eliminate it. The rate limiter has a dedicated interface if you want to replace it entirely.

### Shared distributed cache for `IOtpCodeStore` and `IWebAuthnChallengeStore`

If you configure Redis as your `IDistributedCache`, the OTP code and WebAuthn challenge stores will use it automatically without any further changes:

```csharp
// In Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
```

This covers the softer multi-instance concerns. The atomicity-sensitive stores (`ISingleUseTokenStore` and `IAttemptCounter`) still need the explicit Redis or SQL implementations described above, because the `IDistributedCache` interface does not expose atomic operations.

---

## Verifying your implementation

The test suite ships two concurrency tests you can use as a template to verify your own implementation behaves correctly under parallel load:

- `InMemorySingleUseTokenStoreTests.TryMarkUsedAsync_ConcurrentCalls_OnlyOneSucceeds` — fires 50 parallel calls with the same token hash and asserts exactly 1 returns `true`.
- `InMemoryAttemptCounterTests.IncrementAndCheckAsync_ConcurrentCalls_EachGetsUniqueMonotonicCount` — fires 50 parallel increments and asserts the results are counts 1–50 with no duplicates.

Copy these tests and swap the `CreateStore()` / `CreateCounter()` factory to return your implementation instead. If they pass, your atomicity guarantees are correct.
