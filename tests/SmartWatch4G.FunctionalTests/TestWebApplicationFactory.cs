using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.FunctionalTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory() => _ = Server;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace DatabaseService with a no-op stub so tests don't need SQL Server.
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDatabaseService));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddSingleton<IDatabaseService, NoOpDatabaseService>();

            // Remove all background hosted services (Quartz scheduler, etc.) in the
            // test environment so that LogFileMonitorWorker/FileSystemWatcher and
            // DeviceStatusPollingJob are never instantiated — preventing
            // ObjectDisposedException crashes on test teardown.
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var svc in hostedServices) services.Remove(svc);
        });
    }

    private sealed class NoOpDatabaseService : IDatabaseService
    {
        public Task InsertGpsTrack(string d, string t, double lon, double lat, string loc) => Task.CompletedTask;
        public Task UpsertHealthSnapshot(string d, string rt,
            int? bat=null, int? rssi=null, int? stp=null, double? dist=null, double? cal=null,
            int? ahr=null, int? xhr=null, int? nhr=null, int? spo=null, int? sbp=null, int? dbp=null, int? fat=null) => Task.CompletedTask;
        public Task InsertAlarm(string d, string t, string type, string? details=null) => Task.CompletedTask;
        public Task InsertSosEvent(string d, string t, double? lat, double? lon,
            string? num, int? status, string? start, string? end) => Task.CompletedTask;
        public Task InsertDeviceInfo(string d, string rat,
            string? model, string? ver, string? wear, string? sig, string raw) => Task.CompletedTask;
        public Task InsertSleepCalculation(string d, string rd, int comp,
            string? st, string? et, int hr, int tt,
            double? ra, double? rx, double? rn, string? sec) => Task.CompletedTask;
        public Task InsertEcgCalculation(string d, int result, int hr, int eff, int dir) => Task.CompletedTask;
        public Task InsertAfCalculation(string d, int result) => Task.CompletedTask;
        public Task InsertSpo2Calculation(string d, double score, int? risk) => Task.CompletedTask;

        // User profile methods
        public Task UpsertUserProfile(string deviceId, string name, string surname,
            string? email=null, string? cell=null, string? empNo=null, string? address=null,
            int? companyId=null) => Task.CompletedTask;
        public Task<SmartWatch4G.Domain.Entities.UserProfile?> GetUserProfile(string deviceId) => Task.FromResult<SmartWatch4G.Domain.Entities.UserProfile?>(null);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>> GetAllUserProfiles() => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>>([]);
        public Task DeleteUserProfile(string deviceId) => Task.CompletedTask;

        // Company methods
        public Task<int> CreateCompany(string name, string? reg, string? email, string? phone, string? addr) => Task.FromResult(-1);
        public Task<SmartWatch4G.Domain.Entities.Company?> GetCompany(int id) => Task.FromResult<SmartWatch4G.Domain.Entities.Company?>(null);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.Company>> GetAllCompanies() => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.Company>>([]);
        public Task UpdateCompany(int id, string name, string? reg, string? email, string? phone, string? addr) => Task.CompletedTask;
        public Task DeleteCompany(int id) => Task.CompletedTask;
        public Task LinkUserToCompany(string deviceId, int? companyId) => Task.CompletedTask;
        public Task<int> BackfillDeviceRecords(string deviceId) => Task.FromResult(0);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>> GetUsersByCompanyId(int companyId) => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>>([]);
        public Task ReactivateUserProfile(string deviceId) => Task.CompletedTask;

        // GNSS query methods
        public Task<SmartWatch4G.Domain.Entities.GnssTrack?> GetLatestGnssTrack(string deviceId) => Task.FromResult<SmartWatch4G.Domain.Entities.GnssTrack?>(null);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.GnssTrack>> GetGnssTracks(
            string deviceId, System.DateTime? from, System.DateTime? to) => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.GnssTrack>>([]);

        // Health / stats query methods
        public Task<SmartWatch4G.Domain.Entities.HealthSnapshot?> GetLatestHealthSnapshot(string deviceId) => Task.FromResult<SmartWatch4G.Domain.Entities.HealthSnapshot?>(null);
        public Task<int> GetActiveWorkerCount() => Task.FromResult(0);
        public Task<int> GetActiveWorkerCountByCompany(int companyId) => Task.FromResult(0);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>> GetPagedUserProfiles(int skip, int take) => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>>([]);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>> GetPagedUserProfilesByCompany(int skip, int take, int companyId) => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile>>([]);
        public Task<int> GetRecentAlarmCount(int withinHours) => Task.FromResult(0);
        public Task<int> GetRecentSosCount(int withinHours) => Task.FromResult(0);
        public Task<IReadOnlyList<SmartWatch4G.Domain.Entities.AlarmEvent>> GetRecentAlarms(int withinHours, int limit) => Task.FromResult<IReadOnlyList<SmartWatch4G.Domain.Entities.AlarmEvent>>([]);
        public Task<(int TotalWorkers, int AlarmCount, int SosCount)> GetDashboardCounts(int withinHours) => Task.FromResult((0, 0, 0));
        public Task<(int TotalWorkers, int AlarmCount, int SosCount)> GetDashboardCountsByCompany(int withinHours, int companyId) => Task.FromResult((0, 0, 0));

        // GPS queries
        public Task<(IReadOnlyList<(string DeviceId, string? UserName, SmartWatch4G.Domain.Entities.GnssTrack Track)> Items, int TotalCount)>
            GetGnssTracksByCompany(int companyId, System.DateTime? from, System.DateTime? to,
                int skip, int take, string sortDir, bool onlineOnly, bool offlineOnly)
            => Task.FromResult<(IReadOnlyList<(string, string?, SmartWatch4G.Domain.Entities.GnssTrack)>, int)>(([], 0));
        public Task<(int Online, int Offline)> GetDeviceStatusCountsByCompany(int companyId,
            System.Collections.Generic.IReadOnlyList<string> onlineDeviceIds) => Task.FromResult((0, 0));

        // Health queries
        public Task<(IReadOnlyList<SmartWatch4G.Domain.Entities.HealthSnapshot> Items, int TotalCount)>
            GetHealthSnapshotsByDevice(string deviceId, System.DateTime? from, System.DateTime? to,
                int skip, int take, string sortDir)
            => Task.FromResult<(IReadOnlyList<SmartWatch4G.Domain.Entities.HealthSnapshot>, int)>(([], 0));
        public Task<(IReadOnlyList<(string DeviceId, string? UserName, SmartWatch4G.Domain.Entities.HealthSnapshot Snapshot)> Items, int TotalCount)>
            GetHealthSnapshotsByCompany(int companyId, System.DateTime? from, System.DateTime? to,
                int skip, int take, string sortDir)
            => Task.FromResult<(IReadOnlyList<(string, string?, SmartWatch4G.Domain.Entities.HealthSnapshot)>, int)>(([], 0));
        public Task<IReadOnlyList<(string DeviceId, string? UserName, double? AvgHr, double? AvgSpo2,
            double? AvgFatigue, int? MaxHr, int? MinHr, int? TotalSteps, int Count)>>
            GetHealthSummaryByCompany(int companyId, System.DateTime? from, System.DateTime? to)
            => Task.FromResult<IReadOnlyList<(string, string?, double?, double?, double?, int?, int?, int?, int)>>([]);

        // Device config queries
        public Task<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
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
            double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)?>
            GetDeviceConfig(string deviceId)
            => Task.FromResult<(string, string?, System.DateTime?, bool?, int?, int?, bool?, int?, bool?, int?, bool?, int?, int?, int?, int?, bool?, int?, int?, int?, int?, bool?, int?, bool?, int?, int?, int?, int?, bool?, double?, double?, bool?, int?, string?, int?, string?, int?, int?, bool?, int?, int?, int?, double?, double?, bool?, int?, bool?, bool?, int?, int?, bool?, int?, double?, double?, double?, double?)?>(null);

        public Task<IReadOnlyList<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
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
            double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)>>
            GetDeviceConfigsByCompany(int companyId, int skip, int take)
            => Task.FromResult<IReadOnlyList<(string, string?, System.DateTime?, bool?, int?, int?, bool?, int?, bool?, int?, bool?, int?, int?, int?, int?, bool?, int?, int?, int?, int?, bool?, int?, bool?, int?, int?, int?, int?, bool?, double?, double?, bool?, int?, string?, int?, string?, int?, int?, bool?, int?, int?, int?, double?, double?, bool?, int?, bool?, bool?, int?, int?, bool?, int?, double?, double?, double?, double?)>>([]);

        public Task<int> GetDeviceConfigCountByCompany(int companyId) => Task.FromResult(0);

        public Task<(IReadOnlyList<SmartWatch4G.Domain.Entities.AuditEntry> Items, int TotalCount)> GetAuditLog(
            string? deviceId = null, string? action = null, string? tableName = null,
            System.DateTime? from = null, System.DateTime? to = null, int skip = 0, int take = 50)
            => Task.FromResult<(IReadOnlyList<SmartWatch4G.Domain.Entities.AuditEntry>, int)>(([], 0));
    }
}
