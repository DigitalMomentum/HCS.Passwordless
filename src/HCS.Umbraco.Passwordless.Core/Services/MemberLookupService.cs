using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Services;

internal sealed class MemberLookupService : IMemberLookupService
{
    private readonly IMemberManager _memberManager;

    public MemberLookupService(IMemberManager memberManager) => _memberManager = memberManager;

    public async Task<MemberIdentityUser?> FindApprovedAsync(string email, CancellationToken ct = default)
    {
        var member = await _memberManager.FindByEmailAsync(email);
        if (member is null || !member.IsApproved || member.IsLockedOut) return null;
        return member;
    }

    public async Task<MemberIdentityUser?> FindApprovedByUserHandleAsync(byte[] userHandle, CancellationToken ct = default)
    {
        var memberKey = new Guid(userHandle);
        var member = await _memberManager.FindByIdAsync(memberKey.ToString());
        if (member is null || !member.IsApproved || member.IsLockedOut) return null;
        return member;
    }
}
