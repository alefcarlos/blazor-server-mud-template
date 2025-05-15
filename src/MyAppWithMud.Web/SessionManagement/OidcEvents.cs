using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MyAppWithMud.Web.SessionManagement;

public class OidcEvents : OpenIdConnectEvents
{
    private readonly IUserTokenStore _store;
    private readonly IUserAccessor _userAccessor;
    private readonly IUserTokenManagementService _service;

    public OidcEvents(IUserTokenStore store, IUserAccessor userAccessor, IUserTokenManagementService service)
    {
        _store = store;
        _userAccessor = userAccessor;
        _service = service;
    }

    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        context.ProtocolMessage.Prompt = "login";
        return Task.CompletedTask;
    }

    public override async Task RemoteSignOut(RemoteSignOutContext context)
    {
        var token = context.ProtocolMessage!.Parameters["logout_token"];
        var validationParameters = context.Options.TokenValidationParameters.Clone();
        var _configuration = await context.Options.ConfigurationManager!.GetConfigurationAsync(context.HttpContext.RequestAborted);

        var issuer = new[] { _configuration.Issuer };
        validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(issuer) ?? issuer;

        validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(_configuration.SigningKeys)
            ?? _configuration.SigningKeys;

        var tokenValidationResult = await context.Options.TokenHandler.ValidateTokenAsync(token, validationParameters);
        var user = new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
        //var jwt = JwtSecurityTokenConverter.Convert(tokenValidationResult.SecurityToken as JsonWebToken);

        await _service.RevokeRefreshTokenAsync(user);
        context.Success();
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(double.Parse(context.TokenEndpointResponse!.ExpiresIn));

        await _store.StoreTokenAsync(context.Principal!, new UserToken
        {
            AccessToken = context.TokenEndpointResponse.AccessToken,
            AccessTokenType = context.TokenEndpointResponse.TokenType,
            Expiration = exp,
            RefreshToken = context.TokenEndpointResponse.RefreshToken,
            Scope = context.TokenEndpointResponse.Scope
        });

        await base.TokenValidated(context);
    }
}
