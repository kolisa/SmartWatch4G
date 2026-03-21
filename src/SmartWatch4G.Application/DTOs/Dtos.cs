using System.Text.Json.Serialization;

namespace SmartWatch4G.Application.DTOs;

// ── Response ──────────────────────────────────────────────────────────────────

public sealed class ResponseCodeDto
{
    [JsonPropertyName("ReturnCode")]
    public int ReturnCode { get; set; }
}

// ── Call log upload ──────────────────────────────────────────────────────────

public sealed class CallRecordDto
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("call_number")]
    public string? CallNumber { get; set; }

    [JsonPropertyName("start_time")]
    public string? StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }
}

public sealed class CallLogWithAlarmDto
{
    [JsonPropertyName("alarm_time")]
    public string? AlarmTime { get; set; }

    [JsonPropertyName("lat")]
    public string? Lat { get; set; }

    [JsonPropertyName("lon")]
    public string? Lon { get; set; }

    [JsonPropertyName("call_logs")]
    public List<CallRecordDto>? CallLogs { get; set; }
}

public sealed class DeviceCallLogsDto
{
    [JsonPropertyName("deviceid")]
    public string? DeviceId { get; set; }

    [JsonPropertyName("normal_call_logs")]
    public List<CallRecordDto>? NormalCallLogs { get; set; }

    [JsonPropertyName("sos")]
    public List<CallLogWithAlarmDto>? Sos { get; set; }
}

// ── Device info upload ────────────────────────────────────────────────────────

public sealed class DeviceInfoDto
{
    [JsonPropertyName("deviceid")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("imsi")]
    public string Imsi { get; set; } = string.Empty;

    [JsonPropertyName("sn")]
    public string Sn { get; set; } = string.Empty;

    [JsonPropertyName("mac")]
    public string Mac { get; set; } = string.Empty;

    [JsonPropertyName("net_type")]
    public string NetType { get; set; } = string.Empty;

    [JsonPropertyName("net_operator")]
    public string NetOperator { get; set; } = string.Empty;

    [JsonPropertyName("wearing_status")]
    public string WearingStatus { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("sim1_iccid")]
    public string Sim1IccId { get; set; } = string.Empty;

    [JsonPropertyName("sim1_cellid")]
    public string Sim1CellId { get; set; } = string.Empty;

    [JsonPropertyName("sim1_netadhere")]
    public string Sim1NetAdhere { get; set; } = string.Empty;

    [JsonPropertyName("network_status")]
    public string NetworkStatus { get; set; } = string.Empty;

    [JsonPropertyName("band_detail")]
    public string BandDetail { get; set; } = string.Empty;

    [JsonPropertyName("refsignal")]
    public string RefSignal { get; set; } = string.Empty;

    [JsonPropertyName("band")]
    public string Band { get; set; } = string.Empty;

    [JsonPropertyName("communication_mode")]
    public string CommunicationMode { get; set; } = string.Empty;

    [JsonPropertyName("watch_event")]
    public int WatchEvent { get; set; }
}

// ── Device status upload ──────────────────────────────────────────────────────

public sealed class DeviceStatusDto
{
    [JsonPropertyName("DeviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("EventTime")]
    public string EventTime { get; set; } = string.Empty;

    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;
}

// ── Sleep query response ──────────────────────────────────────────────────────

public sealed class SleepResultDto
{
    [JsonPropertyName("deviceid")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("sleep_date")]
    public string SleepDate { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string EndTime { get; set; } = string.Empty;

    [JsonPropertyName("deep_sleep")]
    public int DeepSleep { get; set; }

    [JsonPropertyName("light_sleep")]
    public int LightSleep { get; set; }

    [JsonPropertyName("weak_sleep")]
    public int WeakSleep { get; set; }

    [JsonPropertyName("eyemove_sleep")]
    public int EyeMoveSleep { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("osahs_risk")]
    public int OsahsRisk { get; set; }

    [JsonPropertyName("spo2_score")]
    public int Spo2Score { get; set; }

    [JsonPropertyName("sleep_hr")]
    public int SleepHeartRate { get; set; }
}

public sealed class SleepResponseDto
{
    [JsonPropertyName("ReturnCode")]
    public int ReturnCode { get; set; }

    [JsonPropertyName("Data")]
    public SleepResultDto? Data { get; set; }
}
