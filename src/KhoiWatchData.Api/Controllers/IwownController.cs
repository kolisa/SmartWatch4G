using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Infrastructure.Services;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("iwown")]
public class IwownController : ControllerBase
{
    private readonly IwownService _iwown;
    private readonly IDeviceSettingsService _settings;

    public IwownController(IwownService iwown, IDeviceSettingsService settings)
    {
        _iwown    = iwown;
        _settings = settings;
    }

    [HttpPost("cmd/userinfo")]
    public async Task<IActionResult> SetUserInfo([FromBody] UserInfoRequest req)
    {
        var result = await _iwown.SetUserInfoAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveUserInfo(req);
        return Ok(result);
    }

    [HttpPost("cmd/realtime/location")]
    public async Task<IActionResult> EnableRealtimeLocation([FromBody] DeviceIdRequest req) =>
        Ok(await _iwown.EnableRealtimeLocationAsync(req));

    [HttpPost("cmd/datasync")]
    public async Task<IActionResult> RequestDataSync([FromBody] DeviceIdRequest req) =>
        Ok(await _iwown.RequestDataSyncAsync(req));

    [HttpGet("device/status")]
    public async Task<IActionResult> GetDeviceStatus([FromQuery] string device_id)
    {
        var result = await _iwown.GetDeviceStatusAsync(device_id);
        if (result is null) return StatusCode(502);
        return Ok(new
        {
            result.ReturnCode,
            result.Data,
            status = DeviceStatusParser.Parse(result)
        });
    }

    [HttpPost("cmd/fallcheck")]
    public async Task<IActionResult> SetFallCheck([FromBody] FallCheckRequest req)
    {
        var result = await _iwown.SetFallCheckAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveFallCheck(req);
        return Ok(result);
    }

    [HttpPost("phonebook/sync")]
    public async Task<IActionResult> SyncPhonebook([FromBody] PhonebookSyncRequest req)
    {
        var result = await _iwown.SyncPhonebookAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SavePhonebook(req);
        return Ok(result);
    }

    [HttpPost("phonebook/clear")]
    public async Task<IActionResult> ClearPhonebook([FromBody] DeviceIdRequest req)
    {
        var result = await _iwown.ClearPhonebookAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.ClearPhonebook(req.device_id);
        return Ok(result);
    }

    [HttpPost("cmd/datafreq")]
    public async Task<IActionResult> SetDataFreq([FromBody] DataFreqRequest req)
    {
        var result = await _iwown.SetDataFreqAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveDataFreq(req);
        return Ok(result);
    }

    [HttpPost("cmd/locate_dataupload/freq")]
    public async Task<IActionResult> SetLocateDataUploadFreq([FromBody] LocateDataUploadFreqRequest req)
    {
        var result = await _iwown.SetLocateDataUploadFreqAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveLocateDataUploadFreq(req);
        return Ok(result);
    }

    [HttpPost("cmd/lcdgesture")]
    public async Task<IActionResult> SetLcdGesture([FromBody] LcdGestureRequest req)
    {
        var result = await _iwown.SetLcdGestureAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveLcdGesture(req);
        return Ok(result);
    }

    [HttpPost("cmd/hralarm")]
    public async Task<IActionResult> SetHrAlarm([FromBody] HrAlarmRequest req)
    {
        var result = await _iwown.SetHrAlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveHrAlarm(req);
        return Ok(result);
    }

    [HttpPost("cmd/dynamic/hralarm")]
    public async Task<IActionResult> SetDynamicHrAlarm([FromBody] DynamicHrAlarmRequest req)
    {
        var result = await _iwown.SetDynamicHrAlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveDynamicHrAlarm(req);
        return Ok(result);
    }

    [HttpPost("cmd/spo2alarm")]
    public async Task<IActionResult> SetSpo2Alarm([FromBody] Spo2AlarmRequest req)
    {
        var result = await _iwown.SetSpo2AlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveSpo2Alarm(req);
        return Ok(result);
    }

    [HttpPost("cmd/bpalarm")]
    public async Task<IActionResult> SetBpAlarm([FromBody] BpAlarmRequest req)
    {
        var result = await _iwown.SetBpAlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveBpAlarm(req);
        return Ok(result);
    }

    [HttpPost("cmd/temperature/alarm")]
    public async Task<IActionResult> SetTemperatureAlarm([FromBody] TemperatureAlarmRequest req)
    {
        var result = await _iwown.SetTemperatureAlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveTemperatureAlarm(req);
        return Ok(result);
    }

    [HttpPost("cmd/autoaf")]
    public async Task<IActionResult> SetAutoAf([FromBody] AutoAfRequest req)
    {
        var result = await _iwown.SetAutoAfAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveAutoAf(req);
        return Ok(result);
    }

