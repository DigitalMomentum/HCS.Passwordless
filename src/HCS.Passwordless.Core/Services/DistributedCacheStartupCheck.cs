using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HCS.Passwordless.Services;

/// <summary>
/// Logs a startup warning when the application is using <see cref="MemoryDistributedCache"/>
/// as its <see cref="IDistributedCache"/> implementation. On multi-node deployments this means
/// OTP code storage, WebAuthn challenge storage, and (if the default token store is in use)
/// replay protection are all node-local and will not coordinate across instances.
/// </summary>
internal sealed class DistributedCacheStartupCheck : IHostedService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheStartupCheck> _logger;

    public DistributedCacheStartupCheck(
        IDistributedCache cache,
        ILogger<DistributedCacheStartupCheck> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_cache is MemoryDistributedCache)
        {
            _logger.LogWarning(
                "HCS Passwordless: IDistributedCache is using MemoryDistributedCache (in-process, node-local). " +
                "On a single-node deployment this is fine. On a multi-node (load-balanced) deployment, " +
                "OTP codes, WebAuthn challenges, and rate-limit counters will not be shared across nodes — " +
                "a member who requests a code on node A may be unable to verify it on node B. " +
                "Configure a shared IDistributedCache (e.g. AddStackExchangeRedisCache) before going live " +
                "with multiple instances. See the HCS Passwordless multi-instance documentation for details.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
