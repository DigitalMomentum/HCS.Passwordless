using HCS.Umbraco.Passwordless.DependencyInjection;
using HCS.Umbraco.Passwordless.Otp.DependencyInjection;
//using HCS.Umbraco.Passwordless.WebAuthn.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .AddPasswordlessMagicLink()
    .AddPasswordlessOtp()
    //.AddPasswordlessWebAuthn()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

app.MapControllerRoute("demo-login", "/login", new { controller = "Demo", action = "Login" });
app.MapControllerRoute("demo-member", "/member", new { controller = "Demo", action = "Member" });
app.MapControllerRoute("demo-logout", "/logout", new { controller = "Demo", action = "Logout" });
app.MapControllers();

await app.RunAsync();
