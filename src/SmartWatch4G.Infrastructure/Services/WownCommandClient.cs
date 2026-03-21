using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartWatch4G.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// HTTP client for the iwown entservice — sends commands to devices and
/// queries device status.
///
/// China Mainland : https://search.iwown.com
/// Rest of world  : https://euapi.iwown.com
///
/// Authentication is via HTTP headers: account + pwd (MD5 of password).
/// </summary>
public sealed class WownCommandClient : IWownCommandClient
{
    private readonly HttpClient _http;
    private readonly WownCommandOptions _options;
    private readonly ILogger<WownCommandClient> _logger;

    public WownCommandClient(
        HttpClient http,
        IOptions<WownCommandOptions> options,
        ILogger<WownCommandClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    // ── User info ─────────────────────────────────────────────────────────────

    public Task<CommandResult> SendUserInfoAsync(UserInfoCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            height = cmd.Height,
            weight = cmd.Weight,
            gender = cmd.Gender,
            age = cmd.Age,
            calibrate_walk = 100,
            calibrate_run = 100,
            wrist_circle = cmd.WristCircle > 0 ? (int?)cmd.WristCircle : null,
            hypertension = cmd.Hypertension > 0 ? (int?)cmd.Hypertension : null
        };
        return PostAsync("/entservice/cmd/userinfo", body, ct);
    }

    // ── Location / sync ───────────────────────────────────────────────────────

    public Task<CommandResult> RequestRealtimeLocationAsync(string deviceId, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/realtime/location", new { device_id = deviceId }, ct);

    public Task<CommandResult> TriggerDataSyncAsync(string deviceId, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/datasync", new { device_id = deviceId }, ct);

    // ── Device status ─────────────────────────────────────────────────────────

    public async Task<DeviceOnlineStatus?> GetDeviceStatusAsync(string deviceId, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/entservice/device/status?device_id={Uri.EscapeDataString(deviceId)}");

            AddAuthHeaders(request);
            var resp = await _http.SendAsync(request, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var result = await resp.Content
                .ReadFromJsonAsync<EntServiceResponse<DeviceStatusData>>(cancellationToken: ct)
                .ConfigureAwait(false);

            if (result?.ReturnCode != 0 || result.Data is null) return null;
            return new DeviceOnlineStatus(deviceId, result.Data.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError("GetDeviceStatus failed for {DeviceId}: {Msg}", deviceId, ex.Message);
            return null;
        }
    }

    // ── Device settings ───────────────────────────────────────────────────────

    public Task<CommandResult> SetFallDetectionAsync(string deviceId, bool enabled, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/fallcheck", new { device_id = deviceId, fall_check = enabled }, ct);

    public Task<CommandResult> SyncPhonebookAsync(PhonebookCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            phone_book = cmd.PhoneBook.Select(e => new { name = e.Name, number = e.Number, sos = e.IsSos }),
            forbid = cmd.Forbid
        };
        return PostAsync("/entservice/phonebook/sync", body, ct);
    }

    public Task<CommandResult> ClearPhonebookAsync(string deviceId, CancellationToken ct = default)
        => PostAsync("/entservice/phonebook/clear", new { device_id = deviceId }, ct);

    public Task<CommandResult> SetDataFrequencyAsync(DataFrequencyCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            gps_auto_check = cmd.GpsAutoCheck,
            gps_interval_time = cmd.GpsIntervalMinutes,
            power_mode = cmd.PowerMode
        };
        return PostAsync("/entservice/cmd/datafreq", body, ct);
    }

    public Task<CommandResult> SetLocateDataUploadFrequencyAsync(LocateDataUploadCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            data_auto_upload = cmd.DataAutoUpload,
            data_upload_interval = cmd.DataUploadIntervalMinutes,
            auto_locate = cmd.AutoLocate,
            locate_interval_time = cmd.LocateIntervalMinutes,
            power_mode = cmd.PowerMode
        };
        return PostAsync("/entservice/cmd/locate_dataupload/freq", body, ct);
    }

    public Task<CommandResult> SetWristGestureAsync(WristGestureCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            open = cmd.Open,
            start_hour = cmd.StartHour,
            end_hour = cmd.EndHour
        };
        return PostAsync("/entservice/cmd/lcdgesture", body, ct);
    }

