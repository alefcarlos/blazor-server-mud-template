using Microsoft.AspNetCore.HttpLogging;

namespace MyAppWithMud.Web;

public class FilterRequestLoggingInterceptor : IHttpLoggingInterceptor
{
    private static bool ShouldFilterEndpoint(HttpContext context, string path)
    {
        if (!context.Request.Path.HasValue)
            return false;

        return context.Request.Path.Value.Contains(path, StringComparison.InvariantCultureIgnoreCase);
    }

    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        if (ShouldFilterEndpoint(logContext.HttpContext, "/metrics"))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        if (ShouldFilterEndpoint(logContext.HttpContext, "/env"))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        if (ShouldFilterEndpoint(logContext.HttpContext, "/health"))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        if (ShouldFilterEndpoint(logContext.HttpContext, "/alive"))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        if (ShouldFilterEndpoint(logContext.HttpContext, "/docs"))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        if (ShouldFilterEndpoint(logContext.HttpContext, "/swagger"))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        return default;
    }
}
