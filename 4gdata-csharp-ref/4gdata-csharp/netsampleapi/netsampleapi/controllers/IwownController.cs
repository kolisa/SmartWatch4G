using Microsoft.AspNetCore.Mvc;
using SampleApi.Iwown;
using SampleApi.Services;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("iwown")]
    public class IwownController : ControllerBase
    {
        private readonly IwownService _iwown;

        public IwownController(IwownService iwown)
        {
            _iwown = iwown;
        }

        [HttpPost("cmd/userinfo")]
        public async Task<IActionResult> SetUserInfo([FromBody] UserInfoRequest req) =>
            Ok(await _iwown.SetUserInfoAsync(req));

        [HttpPost("cmd/realtime/location")]
        public async Task<IActionResult> EnableRealtimeLocation([FromBody] DeviceIdRequest req) =>
            Ok(await _iwown.EnableRealtimeLocationAsync(req));

        [HttpPost("cmd/datasync")]
        public async Task<IActionResult> RequestDataSync([FromBody] DeviceIdRequest req) =>
            Ok(await _iwown.RequestDataSyncAsync(req));

        [HttpGet("device/status")]
        public async Task<IActionResult> GetDeviceStatus([FromQuery] string device_id) =>
            Ok(await _iwown.GetDeviceStatusAsync(device_id));

        [HttpPost("cmd/fallcheck")]
        public async Task<IActionResult> SetFallCheck([FromBody] FallCheckRequest req) =>
            Ok(await _iwown.SetFallCheckAsync(req));

        [HttpPost("phonebook/sync")]
        public async Task<IActionResult> SyncPhonebook([FromBody] PhonebookSyncRequest req) =>
            Ok(await _iwown.SyncPhonebookAsync(req));

        [HttpPost("phonebook/clear")]
        public async Task<IActionResult> ClearPhonebook([FromBody] DeviceIdRequest req) =>
            Ok(await _iwown.ClearPhonebookAsync(req));

        [HttpPost("cmd/datafreq")]
        public async Task<IActionResult> SetDataFreq([FromBody] DataFreqRequest req) =>
            Ok(await _iwown.SetDataFreqAsync(req));

        [HttpPost("cmd/locate_dataupload/freq")]
        public async Task<IActionResult> SetLocateDataUploadFreq([FromBody] LocateDataUploadFreqRequest req) =>
            Ok(await _iwown.SetLocateDataUploadFreqAsync(req));

        [HttpPost("cmd/lcdgesture")]
        public async Task<IActionResult> SetLcdGesture([FromBody] LcdGestureRequest req) =>
            Ok(await _iwown.SetLcdGestureAsync(req));

        [HttpPost("cmd/hralarm")]
        public async Task<IActionResult> SetHrAlarm([FromBody] HrAlarmRequest req) =>
            Ok(await _iwown.SetHrAlarmAsync(req));

        [HttpPost("cmd/dynamic/hralarm")]
        public async Task<IActionResult> SetDynamicHrAlarm([FromBody] DynamicHrAlarmRequest req) =>
            Ok(await _iwown.SetDynamicHrAlarmAsync(req));

        [HttpPost("cmd/spo2alarm")]
        public async Task<IActionResult> SetSpo2Alarm([FromBody] Spo2AlarmRequest req) =>
            Ok(await _iwown.SetSpo2AlarmAsync(req));

        [HttpPost("cmd/bpalarm")]
        public async Task<IActionResult> SetBpAlarm([FromBody] BpAlarmRequest req) =>
            Ok(await _iwown.SetBpAlarmAsync(req));

        [HttpPost("cmd/temperature/alarm")]
        public async Task<IActionResult> SetTemperatureAlarm([FromBody] TemperatureAlarmRequest req) =>
            Ok(await _iwown.SetTemperatureAlarmAsync(req));

        [HttpPost("cmd/autoaf")]
        public async Task<IActionResult> SetAutoAf([FromBody] AutoAfRequest req) =>
            Ok(await _iwown.SetAutoAfAsync(req));

        [HttpPost("clockalarm/set")]
        public async Task<IActionResult> SetAlarm([FromBody] SetAlarmRequest req) =>
            Ok(await _iwown.SetAlarmAsync(req));

        [HttpPost("clockalarm/clear")]
        public async Task<IActionResult> ClearAlarm([FromBody] DeviceIdRequest req) =>
            Ok(await _iwown.ClearAlarmAsync(req));

        [HttpPost("sedentary/set")]
        public async Task<IActionResult> SetSedentary([FromBody] SetSedentaryRequest req) =>
            Ok(await _iwown.SetSedentaryAsync(req));

        [HttpPost("sedentary/clear")]
        public async Task<IActionResult> ClearSedentary([FromBody] DeviceIdRequest req) =>
            Ok(await _iwown.ClearSedentaryAsync(req));

        [HttpPost("cmd/goal")]
        public async Task<IActionResult> SetGoal([FromBody] GoalRequest req) =>
            Ok(await _iwown.SetGoalAsync(req));

        [HttpPost("cmd/factory/reset")]
        public async Task<IActionResult> FactoryReset([FromBody] DeviceIdRequest req) =>
            Ok(await _iwown.FactoryResetAsync(req));

        [HttpPost("cmd/language/set")]
        public async Task<IActionResult> SetLanguage([FromBody] LanguageRequest req) =>
            Ok(await _iwown.SetLanguageAsync(req));

        [HttpPost("cmd/message")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest req) =>
            Ok(await _iwown.SendMessageAsync(req));

        [HttpPost("cmd/fallcheck/sensitivity")]
        public async Task<IActionResult> SetFallCheckSensitivity([FromBody] FallCheckSensitivityRequest req) =>
            Ok(await _iwown.SetFallCheckSensitivityAsync(req));

        [HttpPost("cmd/measure/interval/hr")]
        public async Task<IActionResult> SetHrInterval([FromBody] HrIntervalRequest req) =>
            Ok(await _iwown.SetHrIntervalAsync(req));

        [HttpPost("cmd/measure/interval/other")]
        public async Task<IActionResult> SetOtherInterval([FromBody] OtherIntervalRequest req) =>
            Ok(await _iwown.SetOtherIntervalAsync(req));

        [HttpPost("cmd/gps/locate")]
        public async Task<IActionResult> GpsLocate([FromBody] GpsLocateRequest req) =>
            Ok(await _iwown.GpsLocateAsync(req));

        [HttpPost("cmd/timeformat")]
        public async Task<IActionResult> SetTimeFormat([FromBody] TimeFormatRequest req) =>
            Ok(await _iwown.SetTimeFormatAsync(req));

        [HttpPost("cmd/dateformat")]
        public async Task<IActionResult> SetDateFormat([FromBody] DateFormatRequest req) =>
            Ok(await _iwown.SetDateFormatAsync(req));

        [HttpPost("cmd/distanceunit")]
        public async Task<IActionResult> SetDistanceUnit([FromBody] DistanceUnitRequest req) =>
            Ok(await _iwown.SetDistanceUnitAsync(req));

        [HttpPost("cmd/temperatureunit")]
        public async Task<IActionResult> SetTemperatureUnit([FromBody] TemperatureUnitRequest req) =>
            Ok(await _iwown.SetTemperatureUnitAsync(req));

        [HttpPost("device/cmd/wearhand")]
        public async Task<IActionResult> SetWearHand([FromBody] WearHandRequest req) =>
            Ok(await _iwown.SetWearHandAsync(req));

        [HttpPost("cmd/bpadjust")]
        public async Task<IActionResult> SetBpAdjust([FromBody] BpAdjustRequest req) =>
            Ok(await _iwown.SetBpAdjustAsync(req));
    }
}
