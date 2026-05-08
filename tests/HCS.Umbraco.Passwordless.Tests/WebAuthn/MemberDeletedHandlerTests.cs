using HCS.Umbraco.Passwordless.WebAuthn.Composing;
using HCS.Umbraco.Passwordless.WebAuthn.Storage;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace HCS.Umbraco.Passwordless.Tests.WebAuthn;

public class MemberDeletedHandlerTests
{
    private readonly IMemberCredentialStore _store = Substitute.For<IMemberCredentialStore>();

    private MemberDeletedHandler CreateSut() => new(_store);

    [Fact]
    public async Task HandleAsync_RemovesCredentials_ForDeletedMember()
    {
        var memberKey = Guid.NewGuid();
        var member = Substitute.For<IMember>();
        member.Key.Returns(memberKey);

        var notification = new MemberDeletedNotification(member, new EventMessages());
        await CreateSut().HandleAsync(notification, CancellationToken.None);

        await _store.Received(1).RemoveAllForMemberAsync(memberKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_RemovesCredentials_ForEachDeletedMember()
    {
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var member1 = Substitute.For<IMember>();
        member1.Key.Returns(key1);
        var member2 = Substitute.For<IMember>();
        member2.Key.Returns(key2);

        var sut = CreateSut();
        await sut.HandleAsync(new MemberDeletedNotification(member1, new EventMessages()), CancellationToken.None);
        await sut.HandleAsync(new MemberDeletedNotification(member2, new EventMessages()), CancellationToken.None);

        await _store.Received(1).RemoveAllForMemberAsync(key1, Arg.Any<CancellationToken>());
        await _store.Received(1).RemoveAllForMemberAsync(key2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationToken_ToStore()
    {
        using var cts = new CancellationTokenSource();
        var memberKey = Guid.NewGuid();
        var member = Substitute.For<IMember>();
        member.Key.Returns(memberKey);

        var notification = new MemberDeletedNotification(member, new EventMessages());
        await CreateSut().HandleAsync(notification, cts.Token);

        await _store.Received(1).RemoveAllForMemberAsync(memberKey, cts.Token);
    }
}
