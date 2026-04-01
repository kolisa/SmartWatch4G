using SmartWatch4G.Application.Utilities;

using Xunit;

namespace SmartWatch4G.UnitTests;

public sealed class DateTimeUtilitiesTests
{
    // ── IsValidDate ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("2024-01-15", true)]
    [InlineData("2024-12-31", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("2024-1-1", false)]
    [InlineData("01-01-2024", false)]
    [InlineData("not-a-date", false)]
    public void IsValidDate_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, DateTimeUtilities.IsValidDate(input));
    }

    // ── IsValidDateTime ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("2024-01-15 08:30:00", true)]
    [InlineData("2024-12-31 23:59:59", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("2024-01-15", false)]
    [InlineData("2024-01-15T08:30:00", false)]
    public void IsValidDateTime_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, DateTimeUtilities.IsValidDateTime(input));
    }

    // ── TryParseDate ─────────────────────────────────────────────────────────

    [Fact]
    public void TryParseDate_ValidDate_ReturnsUtcDateTime()
    {
        System.DateTime? result = DateTimeUtilities.TryParseDate("2024-06-15");

        Assert.NotNull(result);
        Assert.Equal(2024, result!.Value.Year);
        Assert.Equal(6, result.Value.Month);
        Assert.Equal(15, result.Value.Day);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("2024-6-15")]
    [InlineData("not-a-date")]
    public void TryParseDate_Invalid_ReturnsNull(string? input)
    {
        Assert.Null(DateTimeUtilities.TryParseDate(input));
    }

    // ── TryParseDateTime ─────────────────────────────────────────────────────

    [Fact]
    public void TryParseDateTime_ValidTimestamp_ReturnsUtcDateTime()
    {
        System.DateTime? result = DateTimeUtilities.TryParseDateTime("2024-06-15 14:30:00");

        Assert.NotNull(result);
        Assert.Equal(2024, result!.Value.Year);
        Assert.Equal(14, result.Value.Hour);
        Assert.Equal(30, result.Value.Minute);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2024-06-15")]
    [InlineData("2024-06-15T14:30:00")]
    public void TryParseDateTime_Invalid_ReturnsNull(string? input)
    {
        Assert.Null(DateTimeUtilities.TryParseDateTime(input));
    }

    // ── TryGetTimeZone ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Invalid/Zone")]
    public void TryGetTimeZone_Invalid_ReturnsNull(string? input)
    {
        Assert.Null(DateTimeUtilities.TryGetTimeZone(input));
    }

    [Fact]
    public void TryGetTimeZone_UtcId_ReturnsUtcZone()
    {
        System.TimeZoneInfo? tz = DateTimeUtilities.TryGetTimeZone("UTC");

        Assert.NotNull(tz);
        Assert.Equal(TimeSpan.Zero, tz!.BaseUtcOffset);
    }

    // ── LocalizeTimestamp ────────────────────────────────────────────────────

    [Fact]
    public void LocalizeTimestamp_NullTz_ReturnsOriginal()
    {
        string result = DateTimeUtilities.LocalizeTimestamp("2024-01-15 10:00:00", null);
        Assert.Equal("2024-01-15 10:00:00", result);
    }

    [Fact]
    public void LocalizeTimestamp_NullOrEmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, DateTimeUtilities.LocalizeTimestamp(null, null));
        Assert.Equal(string.Empty, DateTimeUtilities.LocalizeTimestamp(null, System.TimeZoneInfo.Utc));
    }

    [Fact]
    public void LocalizeTimestamp_InvalidInput_ReturnsOriginal()
    {
        Assert.Equal("not-a-timestamp", DateTimeUtilities.LocalizeTimestamp("not-a-timestamp", System.TimeZoneInfo.Utc));
    }

    [Fact]
    public void LocalizeTimestamp_UtcTz_ReturnsSameValue()
    {
        string result = DateTimeUtilities.LocalizeTimestamp("2024-01-15 10:00:00", System.TimeZoneInfo.Utc);
        Assert.Equal("2024-01-15 10:00:00", result);
    }

    [Fact]
    public void LocalizeTimestamp_OffsetTz_ShiftsTime()
    {
        // UTC+2 zone (try both Windows and IANA IDs)
        System.TimeZoneInfo? tz = DateTimeUtilities.TryGetTimeZone("South Africa Standard Time")
                                  ?? DateTimeUtilities.TryGetTimeZone("Africa/Johannesburg");

        if (tz is null) return; // skip on systems without this zone

        string result = DateTimeUtilities.LocalizeTimestamp("2024-01-15 08:00:00", tz);
        Assert.Equal("2024-01-15 10:00:00", result);
    }

    // ── LocalizeDateTime ─────────────────────────────────────────────────────

    [Fact]
    public void LocalizeDateTime_NullTz_ReturnsUtcFormatted()
    {
        var utc = new System.DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        string result = DateTimeUtilities.LocalizeDateTime(utc, null);
        Assert.Equal("2024-06-15 12:00:00", result);
    }

    [Fact]
    public void LocalizeDateTime_UtcTz_ReturnsSameFormatted()
    {
        var utc = new System.DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        string result = DateTimeUtilities.LocalizeDateTime(utc, System.TimeZoneInfo.Utc);
        Assert.Equal("2024-06-15 12:00:00", result);
    }

    [Fact]
    public void LocalizeDateTime_OffsetTz_ShiftsTime()
    {
        System.TimeZoneInfo? tz = DateTimeUtilities.TryGetTimeZone("South Africa Standard Time")
                                  ?? DateTimeUtilities.TryGetTimeZone("Africa/Johannesburg");

        if (tz is null) return;

        var utc = new System.DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        string result = DateTimeUtilities.LocalizeDateTime(utc, tz);
        Assert.Equal("2024-01-15 10:00:00", result);
    }

    // ── ToDayRange ───────────────────────────────────────────────────────────

    [Fact]
    public void ToDayRange_ValidDate_ReturnsFullDayBounds()
    {
        (string from, string to) = DateTimeUtilities.ToDayRange("2024-03-10");
        Assert.Equal("2024-03-10 00:00:00", from);
        Assert.Equal("2024-03-10 23:59:59", to);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("bad")]
    public void ToDayRange_Invalid_ReturnsEmptyPair(string? input)
    {
        (string from, string to) = DateTimeUtilities.ToDayRange(input);
        Assert.Equal(string.Empty, from);
        Assert.Equal(string.Empty, to);
    }

    // ── GetPreviousDay ────────────────────────────────────────────────────────

    [Fact]
    public void GetPreviousDay_ValidDate_ReturnsPreviousDate()
    {
        Assert.Equal("2024-01-14", DateTimeUtilities.GetPreviousDay("2024-01-15"));
        Assert.Equal("2023-12-31", DateTimeUtilities.GetPreviousDay("2024-01-01"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad")]
    public void GetPreviousDay_Invalid_ReturnsEmpty(string input)
    {
        Assert.Equal(string.Empty, DateTimeUtilities.GetPreviousDay(input));
    }

    // ── FromUnixSeconds ──────────────────────────────────────────────────────

    [Fact]
    public void FromUnixSeconds_KnownEpoch_ReturnsExpectedString()
    {
        string result = DateTimeUtilities.FromUnixSeconds(0);
        Assert.Equal("1970-01-01 00:00:00", result);
    }
}
