using Microsoft.Extensions.DependencyInjection;

namespace MyAppWithMud.Components.BrowserTime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrowserTimeProvider(this IServiceCollection services)
    {
        // Time zone is set by the browser.
        services.AddScoped<BrowserTimeProvider>();

        return services;
    }
}
