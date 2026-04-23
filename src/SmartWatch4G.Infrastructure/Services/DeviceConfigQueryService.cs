using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class DeviceConfigQueryService : IDeviceConfigQueryService
{
    private const string UnexpectedError = "An unexpected error occurred.";

    private readonly IDatabaseService _db;
    private readonly ILogger<DeviceConfigQueryService> _logger;

    public DeviceConfigQueryService(IDatabaseService db, ILogger<DeviceConfigQueryService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public Task<ServiceResult<DeviceConfigPagedResult>> GetByCompanyAsync(int companyId, int page, int pageSize)
    {
        try
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var skip  = (page - 1) * pageSize;

            var total = _db.GetDeviceConfigCountByCompany(companyId);
            var rows  = _db.GetDeviceConfigsByCompany(companyId, skip, pageSize);

            var result = new DeviceConfigPagedResult
            {
                Items      = rows.Select(MapConfig).ToList(),
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
            return Task.FromResult(ServiceResult<DeviceConfigPagedResult>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeviceConfigQueryService.GetByCompanyAsync failed for company {Id}", companyId);
            return Task.FromResult(ServiceResult<DeviceConfigPagedResult>.Fail(UnexpectedError, 500));
        }
    }

    public Task<ServiceResult<DeviceConfigResponse>> GetByDeviceAsync(string deviceId)
    {
        try
        {
            var row = _db.GetDeviceConfig(deviceId);
            if (row is null)
                return Task.FromResult(
                    ServiceResult<DeviceConfigResponse>.Fail("No configuration found for this device.", 404));

            return Task.FromResult(ServiceResult<DeviceConfigResponse>.Ok(MapConfig(row.Value)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeviceConfigQueryService.GetByDeviceAsync failed for {Device}", deviceId);
            return Task.FromResult(ServiceResult<DeviceConfigResponse>.Fail(UnexpectedError, 500));
        }
    }

    // ── mapping ───────────────────────────────────────────────────────────────

    private static DeviceConfigResponse MapConfig((
        string DeviceId, string? UserName, System.DateTime? UpdatedAt,
        bool? GpsAutoCheck, int? GpsIntervalTime, int? PowerMode,
        bool? DataAutoUpload, int? DataUploadInterval, bool? AutoLocate, int? LocateIntervalTime,
        bool? HrAlarmOpen, int? HrAlarmHigh, int? HrAlarmLow, int? HrAlarmThreshold, int? HrAlarmInterval,
        bool? DynHrAlarmOpen, int? DynHrAlarmHigh, int? DynHrAlarmLow, int? DynHrAlarmTimeout, int? DynHrAlarmInterval,
        bool? Spo2AlarmOpen, int? Spo2AlarmLow,
        bool? BpAlarmOpen, int? BpSbpHigh, int? BpSbpBelow, int? BpDbpHigh, int? BpDbpBelow,
        bool? TempAlarmOpen, double? TempAlarmHigh, double? TempAlarmLow,
        bool? FallCheckEnabled, int? FallThreshold,
        string? Language, int? HourFormat, string? DateFormat, int? DistanceUnit, int? TemperatureUnit, bool? WearHandRight,
        int? HrInterval, int? OtherInterval,
        int? GoalStep, double? GoalDistance, double? GoalCalorie,
        bool? GpsLocateAutoCheck, int? GpsLocateIntervalTime, bool? RunGps,
        bool? LcdGestureOpen, int? LcdGestureStartHour, int? LcdGestureEndHour,
        bool? AutoAfOpen, int? AutoAfInterval,
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter) r) =>
        new()
        {
            DeviceId               = r.DeviceId,
            UserName               = r.UserName,
            GpsAutoCheck           = r.GpsAutoCheck,
            GpsIntervalTime        = r.GpsIntervalTime,
            PowerMode              = r.PowerMode,
            DataAutoUpload         = r.DataAutoUpload,
            DataUploadInterval     = r.DataUploadInterval,
            AutoLocate             = r.AutoLocate,
            LocateIntervalTime     = r.LocateIntervalTime,
            HrAlarmOpen            = r.HrAlarmOpen,
            HrAlarmHigh            = r.HrAlarmHigh,
            HrAlarmLow             = r.HrAlarmLow,
            HrAlarmThreshold       = r.HrAlarmThreshold,
            HrAlarmInterval        = r.HrAlarmInterval,
            DynHrAlarmOpen         = r.DynHrAlarmOpen,
            DynHrAlarmHigh         = r.DynHrAlarmHigh,
            DynHrAlarmLow          = r.DynHrAlarmLow,
            DynHrAlarmTimeout      = r.DynHrAlarmTimeout,
            DynHrAlarmInterval     = r.DynHrAlarmInterval,
            Spo2AlarmOpen          = r.Spo2AlarmOpen,
            Spo2AlarmLow           = r.Spo2AlarmLow,
            BpAlarmOpen            = r.BpAlarmOpen,
            BpSbpHigh              = r.BpSbpHigh,
            BpSbpBelow             = r.BpSbpBelow,
            BpDbpHigh              = r.BpDbpHigh,
            BpDbpBelow             = r.BpDbpBelow,
            TempAlarmOpen          = r.TempAlarmOpen,
            TempAlarmHigh          = r.TempAlarmHigh,
            TempAlarmLow           = r.TempAlarmLow,
            FallCheckEnabled       = r.FallCheckEnabled,
            FallThreshold          = r.FallThreshold,
            Language               = r.Language,
            HourFormat             = r.HourFormat,
            DateFormat             = r.DateFormat,
            DistanceUnit           = r.DistanceUnit,
            TemperatureUnit        = r.TemperatureUnit,
            WearHandRight          = r.WearHandRight,
            HrInterval             = r.HrInterval,
            OtherInterval          = r.OtherInterval,
            GoalStep               = r.GoalStep,
            GoalDistance           = r.GoalDistance,
            GoalCalorie            = r.GoalCalorie,
            GpsLocateAutoCheck     = r.GpsLocateAutoCheck,
            GpsLocateIntervalTime  = r.GpsLocateIntervalTime,
            RunGps                 = r.RunGps,
            LcdGestureOpen         = r.LcdGestureOpen,
            LcdGestureStartHour    = r.LcdGestureStartHour,
            LcdGestureEndHour      = r.LcdGestureEndHour,
            AutoAfOpen             = r.AutoAfOpen,
            AutoAfInterval         = r.AutoAfInterval,
            BpSbpBand              = r.BpSbpBand,
            BpDbpBand              = r.BpDbpBand,
            BpSbpMeter             = r.BpSbpMeter,
            BpDbpMeter             = r.BpDbpMeter,
            LastUpdatedAt          = r.UpdatedAt
        };
}
