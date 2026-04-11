using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Injectable implementation of <see cref="IDateTimeService"/> that delegates
/// to the <see cref="DateTimeUtilities"/> static helpers.
/// Registered as a singleton — all methods are stateless.
/// </summary>
internal sealed class DateTimeService : IDateTimeService
{
    public string FromUnixSeconds(long seconds)
        => DateTimeUtilities.FromUnixSeconds(seconds);

    public bool IsValidDate(string? dateStr)
        => DateTimeUtilities.IsValidDate(dateStr);

    public bool IsValidDateTime(string? dateTimeStr)
        => DateTimeUtilities.IsValidDateTime(dateTimeStr);

    public (string From, string To) ToDayRange(string? dateStr)
        => DateTimeUtilities.ToDayRange(dateStr);

    public string GetPreviousDay(string dateStr)
        => DateTimeUtilities.GetPreviousDay(dateStr);

    public System.DateTime? TryParseDate(string? dateStr)
    {
        return DateTimeUtilities.TryParseDate(dateStr);
    }

    public System.DateTime? TryParseDateTime(string? dateTimeStr)
    {
        return DateTimeUtilities.TryParseDateTime(dateTimeStr);
    }

    public TimeZoneInfo? TryGetTimeZone(string? tzId)
        => DateTimeUtilities.TryGetTimeZone(tzId);

    public string LocalizeTimestamp(string? timestamp, TimeZoneInfo? tz)
        => DateTimeUtilities.LocalizeTimestamp(timestamp, tz);

    public string LocalizeDateTime(System.DateTime utcDateTime, TimeZoneInfo? tz)
    {
        return DateTimeUtilities.LocalizeDateTime(utcDateTime, tz);
    }
}