    public Task<CommandResult> SetHrAlarmAsync(HrAlarmCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            open = cmd.Open,
            high = cmd.High,
            low = cmd.Low,
            threshold = cmd.Threshold,
            alarm_interval = cmd.AlarmIntervalMinutes
        };
        return PostAsync("/entservice/cmd/hralarm", body, ct);
    }

    public Task<CommandResult> SetDynamicHrAlarmAsync(DynamicHrAlarmCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            open = cmd.Open,
            high = cmd.High,
            low = cmd.Low,
            timeout = cmd.TimeoutSeconds,
            interval = cmd.IntervalMinutes
        };
        return PostAsync("/entservice/cmd/dynamic/hralarm", body, ct);
    }

    public Task<CommandResult> SetSpo2AlarmAsync(string deviceId, bool open, int lowThreshold, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/spo2alarm", new { device_id = deviceId, open, low = lowThreshold }, ct);

    public Task<CommandResult> SetBpAlarmAsync(BpAlarmCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            open = cmd.Open,
            sbp_high = cmd.SbpHigh,
            sbp_below = cmd.SbpBelow,
            dbp_high = cmd.DbpHigh,
            dbp_below = cmd.DbpBelow
        };
        return PostAsync("/entservice/cmd/bpalarm", body, ct);
    }

    public Task<CommandResult> SetTemperatureAlarmAsync(string deviceId, bool open, int high, int low, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/temperature/alarm", new { device_id = deviceId, open, high, low }, ct);

    public Task<CommandResult> SetBloodSugarAlarmAsync(string deviceId, bool open, double low, double high, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/sugaralarm",
            new { device_id = deviceId, open, blood_sugar_low = low, blood_sugar_high = high }, ct);

    public Task<CommandResult> SetBloodPotassiumAlarmAsync(string deviceId, bool open, double low, double high, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/potassiumalarm",
            new { device_id = deviceId, open, blood_potassium_low = low, blood_potassium_high = high }, ct);

    public Task<CommandResult> SetAutoAfAsync(AutoAfCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            open = cmd.Open,
            interval = cmd.IntervalSeconds,
            rri_single_time = cmd.RriSingleTime,
            rri_type = cmd.RriType
        };
        return PostAsync("/entservice/cmd/autoaf", body, ct);
    }

    public Task<CommandResult> SetAlarmsAsync(ClockAlarmCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            alarms = cmd.Alarms.Select(a => new
            {
                repeat = a.Repeat,
                monday = a.Monday, tuesday = a.Tuesday, wednesday = a.Wednesday,
                thursday = a.Thursday, friday = a.Friday,
                saturday = a.Saturday, sunday = a.Sunday,
                hour = a.Hour, minute = a.Minute, title = a.Title
            })
        };
        return PostAsync("/entservice2/clockalarm/set", body, ct);
    }

    public Task<CommandResult> ClearAlarmsAsync(string deviceId, CancellationToken ct = default)
        => PostAsync("/entservice2/clockalarm/clear", new { device_id = deviceId }, ct);

    public Task<CommandResult> SetSedentaryAsync(SedentaryCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            sedentaries = cmd.Sedentaries.Select(s => new
            {
                repeat = s.Repeat,
                monday = s.Monday, tuesday = s.Tuesday, wednesday = s.Wednesday,
                thursday = s.Thursday, friday = s.Friday,
                saturday = s.Saturday, sunday = s.Sunday,
                start_hour = s.StartHour, end_hour = s.EndHour,
                duration = s.Duration, threshold = s.Threshold
            })
        };
        return PostAsync("/entservice3/sedentary/set", body, ct);
    }

    public Task<CommandResult> ClearSedentaryAsync(string deviceId, CancellationToken ct = default)
        => PostAsync("/entservice3/sedentary/clear", new { device_id = deviceId }, ct);

    public Task<CommandResult> SetGoalAsync(string deviceId, int step, int distanceMetres, int calorieKcal, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/goal",
            new { device_id = deviceId, step, distance = distanceMetres, calorie = calorieKcal }, ct);

    public Task<CommandResult> FactoryResetAsync(string deviceId, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/factory/reset", new { device_id = deviceId }, ct);

    public Task<CommandResult> SetLanguageAsync(string deviceId, int languageCode, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/language/set", new { device_id = deviceId, language = languageCode }, ct);

    public Task<CommandResult> SendMessageAsync(string deviceId, string title, string description, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/message", new { device_id = deviceId, title, description }, ct);

    public Task<CommandResult> SetFallSensitivityAsync(string deviceId, int fallThreshold, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/fallcheck/sensitivity", new { device_id = deviceId, fall_threshold = fallThreshold }, ct);

    public Task<CommandResult> SetHrMeasureIntervalAsync(string deviceId, int intervalMinutes, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/measure/interval/hr", new { device_id = deviceId, interval = intervalMinutes }, ct);

    public Task<CommandResult> SetOtherMeasureIntervalAsync(string deviceId, int intervalMinutes, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/measure/interval/other", new { device_id = deviceId, interval = intervalMinutes }, ct);

    public Task<CommandResult> SetGpsLocateAsync(GpsLocateCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            gps_auto_check = cmd.GpsAutoCheck,
            gps_interval_time = cmd.GpsIntervalMinutes,
            run_gps = cmd.RunGps
        };
        return PostAsync("/entservice/cmd/gps/locate", body, ct);
    }

    public Task<CommandResult> SetTimeFormatAsync(string deviceId, bool use12Hour, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/timeformat", new { device_id = deviceId, hour_format = use12Hour ? 1 : 0 }, ct);

    public Task<CommandResult> SetDateFormatAsync(string deviceId, bool dayFirst, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/dateformat", new { device_id = deviceId, date_format = dayFirst ? 1 : 0 }, ct);

    public Task<CommandResult> SetDistanceUnitAsync(string deviceId, bool imperial, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/distanceunit", new { device_id = deviceId, distance_unit = imperial ? 1 : 0 }, ct);

    public Task<CommandResult> SetTemperatureUnitAsync(string deviceId, bool fahrenheit, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/temperatureunit", new { device_id = deviceId, temperature_unit = fahrenheit ? 1 : 0 }, ct);

    public Task<CommandResult> SetWearHandAsync(string deviceId, bool rightHand, CancellationToken ct = default)
        => PostAsync("/entservice/device/cmd/wearhand", new { device_id = deviceId, right = rightHand }, ct);

    public Task<CommandResult> SetBpMeasureScheduleAsync(string deviceId, IReadOnlyList<string> measureTimes, CancellationToken ct = default)
        => PostAsync("/entservice/cmd/bp/measure/schedule", new { device_id = deviceId, measure_time = measureTimes }, ct);

    public Task<CommandResult> SetBpCalibrationAsync(BpCalibrationCommand cmd, CancellationToken ct = default)
    {
        var body = new
        {
            device_id = cmd.DeviceId,
            sbp_band = cmd.SbpBand, dbp_band = cmd.DbpBand,
            sbp_meter = cmd.SbpMeter, dbp_meter = cmd.DbpMeter
        };
        return PostAsync("/entservice/cmd/bpadjust", body, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<CommandResult> PostAsync(string path, object body, CancellationToken ct)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(body)
            };
            AddAuthHeaders(request);

            var resp = await _http.SendAsync(request, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var result = await resp.Content
                .ReadFromJsonAsync<EntServiceResponse<object>>(cancellationToken: ct)
                .ConfigureAwait(false);

            int code = result?.ReturnCode ?? -1;
            bool success = code == 0;
            if (!success)
            {
                _logger.LogWarning("EntService {Path} returned code {Code}", path, code);
            }

            return new CommandResult(code, success);
        }
        catch (Exception ex)
        {
            _logger.LogError("EntService call to {Path} failed: {Msg}", path, ex.Message);
            return new CommandResult(-1, false);
        }
    }

    /// <summary>
    /// Adds account + MD5(password) authentication headers required by the entservice.
    /// </summary>
    private void AddAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("account", _options.Account);
        request.Headers.TryAddWithoutValidation("pwd", Md5Hex(_options.Password));
    }

    private static string Md5Hex(string input)
    {
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    // ── Private response DTOs ─────────────────────────────────────────────────

    private sealed class EntServiceResponse<T>
    {
        [JsonPropertyName("ReturnCode")]
        public int ReturnCode { get; init; }

        [JsonPropertyName("Data")]
        public T? Data { get; init; }
    }

    private sealed class DeviceStatusData
    {
        [JsonPropertyName("status")]
        public int Status { get; init; }
    }
}

/// <summary>
/// Configuration for the iwown entservice (bind from appsettings.json).
/// </summary>
public sealed class WownCommandOptions
{
    public const string SectionName = "WownCommand";

    /// <summary>
    /// Base URL — https://search.iwown.com (China) or https://euapi.iwown.com (rest of world).
    /// </summary>
    public string BaseUrl { get; set; } = "https://search.iwown.com";

    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
