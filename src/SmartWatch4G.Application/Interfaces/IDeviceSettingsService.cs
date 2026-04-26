using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

public interface IDeviceSettingsService
{
    Task SaveUserInfo(UserInfoRequest r);
    Task SaveFallCheck(FallCheckRequest r);
    Task SaveFallCheckSensitivity(FallCheckSensitivityRequest r);
    Task SaveDataFreq(DataFreqRequest r);
    Task SaveLocateDataUploadFreq(LocateDataUploadFreqRequest r);
    Task SaveLcdGesture(LcdGestureRequest r);
    Task SaveHrAlarm(HrAlarmRequest r);
    Task SaveDynamicHrAlarm(DynamicHrAlarmRequest r);
    Task SaveSpo2Alarm(Spo2AlarmRequest r);
    Task SaveBpAlarm(BpAlarmRequest r);
    Task SaveTemperatureAlarm(TemperatureAlarmRequest r);
    Task SaveAutoAf(AutoAfRequest r);
    Task SaveGoal(GoalRequest r);
    Task SaveLanguage(LanguageRequest r);
    Task SaveTimeFormat(TimeFormatRequest r);
    Task SaveDateFormat(DateFormatRequest r);
    Task SaveDistanceUnit(DistanceUnitRequest r);
    Task SaveTemperatureUnit(TemperatureUnitRequest r);
    Task SaveWearHand(WearHandRequest r);
    Task SaveBpAdjust(BpAdjustRequest r);
    Task SaveHrInterval(HrIntervalRequest r);
    Task SaveOtherInterval(OtherIntervalRequest r);
    Task SaveGpsLocate(GpsLocateRequest r);
    Task SavePhonebook(PhonebookSyncRequest r);
    Task ClearPhonebook(string deviceId);
    Task SaveClockAlarms(SetAlarmRequest r);
    Task ClearClockAlarms(string deviceId);
    Task SaveSedentary(SetSedentaryRequest r);
    Task ClearSedentary(string deviceId);
}
