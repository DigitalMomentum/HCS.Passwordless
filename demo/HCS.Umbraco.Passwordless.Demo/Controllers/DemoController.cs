using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Security;

namespace HCS.Umbraco.Passwordless.Demo.Controllers;

public class DemoController : Controller
{
    private readonly IMemberSignInManager _signInManager;


    public DemoController(IMemberSignInManager signInManager)
    {
        _signInManager = signInManager;

    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    [Authorize]
    public IActionResult Member() => View();

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
