using System.ComponentModel.DataAnnotations;

namespace SmartWatch4G.Application.DTOs;

public class SendUserInfoRequest
{
    [Range(50, 250)] public int Height       { get; set; }
    [Range(20, 300)] public int Weight       { get; set; }
    [Range(1, 2)]    public int Gender       { get; set; }
    [Range(0, 120)]  public int Age          { get; set; }
    [Range(80, 230)] public int WristCircle  { get; set; }
    public int Hypertension { get; set; }
}

public class SendMessageRequest
{
    [Required]
    [StringLength(15)]
    public string Title       { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Description { get; set; }
}

public class SetHrAlarmRequest
{
    public bool Open { get; set; }
    [Range(40, 220)] public int High                { get; set; }
    [Range(40, 220)] public int Low                 { get; set; }
    public int Threshold            { get; set; }
    public int AlarmIntervalMinutes { get; set; }
}

public class SetSpo2AlarmRequest
{
    public bool Open { get; set; }
    [Range(70, 99)] public int LowThreshold { get; set; }
}

public class PhonebookEntryRequest
{
    [Required]
    [StringLength(24)]
    public string Name   { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Number { get; set; }

    public bool IsSos { get; set; }
}

public class SetLanguageRequest
{
    [Range(0, 23)] public int LanguageCode { get; set; }
}

public class SetHrMeasureIntervalRequest
{
    [Range(1, 1440)] public int IntervalMinutes { get; set; }
}
