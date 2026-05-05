using Microsoft.Extensions.DependencyInjection;
using HCS.Umbraco.Passwordless.WebAuthn.Migrations;
using HCS.Umbraco.Passwordless.WebAuthn.Storage;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

namespace HCS.Umbraco.Passwordless.WebAuthn.Composing;

public sealed class WebAuthnComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<MemberDeletedNotification, MemberDeletedHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, MigrationStartedHandler>();
    }
}

internal sealed class MigrationStartedHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IMigrationPlanExecutor _executor;
    private readonly IKeyValueService _keyValue;
    private readonly IRuntimeState _runtimeState;

    public MigrationStartedHandler(
        ICoreScopeProvider scopeProvider,
        IMigrationPlanExecutor executor,
        IKeyValueService keyValue,
        IRuntimeState runtimeState)
    {
        _scopeProvider = scopeProvider;
        _executor = executor;
        _keyValue = keyValue;
        _runtimeState = runtimeState;
    }

    public Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken ct)
    {
        if (_runtimeState.Level < Umbraco.Cms.Core.RuntimeLevel.Run) return Task.CompletedTask;

        var plan = new PasswordlessMigrationPlan();
        var upgrader = new Upgrader(plan);
        upgrader.Execute(_executor, _scopeProvider, _keyValue);
        return Task.CompletedTask;
    }
}

internal sealed class MemberDeletedHandler : INotificationAsyncHandler<MemberDeletedNotification>
{
    private readonly IMemberCredentialStore _store;

    public MemberDeletedHandler(IMemberCredentialStore store) => _store = store;

    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken ct)
    {
        foreach (var member in notification.DeletedEntities)
        {
            await _store.RemoveAllForMemberAsync(member.Key, ct);
        }
    }
}
