using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
