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

    // Constants for claim types and event URIs
    private const string LogoutTokenParameter = "logout_token";
    private const string EventsClaim = "events";
    private const string SubClaim = "sub";
    private const string SidClaim = "sid";
    private const string BackchannelLogoutEvent = "http://schemas.openid.net/event/backchannel-logout";

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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during RemoteSignOut");
            HandleBadRequest(context);
        }
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
        if (!context.ProtocolMessage.Parameters.TryGetValue(LogoutTokenParameter, out token))
        {
            _logger.LogError("{LogoutTokenParameter} must not be null", LogoutTokenParameter);
            HandleBadRequest(context);
            return false;
        }
        return true;
    }

    private async Task<ClaimsPrincipal?> ValidateLogoutTokenAsync(RemoteSignOutContext context, string token)
    {
        try
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
                _logger.LogError(tokenValidationResult.Exception, "Error validating logout token");
                HandleBadRequest(context);
                return null;
            }
            return new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ValidateLogoutTokenAsync");
            HandleBadRequest(context);
            return null;
        }
    }

    private bool HasValidLogoutEvent(ClaimsPrincipal principal, RemoteSignOutContext context)
    {
        var events = principal.FindFirst(EventsClaim)?.Value;
        if (string.IsNullOrWhiteSpace(events))
        {
            _logger.LogError("{EventsClaim} claim is required", EventsClaim);
            HandleBadRequest(context);
            return false;
        }
        if (!events.Contains(BackchannelLogoutEvent))
        {
            _logger.LogError("{EventsClaim} must contain '{BackchannelLogoutEvent}'", EventsClaim, BackchannelLogoutEvent);
            HandleBadRequest(context);
            return false;
        }
        return true;
    }

    private bool HasSubOrSid(ClaimsPrincipal principal, RemoteSignOutContext context)
    {
        var sub = principal.FindFirst(SubClaim)?.Value;
        var sid = principal.FindFirst(SidClaim)?.Value;
        if (string.IsNullOrEmpty(sub) && string.IsNullOrEmpty(sid))
        {
            _logger.LogError("Token must contain '{SubClaim}' or '{SidClaim}'", SubClaim, SidClaim);
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
        if (context.TokenEndpointResponse == null)
        {
            _logger.LogError("TokenEndpointResponse is null in TokenValidated");
            return;
        }

        if (!double.TryParse(context.TokenEndpointResponse.ExpiresIn, out var expiresInSeconds))
        {
            _logger.LogError("ExpiresIn is not a valid number: {ExpiresIn}", context.TokenEndpointResponse.ExpiresIn);
            return;
        }

        var exp = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);

        try
        {
            await _store.StoreTokenAsync(context.Principal!, new UserToken
            {
                AccessToken = context.TokenEndpointResponse.AccessToken,
                AccessTokenType = context.TokenEndpointResponse.TokenType,
                Expiration = exp,
                RefreshToken = context.TokenEndpointResponse.RefreshToken,
                Scope = context.TokenEndpointResponse.Scope
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception storing token in TokenValidated");
        }

        await base.TokenValidated(context);
    }
}
