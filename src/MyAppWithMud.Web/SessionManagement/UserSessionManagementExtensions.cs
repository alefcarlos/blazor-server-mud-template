using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace MyAppWithMud.Web.SessionManagement;

public static class UserSessionManagementExtensions
{
    public static IServiceCollection AddUserSessionManagement(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();

        services.AddCascadingAuthenticationState();

        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;

                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "given_name";
                options.TokenValidationParameters.RoleClaimType = "role";
                options.SignedOutRedirectUri = "/account/signed-out";

                options.EventsType = typeof(OidcEvents);
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/account/access-denied";
                options.EventsType = typeof(CookieEvents);
            })
            ;

        // adds access token management
        services.AddOpenIdConnectAccessTokenManagement()
            .AddBlazorServerAccessTokenManagement<ServerSideTokenStore>()
            ;

        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        services.AddTransient<OidcEvents>();
        services.AddTransient<CookieEvents>();

        return services;
    }
}
