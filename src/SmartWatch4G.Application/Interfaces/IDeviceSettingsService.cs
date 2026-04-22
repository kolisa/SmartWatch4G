using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

public interface IDeviceSettingsService
{
    void SaveUserInfo(UserInfoRequest r);
    void SaveFallCheck(FallCheckRequest r);
    void SaveFallCheckSensitivity(FallCheckSensitivityRequest r);
    void SaveDataFreq(DataFreqRequest r);
    void SaveLocateDataUploadFreq(LocateDataUploadFreqRequest r);
    void SaveLcdGesture(LcdGestureRequest r);
    void SaveHrAlarm(HrAlarmRequest r);
    void SaveDynamicHrAlarm(DynamicHrAlarmRequest r);
    void SaveSpo2Alarm(Spo2AlarmRequest r);
    void SaveBpAlarm(BpAlarmRequest r);
    void SaveTemperatureAlarm(TemperatureAlarmRequest r);
    void SaveAutoAf(AutoAfRequest r);
    void SaveGoal(GoalRequest r);
    void SaveLanguage(LanguageRequest r);
    void SaveTimeFormat(TimeFormatRequest r);
    void SaveDateFormat(DateFormatRequest r);
    void SaveDistanceUnit(DistanceUnitRequest r);
    void SaveTemperatureUnit(TemperatureUnitRequest r);
    void SaveWearHand(WearHandRequest r);
    void SaveBpAdjust(BpAdjustRequest r);
    void SaveHrInterval(HrIntervalRequest r);
    void SaveOtherInterval(OtherIntervalRequest r);
    void SaveGpsLocate(GpsLocateRequest r);
    void SavePhonebook(PhonebookSyncRequest r);
    void ClearPhonebook(string deviceId);
    void SaveClockAlarms(SetAlarmRequest r);
    void ClearClockAlarms(string deviceId);
    void SaveSedentary(SetSedentaryRequest r);
    void ClearSedentary(string deviceId);
}
