using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace MyAppWithMud.Web.SessionManagement;

public class OidcEvents : OpenIdConnectEvents
{
    private readonly IUserTokenStore _store;
    private readonly IUserAccessor _userAccessor;
    private readonly IUserTokenManagementService _service;
    private readonly ILogger _logger;

    public OidcEvents(IUserTokenStore store, IUserAccessor userAccessor, IUserTokenManagementService service, ILogger<OidcEvents> logger)
    {
        _store = store;
        _userAccessor = userAccessor;
        _service = service;
        _logger = logger;
    }

    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        context.ProtocolMessage.Prompt = "login";
        return Task.CompletedTask;
    }

    public override async Task RemoteSignOut(RemoteSignOutContext context)
    {
        if (!TryGetLogoutToken(context, out var token) || token == null)
        {
            return;
        }

        var principal = await ValidateLogoutTokenAsync(context, token);
        if (principal == null)
        {
            return;
        }

        if (!HasValidLogoutEvent(principal, context))
        {
            return;
        }

        if (!HasSubOrSid(principal, context))
        {
            return;
        }

        await _service.RevokeRefreshTokenAsync(principal);
        context.Principal = principal;
        context.Success();
    }

    private bool TryGetLogoutToken(RemoteSignOutContext context, out string? token)
    {
        token = null;
        if (context.ProtocolMessage is null)
        {
            _logger.LogError("ProtocolMessage must not be null");
            HandleBadRequest(context);
            return false;
        }
        if (!context.ProtocolMessage.Parameters.TryGetValue("logout_token", out token))
        {
            _logger.LogError("logout_token must not be null");
            HandleBadRequest(context);
            return false;
        }
        return true;
    }

    private async Task<ClaimsPrincipal?> ValidateLogoutTokenAsync(RemoteSignOutContext context, string token)
    {
        var validationParameters = context.Options.TokenValidationParameters.Clone();
        var configuration = await context.Options.ConfigurationManager!.GetConfigurationAsync(context.HttpContext.RequestAborted);
        var issuer = new[] { configuration.Issuer };
        validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(issuer) ?? issuer;
        validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(configuration.SigningKeys)
            ?? configuration.SigningKeys;
        var tokenValidationResult = await context.Options.TokenHandler.ValidateTokenAsync(token, validationParameters);
        if (!tokenValidationResult.IsValid)
        {
            _logger.LogError(tokenValidationResult.Exception, "erro on validating token");
            HandleBadRequest(context);
            return null;
        }
        return new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
    }

    private bool HasValidLogoutEvent(ClaimsPrincipal principal, RemoteSignOutContext context)
    {
        var events = principal.FindFirst("events")?.Value;
        if (string.IsNullOrWhiteSpace(events))
        {
            _logger.LogError("events claim is required");
            HandleBadRequest(context);
            return false;
        }
        if (!events.Contains("http://schemas.openid.net/event/backchannel-logout"))
        {
            _logger.LogError("events must be 'http://schemas.openid.net/event/backchannel-logout'");
            HandleBadRequest(context);
            return false;
        }
        return true;
    }

    private bool HasSubOrSid(ClaimsPrincipal principal, RemoteSignOutContext context)
    {
        var sub = principal.FindFirst("sub")?.Value;
        var sid = principal.FindFirst("sid")?.Value;
        if (string.IsNullOrEmpty(sub) && string.IsNullOrEmpty(sid))
        {
            _logger.LogError("Token must contain 'sub' or 'sid'");
            HandleBadRequest(context);
            return false;
        }
        return true;
    }

    private static void HandleBadRequest(RemoteSignOutContext context)
    {
        context.HandleResponse();
        context.Response.StatusCode = 400;
        context.Response.Headers.CacheControl = "no-cache, no-store";
        context.Response.Headers.Pragma = "no-cache";
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
