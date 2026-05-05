using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.WebAuthn.Configuration;
using HCS.Umbraco.Passwordless.WebAuthn.Dtos;
using HCS.Umbraco.Passwordless.WebAuthn.Storage;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Controllers;

namespace HCS.Umbraco.Passwordless.WebAuthn.Controllers;

[ApiController]
[Route("auth/webauthn/credentials")]
[Authorize]
public class WebAuthnCredentialsController : UmbracoApiController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberCredentialStore _store;
    private readonly IOptionsMonitor<WebAuthnOptions> _waOpts;
    private readonly IEnumerable<IPasswordlessAuthFactor> _authFactors;

    public WebAuthnCredentialsController(
        IMemberManager memberManager,
        IMemberCredentialStore store,
        IOptionsMonitor<WebAuthnOptions> waOpts,
        IEnumerable<IPasswordlessAuthFactor> authFactors)
    {
        _memberManager = memberManager;
        _store = store;
        _waOpts = waOpts;
        _authFactors = authFactors;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var member = await _memberManager.GetCurrentMemberAsync();
        if (member is null) return Forbid();

        var credentials = await _store.GetByMemberAsync(member.Key, ct);
        var views = credentials.Select(c => new CredentialView(
            c.Id, c.Nickname, null, c.AaGuid,
            c.CreatedUtc, c.LastUsedUtc,
            c.BackupEligible, c.BackupState, c.Transports));

        return Ok(views);
    }

    [HttpPatch("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(Guid id, [FromBody] RenameCredentialRequest req, CancellationToken ct)
    {
        var member = await _memberManager.GetCurrentMemberAsync();
        if (member is null) return Forbid();

        var nickname = (req.Nickname ?? string.Empty).Trim();
        if (nickname.Length > 64) return BadRequest(new { error = "nickname_too_long" });

        var ok = await _store.RenameAsync(member.Key, id, nickname, ct);
        return ok ? Ok(new { ok = true }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var member = await _memberManager.GetCurrentMemberAsync();
        if (member is null) return Forbid();

        if (_waOpts.CurrentValue.RequireAtLeastOneNonPasskeyFactor)
        {
            var count = await _store.CountForMemberAsync(member.Key, ct);
            var hasOtherFactor = _authFactors
                .Where(f => f.Name != "webauthn")
                .Any(f => f.IsEnabled);
            if (count <= 1 && !hasOtherFactor)
                return Conflict(new { error = "last_factor" });
        }

        var ok = await _store.RemoveAsync(member.Key, id, ct);
        return ok ? NoContent() : NotFound();
    }
}
