﻿namespace MyAppWithMud.Components.BrowserTime;

public static class TimeProviderExtensions
{
    public static DateTime ToLocal(this BrowserTimeProvider timeProvider, DateTimeOffset utcDateTimeOffset)
    {
        var dateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTimeOffset.UtcDateTime, timeProvider.LocalTimeZone);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

        return dateTime;
    }

    public static DateTimeOffset? ToLocalDateTimeOffset(this BrowserTimeProvider timeProvider, DateTimeOffset? utcDateTimeOffset)
    {
        if (utcDateTimeOffset is null) return null;

        return timeProvider.ToLocalDateTimeOffset(utcDateTimeOffset.Value);
    }

    public static DateTimeOffset ToLocalDateTimeOffset(this BrowserTimeProvider timeProvider, DateTimeOffset utcDateTimeOffset)
    {
        return TimeZoneInfo.ConvertTime(utcDateTimeOffset, timeProvider.LocalTimeZone);
    }

    public static DateTime ToLocal(this BrowserTimeProvider timeProvider, DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime;
        }
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            throw new InvalidOperationException("Unable to convert unspecified DateTime to local time.");
        }

        var local = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeProvider.LocalTimeZone);
        local = DateTime.SpecifyKind(local, DateTimeKind.Local);

        return local;
    }
}
