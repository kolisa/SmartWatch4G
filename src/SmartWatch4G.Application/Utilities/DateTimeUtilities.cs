using System.Globalization;

namespace SmartWatch4G.Application.Utilities;

/// <summary>
/// Date/time helpers used throughout the application.
/// Migrated from the original <c>MyDateTimeUtils</c> static class.
/// </summary>
public static class DateTimeUtilities
{
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Converts a Unix epoch (seconds) to a UTC datetime string formatted as
    /// <c>yyyy-MM-dd HH:mm:ss</c>.  Returns <see cref="string.Empty"/> on failure.
    /// </summary>
    public static string FromUnixSeconds(long seconds)
    {
        try
        {
            DateTime dt = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            return dt.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        }
        catch (ArgumentOutOfRangeException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="dateStr"/> is a valid
    /// <c>yyyy-MM-dd</c> date string.
    /// </summary>
    public static bool IsValidDate(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr))
        {
            return false;
        }

        return DateTime.TryParseExact(
            dateStr,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="dateTimeStr"/> is a valid
    /// <c>yyyy-MM-dd HH:mm:ss</c> datetime string.
    /// </summary>
    public static bool IsValidDateTime(string? dateTimeStr)
    {
        if (string.IsNullOrEmpty(dateTimeStr))
        {
            return false;
        }

        return DateTime.TryParseExact(
            dateTimeStr,
            TimestampFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }

    /// <summary>
    /// Converts a <c>yyyy-MM-dd</c> date string into an inclusive timestamp range
    /// (<c>yyyy-MM-dd 00:00:00</c> → <c>yyyy-MM-dd 23:59:59</c>) suitable for
    /// string-comparison queries against stored <c>DataTime</c> fields.
    /// Returns <c>(string.Empty, string.Empty)</c> when the input is invalid.
    /// </summary>
    public static (string From, string To) ToDayRange(string? dateStr)
    {
        if (!IsValidDate(dateStr))
        {
            return (string.Empty, string.Empty);
        }

        return ($"{dateStr} 00:00:00", $"{dateStr} 23:59:59");
    }

    /// <summary>
    /// Returns the calendar date that precedes <paramref name="dateStr"/>
    /// (formatted <c>yyyy-MM-dd</c>), or <see cref="string.Empty"/> when
    /// the input is invalid.
    /// </summary>
    public static string GetPreviousDay(string dateStr)
    {
        if (!IsValidDate(dateStr))
        {
            return string.Empty;
        }

        DateTime date = DateTime.ParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture);
        return date.AddDays(-1).ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    // ── Safe UTC parsers ─────────────────────────────────────────────────────

    /// <summary>
    /// Converts a valid <c>yyyy-MM-dd</c> date string to a UTC <see cref="DateTime"/>.
    /// Returns <c>null</c> when the input is invalid instead of throwing.
    /// </summary>
    public static DateTime? TryParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        if (!DateTime.TryParseExact(dateStr, DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)) return null;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    /// <summary>
    /// Converts a valid <c>yyyy-MM-dd HH:mm:ss</c> datetime string to a UTC <see cref="DateTime"/>.
    /// Returns <c>null</c> when the input is invalid instead of throwing.
    /// </summary>
    public static DateTime? TryParseDateTime(string? dateTimeStr)
    {
        if (string.IsNullOrWhiteSpace(dateTimeStr)) return null;
        if (!DateTime.TryParseExact(dateTimeStr, TimestampFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)) return null;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    // ── Timezone localization ─────────────────────────────────────────────────

    /// <summary>
    /// Tries to find a <see cref="TimeZoneInfo"/> from an IANA or Windows timezone ID.
    /// Returns <c>null</c> when the ID is blank or unknown.
    /// </summary>
    public static TimeZoneInfo? TryGetTimeZone(string? tzId)
    {
        if (string.IsNullOrWhiteSpace(tzId))
        {
            return null;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(tzId, out TimeZoneInfo? tz))
        {
            return tz;
        }

        return null;
    }

    /// <summary>
    /// Converts a UTC timestamp string (<c>yyyy-MM-dd HH:mm:ss</c>) to the given timezone.
    /// Returns the original string unchanged when <paramref name="tz"/> is <c>null</c>
    /// or the string cannot be parsed.
    /// </summary>
    public static string LocalizeTimestamp(string? timestamp, TimeZoneInfo? tz)
    {
        if (tz is null || string.IsNullOrEmpty(timestamp))
        {
            return timestamp ?? string.Empty;
        }

        if (!DateTime.TryParseExact(timestamp, TimestampFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime utcDt))
        {
            return timestamp;
        }

        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDt, DateTimeKind.Utc), tz);

        return local.ToString(TimestampFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a UTC <see cref="DateTime"/> to the given timezone and returns it
    /// as a <c>yyyy-MM-dd HH:mm:ss</c> string.
    /// Returns UTC when <paramref name="tz"/> is <c>null</c>.
    /// </summary>
    public static string LocalizeDateTime(DateTime utcDateTime, TimeZoneInfo? tz)
    {
        if (tz is null)
        {
            return utcDateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        }

        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), tz);

        return local.ToString(TimestampFormat, CultureInfo.InvariantCulture);
    }
}