    [HttpPost("clockalarm/set")]
    public async Task<IActionResult> SetAlarm([FromBody] SetAlarmRequest req)
    {
        var result = await _iwown.SetAlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveClockAlarms(req);
        return Ok(result);
    }

    [HttpPost("clockalarm/clear")]
    public async Task<IActionResult> ClearAlarm([FromBody] DeviceIdRequest req)
    {
        var result = await _iwown.ClearAlarmAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.ClearClockAlarms(req.device_id);
        return Ok(result);
    }

    [HttpPost("sedentary/set")]
    public async Task<IActionResult> SetSedentary([FromBody] SetSedentaryRequest req)
    {
        var result = await _iwown.SetSedentaryAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveSedentary(req);
        return Ok(result);
    }

    [HttpPost("sedentary/clear")]
    public async Task<IActionResult> ClearSedentary([FromBody] DeviceIdRequest req)
    {
        var result = await _iwown.ClearSedentaryAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.ClearSedentary(req.device_id);
        return Ok(result);
    }

    [HttpPost("cmd/goal")]
    public async Task<IActionResult> SetGoal([FromBody] GoalRequest req)
    {
        var result = await _iwown.SetGoalAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveGoal(req);
        return Ok(result);
    }

    [HttpPost("cmd/factory/reset")]
    public async Task<IActionResult> FactoryReset([FromBody] DeviceIdRequest req) =>
        Ok(await _iwown.FactoryResetAsync(req));

    [HttpPost("cmd/language/set")]
    public async Task<IActionResult> SetLanguage([FromBody] LanguageRequest req)
    {
        var result = await _iwown.SetLanguageAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveLanguage(req);
        return Ok(result);
    }

    [HttpPost("cmd/message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageRequest req) =>
        Ok(await _iwown.SendMessageAsync(req));

    [HttpPost("cmd/fallcheck/sensitivity")]
    public async Task<IActionResult> SetFallCheckSensitivity([FromBody] FallCheckSensitivityRequest req)
    {
        var result = await _iwown.SetFallCheckSensitivityAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveFallCheckSensitivity(req);
        return Ok(result);
    }

    [HttpPost("cmd/measure/interval/hr")]
    public async Task<IActionResult> SetHrInterval([FromBody] HrIntervalRequest req)
    {
        var result = await _iwown.SetHrIntervalAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveHrInterval(req);
        return Ok(result);
    }

    [HttpPost("cmd/measure/interval/other")]
    public async Task<IActionResult> SetOtherInterval([FromBody] OtherIntervalRequest req)
    {
        var result = await _iwown.SetOtherIntervalAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveOtherInterval(req);
        return Ok(result);
    }

    [HttpPost("cmd/gps/locate")]
    public async Task<IActionResult> GpsLocate([FromBody] GpsLocateRequest req)
    {
        var result = await _iwown.GpsLocateAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveGpsLocate(req);
        return Ok(result);
    }

    [HttpPost("cmd/timeformat")]
    public async Task<IActionResult> SetTimeFormat([FromBody] TimeFormatRequest req)
    {
        var result = await _iwown.SetTimeFormatAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveTimeFormat(req);
        return Ok(result);
    }

    [HttpPost("cmd/dateformat")]
    public async Task<IActionResult> SetDateFormat([FromBody] DateFormatRequest req)
    {
        var result = await _iwown.SetDateFormatAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveDateFormat(req);
        return Ok(result);
    }

    [HttpPost("cmd/distanceunit")]
    public async Task<IActionResult> SetDistanceUnit([FromBody] DistanceUnitRequest req)
    {
        var result = await _iwown.SetDistanceUnitAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveDistanceUnit(req);
        return Ok(result);
    }

    [HttpPost("cmd/temperatureunit")]
    public async Task<IActionResult> SetTemperatureUnit([FromBody] TemperatureUnitRequest req)
    {
        var result = await _iwown.SetTemperatureUnitAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveTemperatureUnit(req);
        return Ok(result);
    }

    [HttpPost("device/cmd/wearhand")]
    public async Task<IActionResult> SetWearHand([FromBody] WearHandRequest req)
    {
        var result = await _iwown.SetWearHandAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveWearHand(req);
        return Ok(result);
    }

    [HttpPost("cmd/bpadjust")]
    public async Task<IActionResult> SetBpAdjust([FromBody] BpAdjustRequest req)
    {
        var result = await _iwown.SetBpAdjustAsync(req);
        if (result is null) return StatusCode(502);
        if (result.Succeeded) await _settings.SaveBpAdjust(req);
        return Ok(result);
    }
}
