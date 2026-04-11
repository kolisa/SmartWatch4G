using System.ComponentModel.DataAnnotations;

using SmartWatch4G.Application.DTOs;

using Xunit;

namespace SmartWatch4G.UnitTests;

/// <summary>
/// Ensures that [Range], [Required], and [StringLength] attributes are wired up
/// correctly on request DTOs so that [ApiController] automatic ModelState validation
/// rejects bad values before they reach service or repository code.
/// </summary>
public sealed class CommandDtoValidationTests
{
    private static IList<ValidationResult> Validate(object dto)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(dto);
        Validator.TryValidateObject(dto, ctx, results, validateAllProperties: true);
        return results;
    }

    private static bool IsValid(object dto) => Validate(dto).Count == 0;

    // ── SendUserInfoRequest ───────────────────────────────────────────────────

    [Theory]
    [InlineData(50, 80, 1, 30, 150, 1)]   // lower bounds
    [InlineData(250, 300, 2, 120, 230, 2)] // upper bounds
    [InlineData(170, 70, 1, 35, 160, 2)]  // typical adult
    public void SendUserInfoRequest_ValidValues_PassesValidation(
        int height, int weight, int gender, int age, int wrist, int hypertension)
    {
        var dto = new SendUserInfoRequest
        {
            Height = height, Weight = weight, Gender = gender,
            Age = age, WristCircle = wrist, Hypertension = hypertension
        };
        Assert.True(IsValid(dto));
    }

    [Theory]
    [InlineData(49, 70, 1, 35, 160, 1)]   // height too low
    [InlineData(251, 70, 1, 35, 160, 1)]  // height too high
    [InlineData(170, 19, 1, 35, 160, 1)]  // weight too low
    [InlineData(170, 301, 1, 35, 160, 1)] // weight too high
    [InlineData(170, 70, 0, 35, 160, 1)]  // gender invalid (0)
    [InlineData(170, 70, 3, 35, 160, 1)]  // gender invalid (3)
    [InlineData(170, 70, 1, -1, 160, 1)]  // age negative
    [InlineData(170, 70, 1, 121, 160, 1)] // age too high
    [InlineData(170, 70, 1, 35, 79, 1)]   // wrist too small
    [InlineData(170, 70, 1, 35, 231, 1)]  // wrist too large
    public void SendUserInfoRequest_InvalidValues_FailsValidation(
        int height, int weight, int gender, int age, int wrist, int hypertension)
    {
        var dto = new SendUserInfoRequest
        {
            Height = height, Weight = weight, Gender = gender,
            Age = age, WristCircle = wrist, Hypertension = hypertension
        };
        Assert.False(IsValid(dto));
    }

    // ── SendMessageRequest ────────────────────────────────────────────────────

    [Fact]
    public void SendMessageRequest_ValidTitleAndBody_PassesValidation()
    {
        var dto = new SendMessageRequest { Title = "Alert", Description = "You have a message." };
        Assert.True(IsValid(dto));
    }

    [Fact]
    public void SendMessageRequest_EmptyTitle_FailsValidation()
    {
        var dto = new SendMessageRequest { Title = string.Empty, Description = "body" };
        Assert.False(IsValid(dto));
    }

    [Fact]
    public void SendMessageRequest_TitleExceeds15Chars_FailsValidation()
    {
        var dto = new SendMessageRequest { Title = new string('A', 16), Description = "body" };
        Assert.False(IsValid(dto));
    }

    [Fact]
    public void SendMessageRequest_DescriptionExceeds240Chars_FailsValidation()
    {
        var dto = new SendMessageRequest { Title = "Hi", Description = new string('X', 241) };
        Assert.False(IsValid(dto));
    }

    // ── SetHrAlarmRequest ─────────────────────────────────────────────────────

    [Fact]
    public void SetHrAlarmRequest_ValidThresholds_PassesValidation()
    {
        var dto = new SetHrAlarmRequest
        {
            Open = true, High = 120, Low = 50, Threshold = 3, AlarmIntervalMinutes = 5
        };
        Assert.True(IsValid(dto));
    }

    [Theory]
    [InlineData(39, 50)]   // high too low
    [InlineData(221, 50)]  // high too high
    [InlineData(120, 39)]  // low too low
    [InlineData(120, 221)] // low too high
    public void SetHrAlarmRequest_OutOfRangeThresholds_FailsValidation(int high, int low)
    {
        var dto = new SetHrAlarmRequest
        {
            Open = true, High = high, Low = low, Threshold = 3, AlarmIntervalMinutes = 5
        };
        Assert.False(IsValid(dto));
    }

    // ── SetSpo2AlarmRequest ───────────────────────────────────────────────────

    [Fact]
    public void SetSpo2AlarmRequest_ValidThreshold_PassesValidation()
    {
        var dto = new SetSpo2AlarmRequest { Open = true, LowThreshold = 90 };
        Assert.True(IsValid(dto));
    }

    [Theory]
    [InlineData(69)]
    [InlineData(100)]
    public void SetSpo2AlarmRequest_InvalidThreshold_FailsValidation(int threshold)
    {
        var dto = new SetSpo2AlarmRequest { Open = true, LowThreshold = threshold };
        Assert.False(IsValid(dto));
    }

    // ── SyncPhonebookRequest / PhonebookEntryRequest ──────────────────────────

    [Fact]
    public void PhonebookEntryRequest_EmptyName_FailsValidation()
    {
        var dto = new PhonebookEntryRequest { Name = string.Empty, Number = "0821234567" };
        Assert.False(IsValid(dto));
    }

    [Fact]
    public void PhonebookEntryRequest_NameExceeds24Chars_FailsValidation()
    {
        var dto = new PhonebookEntryRequest
        {
            Name = new string('A', 25),
            Number = "0821234567"
        };
        Assert.False(IsValid(dto));
    }

    [Fact]
    public void PhonebookEntryRequest_NumberExceeds20Chars_FailsValidation()
    {
        var dto = new PhonebookEntryRequest
        {
            Name = "Mom",
            Number = new string('1', 21)
        };
        Assert.False(IsValid(dto));
    }

    [Fact]
    public void PhonebookEntryRequest_ValidEntry_PassesValidation()
    {
        var dto = new PhonebookEntryRequest { Name = "Mom", Number = "0821234567", IsSos = true };
        Assert.True(IsValid(dto));
    }

    // ── SetLanguageRequest ────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(23)]
    public void SetLanguageRequest_ValidCode_PassesValidation(int code)
    {
        var dto = new SetLanguageRequest { LanguageCode = code };
        Assert.True(IsValid(dto));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    public void SetLanguageRequest_InvalidCode_FailsValidation(int code)
    {
        var dto = new SetLanguageRequest { LanguageCode = code };
        Assert.False(IsValid(dto));
    }

    // ── SetHrMeasureIntervalRequest ───────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(1441)]
    public void SetHrMeasureIntervalRequest_OutOfRange_FailsValidation(int minutes)
    {
        var dto = new SetHrMeasureIntervalRequest { IntervalMinutes = minutes };
        Assert.False(IsValid(dto));
    }

    [Fact]
    public void SetHrMeasureIntervalRequest_ValidInterval_PassesValidation()
    {
        var dto = new SetHrMeasureIntervalRequest { IntervalMinutes = 5 };
        Assert.True(IsValid(dto));
    }
}
