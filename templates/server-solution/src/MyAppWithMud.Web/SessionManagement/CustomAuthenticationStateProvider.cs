using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace MyAppWithMud.Web.SessionManagement;

internal class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IUserTokenStore _userTokenStore;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(ILoggerFactory loggerFactory, IUserTokenStore userTokenStore, ILogger<CustomAuthenticationStateProvider> logger) : base(loggerFactory)
    {
        _userTokenStore = userTokenStore;
        _logger = logger;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(10);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var user = authenticationState.User;

        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            return false;
        }

        //Validar se alguma sessão desse usuário foi encerrada
        var anyToken = await _userTokenStore.GetTokenAsync(user);
        if (anyToken.IsError)
        {
            var userName = user.FindFirst(JwtClaimTypes.Name)?.Value ??
               user.FindFirst(JwtClaimTypes.Subject)?.Value ?? "unknown";
            _logger.LogInformation("Não existe token para o usuário {username}. Invalidando sessão", userName);
            return false;
        }

        return true;
    }
}
