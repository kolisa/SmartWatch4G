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
        public void InsertGpsTrack(string d, string t, double lon, double lat, string loc) { }
        public void UpsertHealthSnapshot(string d, string rt,
            int? bat=null, int? rssi=null, int? stp=null, double? dist=null, double? cal=null,
            int? ahr=null, int? xhr=null, int? nhr=null, int? spo=null, int? sbp=null, int? dbp=null, int? fat=null) { }
        public void InsertAlarm(string d, string t, string type, string? details=null) { }
        public void InsertSosEvent(string d, string t, double? lat, double? lon,
            string? num, int? status, string? start, string? end) { }
        public void InsertDeviceInfo(string d, string rat,
            string? model, string? ver, string? wear, string? sig, string raw) { }
        public void InsertSleepCalculation(string d, string rd, int comp,
            string? st, string? et, int hr, int tt,
            double? ra, double? rx, double? rn, string? sec) { }
        public void InsertEcgCalculation(string d, int result, int hr, int eff, int dir) { }
        public void InsertAfCalculation(string d, int result) { }
        public void InsertSpo2Calculation(string d, double score, int? risk) { }

        // User profile methods
        public void UpsertUserProfile(string deviceId, string name, string surname,
            string? email=null, string? cell=null, string? empNo=null, string? address=null,
            int? companyId=null) { }
        public SmartWatch4G.Domain.Entities.UserProfile? GetUserProfile(string deviceId) => null;
        public IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile> GetAllUserProfiles() => [];
        public void DeleteUserProfile(string deviceId) { }

        // Company methods
        public int CreateCompany(string name, string? reg, string? email, string? phone, string? addr) => -1;
        public SmartWatch4G.Domain.Entities.Company? GetCompany(int id) => null;
        public IReadOnlyList<SmartWatch4G.Domain.Entities.Company> GetAllCompanies() => [];
        public void UpdateCompany(int id, string name, string? reg, string? email, string? phone, string? addr) { }
        public void DeleteCompany(int id) { }
        public void LinkUserToCompany(string deviceId, int? companyId) { }
        public int BackfillDeviceRecords(string deviceId) => 0;
        public IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile> GetUsersByCompanyId(int companyId) => [];
        public void ReactivateUserProfile(string deviceId) { }

        // GNSS query methods
        public SmartWatch4G.Domain.Entities.GnssTrack? GetLatestGnssTrack(string deviceId) => null;
        public IReadOnlyList<SmartWatch4G.Domain.Entities.GnssTrack> GetGnssTracks(
            string deviceId, System.DateTime? from, System.DateTime? to) => [];

        // Health / stats query methods
        public SmartWatch4G.Domain.Entities.HealthSnapshot? GetLatestHealthSnapshot(string deviceId) => null;
        public int GetActiveWorkerCount() => 0;
        public IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile> GetPagedUserProfiles(int skip, int take) => [];
        public int GetRecentAlarmCount(int withinHours) => 0;
        public int GetRecentSosCount(int withinHours) => 0;
        public IReadOnlyList<SmartWatch4G.Domain.Entities.AlarmEvent> GetRecentAlarms(int withinHours, int limit) => [];
        public (int TotalWorkers, int AlarmCount, int SosCount) GetDashboardCounts(int withinHours) => (0, 0, 0);
    }
}
