using Microsoft.Extensions.Logging;

namespace Alef.Components.BrowserTime;

public sealed class BrowserTimeProvider : TimeProvider
{
    private readonly ILogger _logger;
    private TimeZoneInfo? _browserLocalTimeZone;

    public BrowserTimeProvider(ILogger<BrowserTimeProvider> logger)
    {
        _logger = logger;
    }

    public override TimeZoneInfo LocalTimeZone
    {
        get => _browserLocalTimeZone ?? base.LocalTimeZone;
    }

    public void SetBrowserTimeZone(string timeZone)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out var timeZoneInfo))
        {
            _logger.LogWarning("Couldn't find a time zone '{TimeZone}'. Defaulting to UTC.", timeZone);
            timeZoneInfo = TimeZoneInfo.Utc;
        }

        _logger.LogInformation("Browser time zone set to '{TimeZone}' with UTC offset {UtcOffset}.", timeZoneInfo.Id, timeZoneInfo.BaseUtcOffset);
        _browserLocalTimeZone = timeZoneInfo;
    }
}
