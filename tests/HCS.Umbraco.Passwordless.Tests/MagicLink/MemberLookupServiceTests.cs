using HCS.Umbraco.Passwordless.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace HCS.Umbraco.Passwordless.Tests.MagicLink;

public class MemberLookupServiceTests
{
    private readonly IMemberManager _memberManager = Substitute.For<IMemberManager>();
    private readonly MemberLookupService _sut;

    public MemberLookupServiceTests()
        => _sut = new MemberLookupService(_memberManager);

    [Fact]
    public async Task FindApprovedAsync_ReturnsMember_WhenApprovedAndNotLocked()
    {
        var member = new MemberIdentityUser { IsApproved = true };
        _memberManager.FindByEmailAsync("test@example.com").Returns(member);

        var result = await _sut.FindApprovedAsync("test@example.com");

        result.Should().BeSameAs(member);
    }

    [Fact]
    public async Task FindApprovedAsync_ReturnsNull_WhenMemberNotFound()
    {
        _memberManager.FindByEmailAsync("missing@example.com").Returns((MemberIdentityUser?)null);

        var result = await _sut.FindApprovedAsync("missing@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindApprovedAsync_ReturnsNull_WhenNotApproved()
    {
        var member = new MemberIdentityUser { IsApproved = false };
        _memberManager.FindByEmailAsync("unapproved@example.com").Returns(member);

        var result = await _sut.FindApprovedAsync("unapproved@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindApprovedAsync_ReturnsNull_WhenLockedOut()
    {
        var member = new MemberIdentityUser
        {
            IsApproved = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };
        _memberManager.FindByEmailAsync("locked@example.com").Returns(member);

        var result = await _sut.FindApprovedAsync("locked@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindApprovedByUserHandleAsync_ReturnsMember_ForValidHandle()
    {
        var memberKey = Guid.NewGuid();
        var handle = memberKey.ToByteArray();
        var member = new MemberIdentityUser { IsApproved = true };

        _memberManager.FindByIdAsync(memberKey.ToString()).Returns(member);

        var result = await _sut.FindApprovedByUserHandleAsync(handle);

        result.Should().BeSameAs(member);
    }

    [Fact]
    public async Task FindApprovedByUserHandleAsync_ReturnsNull_WhenMemberNotFound()
    {
        var handle = Guid.NewGuid().ToByteArray();
        _memberManager.FindByIdAsync(Arg.Any<string>()).Returns((MemberIdentityUser?)null);

        var result = await _sut.FindApprovedByUserHandleAsync(handle);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(32)]
    public async Task FindApprovedByUserHandleAsync_ReturnsNull_WhenHandleLengthIsNot16(int length)
    {
        var handle = new byte[length];

        var result = await _sut.FindApprovedByUserHandleAsync(handle);

        result.Should().BeNull();
        await _memberManager.DidNotReceive().FindByIdAsync(Arg.Any<string>());
    }
}
