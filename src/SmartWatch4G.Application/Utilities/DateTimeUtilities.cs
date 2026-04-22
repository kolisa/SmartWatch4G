using System.Globalization;

namespace SmartWatch4G.Application.Utilities;

public static class DateTimeUtilities
{
    private const string DateFmt     = "yyyy-MM-dd";
    private const string DateTimeFmt = "yyyy-MM-dd HH:mm:ss";

    public static string FromUnixSeconds(long seconds)
    {
        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                                 .ToString(DateTimeFmt);
        }
        catch { return string.Empty; }
    }

    public static bool IsValidDate(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return false;
        return DateTime.TryParseExact(dateStr, DateFmt,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    public static bool IsValidDateTime(string? dateTimeStr)
    {
        if (string.IsNullOrEmpty(dateTimeStr)) return false;
        return DateTime.TryParseExact(dateTimeStr, DateTimeFmt,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    public static DateTime? TryParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        if (!DateTime.TryParseExact(dateStr, DateFmt,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return null;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public static DateTime? TryParseDateTime(string? dateTimeStr)
    {
        if (string.IsNullOrWhiteSpace(dateTimeStr)) return null;
        if (!DateTime.TryParseExact(dateTimeStr, DateTimeFmt,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return null;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public static TimeZoneInfo? TryGetTimeZone(string? zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId)) return null;
        try { return TimeZoneInfo.FindSystemTimeZoneById(zoneId); }
        catch { return null; }
    }

    public static string LocalizeTimestamp(string? timestamp, TimeZoneInfo? tz)
    {
        if (string.IsNullOrEmpty(timestamp)) return string.Empty;
        if (tz is null) return timestamp;
        var parsed = TryParseDateTime(timestamp);
        if (parsed is null) return timestamp;
        return LocalizeDateTime(parsed.Value, tz);
    }

    public static string LocalizeDateTime(DateTime utc, TimeZoneInfo? tz)
    {
        if (tz is null) return utc.ToString(DateTimeFmt);
        var local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc), tz);
        return local.ToString(DateTimeFmt);
    }

    public static (string from, string to) ToDayRange(string? dateStr)
    {
        if (!IsValidDate(dateStr)) return (string.Empty, string.Empty);
        return ($"{dateStr} 00:00:00", $"{dateStr} 23:59:59");
    }

    public static string GetPreviousDay(string dateStr)
    {
        if (!IsValidDate(dateStr)) return string.Empty;
        var date = DateTime.ParseExact(dateStr, DateFmt, CultureInfo.InvariantCulture);
        return date.AddDays(-1).ToString(DateFmt);
    }
}
