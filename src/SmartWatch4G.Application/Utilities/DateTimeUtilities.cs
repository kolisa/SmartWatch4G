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
}
