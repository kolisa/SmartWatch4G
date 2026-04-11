namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Abstracts date/time formatting and timezone operations so that consumers
/// can depend on an interface rather than the concrete static utility class.
/// </summary>
public interface IDateTimeService
{
    /// <summary>Converts a Unix epoch (seconds) to a UTC <c>yyyy-MM-dd HH:mm:ss</c> string.</summary>
    string FromUnixSeconds(long seconds);

    /// <summary>Returns <c>true</c> when <paramref name="dateStr"/> is a valid <c>yyyy-MM-dd</c> string.</summary>
    bool IsValidDate(string? dateStr);

    /// <summary>Returns <c>true</c> when <paramref name="dateTimeStr"/> is a valid <c>yyyy-MM-dd HH:mm:ss</c> string.</summary>
    bool IsValidDateTime(string? dateTimeStr);

    /// <summary>Maps a <c>yyyy-MM-dd</c> date to an inclusive <c>(from, to)</c> timestamp range.</summary>
    (string From, string To) ToDayRange(string? dateStr);

    /// <summary>Returns the calendar day preceding <paramref name="dateStr"/>.</summary>
    string GetPreviousDay(string dateStr);

    /// <summary>Parses a <c>yyyy-MM-dd</c> string to UTC <see cref="DateTime"/>; returns <c>null</c> on failure.</summary>
    DateTime? TryParseDate(string? dateStr);

    /// <summary>Parses a <c>yyyy-MM-dd HH:mm:ss</c> string to UTC <see cref="DateTime"/>; returns <c>null</c> on failure.</summary>
    DateTime? TryParseDateTime(string? dateTimeStr);

    /// <summary>Resolves a timezone ID (IANA or Windows) to <see cref="TimeZoneInfo"/>; returns <c>null</c> when unknown.</summary>
    TimeZoneInfo? TryGetTimeZone(string? tzId);

    /// <summary>Converts a UTC timestamp string to the given timezone; returns the original string when <paramref name="tz"/> is <c>null</c>.</summary>
    string LocalizeTimestamp(string? timestamp, TimeZoneInfo? tz);

    /// <summary>Converts a UTC <see cref="DateTime"/> to the given timezone and formats it as <c>yyyy-MM-dd HH:mm:ss</c>.</summary>
    string LocalizeDateTime(DateTime utcDateTime, TimeZoneInfo? tz);
}
