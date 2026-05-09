using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using HCS.Passwordless.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace HCS.Passwordless.Tests.MagicLink;

public class PasswordlessSignInServiceTests
{
    private readonly UserManager<MemberIdentityUser> _userManager;
    private readonly IMemberSignInManager _signInManager;
    private readonly PasswordlessSignInService _sut;

    public PasswordlessSignInServiceTests()
    {
        var store = Substitute.For<IUserStore<MemberIdentityUser>>();
        _userManager = Substitute.For<UserManager<MemberIdentityUser>>(
            store, null, null, null, null, null, null, null, null);
        _signInManager = Substitute.For<IMemberSignInManager>();
        _sut = new PasswordlessSignInService(_userManager, _signInManager, NullLogger<PasswordlessSignInService>.Instance);
    }

    [Fact]
    public async Task SignInAndRotateAsync_UpdatesSecurityStampBeforeSignIn()
    {
        var member = new MemberIdentityUser { Id = "m1" };
        var refreshed = new MemberIdentityUser { Id = "m1" };

        _userManager.UpdateSecurityStampAsync(member).Returns(IdentityResult.Success);
        _userManager.FindByIdAsync("m1").Returns(refreshed);

        await _sut.SignInAndRotateAsync(member);

        await _userManager.Received(1).UpdateSecurityStampAsync(member);
        await _userManager.Received(1).FindByIdAsync("m1");
    }

    [Fact]
    public async Task SignInAndRotateAsync_SignsInWithRefreshedMember()
    {
        var member = new MemberIdentityUser { Id = "m2" };
        var refreshed = new MemberIdentityUser { Id = "m2" };

        _userManager.UpdateSecurityStampAsync(member).Returns(IdentityResult.Success);
        _userManager.FindByIdAsync("m2").Returns(refreshed);

        await _sut.SignInAndRotateAsync(member, isPersistent: false, authenticationMethod: "test-method");

        await _signInManager.Received(1).SignInAsync(refreshed, false, "test-method");
    }

    [Fact]
    public async Task SignInAndRotateAsync_UsesPersistentByDefault()
    {
        var member = new MemberIdentityUser { Id = "m3" };
        _userManager.UpdateSecurityStampAsync(member).Returns(IdentityResult.Success);
        _userManager.FindByIdAsync("m3").Returns(member);

        await _sut.SignInAndRotateAsync(member);

        await _signInManager.Received(1).SignInAsync(Arg.Any<MemberIdentityUser>(), true, "passwordless");
    }
}
