using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using MyAppWithMud.Components;
using MyAppWithMud.Components.BrowserTime;

namespace Microsoft.AspNetCore.Builder;

public static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders | HttpLoggingFields.Duration;
            logging.CombineLogs = true;
        });

        // Add MudBlazor services
        builder.Services.AddMudServices();

        builder.Services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options
                .SetDefaultCulture(ApplicationCultures.SupportedCultures[0].Name)
                .AddSupportedCultures(ApplicationCultures.SupportedCultures.Select(x => x.Name).ToArray())
                .AddSupportedUICultures(ApplicationCultures.SupportedCultures.Select(x => x.Name).ToArray());
        });

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        //Para usar Auth com Oidc:
        //builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        //    .AddOpenIdConnect(options =>
        //    {
        //        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //        options.ResponseType = OpenIdConnectResponseType.Code;
        //        options.SaveTokens = true;

        //        options.MapInboundClaims = false;
        //        options.TokenValidationParameters.NameClaimType = "given_name";
        //        options.TokenValidationParameters.RoleClaimType = "role";
        //        options.SignedOutRedirectUri = "/account/signed-out";

        //        options.AdditionalAuthorizationParameters.Add("prompt", "login");

        //        options.Events = new OpenIdConnectEvents
        //        {
        //            OnRedirectToIdentityProviderForSignOut = async (context) =>
        //            {
        //                context.ProtocolMessage.IdTokenHint = await context.HttpContext.GetTokenAsync(OpenIdConnectResponseType.IdToken);
        //            }
        //        };
        //    })
        //    .AddCookie(options =>
        //    {
        //        options.AccessDeniedPath = "/account/access-denied";
        //    })
        //    ;

        builder.Services.AddHttpLogging(options =>
        {
            options.RequestHeaders.Add("X-Forwarded-For");
            options.RequestHeaders.Add("X-Forwarded-Host");
            options.RequestHeaders.Add("X-Forwarded-Proto");
            options.RequestHeaders.Add("X-Original-For");
            options.RequestHeaders.Add("X-Original-Host");
            options.RequestHeaders.Add("X-Original-Proto");
            options.RequestHeaders.Add("X-Real-IP");
            options.RequestHeaders.Add("X-Request-ID");
            options.RequestHeaders.Add("x-tenant");

            options.ResponseHeaders.Add("WWW-Authenticate");
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddBrowserTimeProvider();

        return builder;
    }
}
