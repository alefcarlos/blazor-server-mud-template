using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using MyAppWithMud.Components;
using MyAppWithMud.Web;
using MyAppWithMud.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureServices();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/signin-oidc", StringComparison.OrdinalIgnoreCase)
        && context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/");
        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseRequestLocalization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(IComponentsMarker).Assembly)
    ;

app.MapGroup("/authentication").MapLoginAndLogoutDefaults();
app.MapGet("/culture-set", (HttpContext context, string culture, string redirectUri) =>
{
    if (culture is not null)
    {
        context.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture, culture)));
    }

    return Results.LocalRedirect(redirectUri);
});

app.MapDefaultEndpoints();

await app.RunAsync();
