using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MyAppWithMud.Web.SessionManagement;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserTokenStore _userTokenStore;

    public CookieEvents(IUserTokenStore userTokenStore)
    {
        _userTokenStore = userTokenStore;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        //Aqui já sabemos que teremos um usuário com cookie válido
        var exists = await _userTokenStore.GetTokenAsync(context.Principal!);

        if (exists.IsError)
        {
            var expires = DateTimeOffset.Parse(context.Properties.GetTokenValue("expires_at")!);

            await _userTokenStore.StoreTokenAsync(context.Principal!, new UserToken
            {
                AccessToken = context.Properties.GetTokenValue("access_token"),
                AccessTokenType = context.Properties.GetTokenValue("token_type"),
                RefreshToken = context.Properties.GetTokenValue("refresh_token"),
                Expiration = expires,
                Scope = "openid email offline_access profile"
            });
        }
    }
}
