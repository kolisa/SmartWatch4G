using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Infrastructure.Services;

public class IwownService
{
    private readonly HttpClient _http;
    private readonly ILogger<IwownService> _logger;

    public IwownService(HttpClient http, ILogger<IwownService> logger)
    {
        _http = http;
        _logger = logger;
    }

    private async Task<IwownResponse?> PostAsync<T>(string path, T request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(path, request);
            return await response.Content.ReadFromJsonAsync<IwownResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling {Path}", path);
            return null;
        }
    }

    public Task<IwownResponse?> SetUserInfoAsync(UserInfoRequest req) =>
        PostAsync("/entservice/cmd/userinfo", req);

    public Task<IwownResponse?> EnableRealtimeLocationAsync(DeviceIdRequest req) =>
        PostAsync("/entservice/cmd/realtime/location", req);

    public Task<IwownResponse?> RequestDataSyncAsync(DeviceIdRequest req) =>
        PostAsync("/entservice/cmd/datasync", req);

    public async Task<IwownResponse?> GetDeviceStatusAsync(string deviceId)
    {
        try
        {
            return await _http.GetFromJsonAsync<IwownResponse>(
                $"/entservice/device/status?device_id={Uri.EscapeDataString(deviceId)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device status for {DeviceId}", deviceId);
            return null;
        }
    }

    public Task<IwownResponse?> SetFallCheckAsync(FallCheckRequest req) =>
        PostAsync("/entservice/cmd/fallcheck", req);

    public Task<IwownResponse?> SyncPhonebookAsync(PhonebookSyncRequest req) =>
        PostAsync("/entservice/phonebook/sync", req);

    public Task<IwownResponse?> ClearPhonebookAsync(DeviceIdRequest req) =>
        PostAsync("/entservice/phonebook/clear", req);

    public Task<IwownResponse?> SetDataFreqAsync(DataFreqRequest req) =>
        PostAsync("/entservice/cmd/datafreq", req);

    public Task<IwownResponse?> SetLocateDataUploadFreqAsync(LocateDataUploadFreqRequest req) =>
        PostAsync("/entservice/cmd/locate_dataupload/freq", req);

    public Task<IwownResponse?> SetLcdGestureAsync(LcdGestureRequest req) =>
        PostAsync("/entservice/cmd/lcdgesture", req);

    public Task<IwownResponse?> SetHrAlarmAsync(HrAlarmRequest req) =>
        PostAsync("/entservice/cmd/hralarm", req);

    public Task<IwownResponse?> SetDynamicHrAlarmAsync(DynamicHrAlarmRequest req) =>
        PostAsync("/entservice/cmd/dynamic/hralarm", req);

    public Task<IwownResponse?> SetSpo2AlarmAsync(Spo2AlarmRequest req) =>
        PostAsync("/entservice/cmd/spo2alarm", req);

    public Task<IwownResponse?> SetBpAlarmAsync(BpAlarmRequest req) =>
        PostAsync("/entservice/cmd/bpalarm", req);

    public Task<IwownResponse?> SetTemperatureAlarmAsync(TemperatureAlarmRequest req) =>
        PostAsync("/entservice/cmd/temperature/alarm", req);

    public Task<IwownResponse?> SetAutoAfAsync(AutoAfRequest req) =>
        PostAsync("/entservice/cmd/autoaf", req);

    public Task<IwownResponse?> SetAlarmAsync(SetAlarmRequest req) =>
        PostAsync("/entservice2/clockalarm/set", req);

    public Task<IwownResponse?> ClearAlarmAsync(DeviceIdRequest req) =>
        PostAsync("/entservice2/clockalarm/clear", req);

    public Task<IwownResponse?> SetSedentaryAsync(SetSedentaryRequest req) =>
        PostAsync("/entservice3/sedentary/set", req);

    public Task<IwownResponse?> ClearSedentaryAsync(DeviceIdRequest req) =>
        PostAsync("/entservice3/sedentary/clear", req);

    public Task<IwownResponse?> SetGoalAsync(GoalRequest req) =>
        PostAsync("/entservice/cmd/goal", req);

    public Task<IwownResponse?> FactoryResetAsync(DeviceIdRequest req) =>
        PostAsync("/entservice/cmd/factory/reset", req);

    public Task<IwownResponse?> SetLanguageAsync(LanguageRequest req) =>
        PostAsync("/entservice/cmd/language/set", req);

    public Task<IwownResponse?> SendMessageAsync(MessageRequest req) =>
        PostAsync("/entservice/cmd/message", req);

    public Task<IwownResponse?> SetFallCheckSensitivityAsync(FallCheckSensitivityRequest req) =>
        PostAsync("/entservice/cmd/fallcheck/sensitivity", req);

    public Task<IwownResponse?> SetHrIntervalAsync(HrIntervalRequest req) =>
        PostAsync("/entservice/cmd/measure/interval/hr", req);

    public Task<IwownResponse?> SetOtherIntervalAsync(OtherIntervalRequest req) =>
        PostAsync("/entservice/cmd/measure/interval/other", req);

    public Task<IwownResponse?> GpsLocateAsync(GpsLocateRequest req) =>
        PostAsync("/entservice/cmd/gps/locate", req);

    public Task<IwownResponse?> SetTimeFormatAsync(TimeFormatRequest req) =>
        PostAsync("/entservice/cmd/timeformat", req);

    public Task<IwownResponse?> SetDateFormatAsync(DateFormatRequest req) =>
        PostAsync("/entservice/cmd/dateformat", req);

    public Task<IwownResponse?> SetDistanceUnitAsync(DistanceUnitRequest req) =>
        PostAsync("/entservice/cmd/distanceunit", req);

    public Task<IwownResponse?> SetTemperatureUnitAsync(TemperatureUnitRequest req) =>
        PostAsync("/entservice/cmd/temperatureunit", req);

    public Task<IwownResponse?> SetWearHandAsync(WearHandRequest req) =>
        PostAsync("/entservice/device/cmd/wearhand", req);

    public Task<IwownResponse?> SetBpAdjustAsync(BpAdjustRequest req) =>
        PostAsync("/entservice/cmd/bpadjust", req);
}
