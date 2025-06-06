using System.Collections.Concurrent;
using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;

namespace MyAppWithMud.Web.SessionManagement;

/// <summary>
/// Simplified implementation of a server-side token store.
/// Probably want something more robust IRL
/// https://github.com/DuendeSoftware/foss/blob/main/access-token-management/samples/BlazorServer/Plumbing/ServerSideTokenStore.cs
/// </summary>
public class ServerSideTokenStore : IUserTokenStore
{
    private static readonly ConcurrentDictionary<string, UserToken> _tokens = new();

    public Task<UserToken> GetTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        if (_tokens.TryGetValue(sub, out var value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult(new UserToken { Error = "not found" });
    }

    public Task StoreTokenAsync(ClaimsPrincipal user, UserToken token, UserTokenRequestParameters? parameters = null)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        _tokens[sub] = token;

        return Task.CompletedTask;
    }

    public Task ClearTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        if (_tokens.TryRemove(sub, out var token) && token != null)
        {
            // Zero out sensitive fields if present
            try
            {
                var type = token.GetType();
                var accessTokenProp = type.GetProperty("AccessToken");
                var refreshTokenProp = type.GetProperty("RefreshToken");
                var idTokenProp = type.GetProperty("IdToken");

                accessTokenProp?.SetValue(token, string.Empty);
                refreshTokenProp?.SetValue(token, string.Empty);
                idTokenProp?.SetValue(token, string.Empty);
            }
            catch
            {
                // Ignore errors in zeroing out for safety
            }
        }
        return Task.CompletedTask;
    }
}
