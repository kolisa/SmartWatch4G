using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
using SmartWatch4G.Infrastructure.Persistence;
using SmartWatch4G.Infrastructure.Persistence.Repositories;
using SmartWatch4G.Infrastructure.Processors;
using SmartWatch4G.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartWatch4G.Infrastructure.Extensions;

/// <summary>
/// Registers all Infrastructure-layer services into the DI container.
/// Called from SmartWatch4G.Api / Program.cs.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=fourGData.db"));

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IDeviceInfoRepository, DeviceInfoRepository>();
        services.AddScoped<IDeviceStatusRepository, DeviceStatusRepository>();
        services.AddScoped<ICallLogRepository, CallLogRepository>();
        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<IHealthDataRepository, HealthDataRepository>();
        services.AddScoped<IGnssTrackRepository, GnssTrackRepository>();
        services.AddScoped<ISleepDataRepository, SleepDataRepository>();
        services.AddScoped<IRriDataRepository, RriDataRepository>();
        services.AddScoped<ISpo2DataRepository, Spo2DataRepository>();
        services.AddScoped<IAccDataRepository, AccDataRepository>();

        // ── iwown algorithm service HTTP client ───────────────────────────────
        WownAlgoOptions algoOpts = configuration
            .GetSection(WownAlgoOptions.SectionName)
            .Get<WownAlgoOptions>() ?? new WownAlgoOptions();

        services.Configure<WownAlgoOptions>(
            configuration.GetSection(WownAlgoOptions.SectionName));

        services.AddHttpClient<IWownAlgoClient, WownAlgoClient>(client =>
        {
            client.BaseAddress = new Uri(algoOpts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── Domain service implementations ────────────────────────────────────
        services.AddScoped<ISleepQueryService, SleepQueryService>();
        services.AddScoped<IProtobufPacketHandler, ProtobufPacketDispatcher>();

        // ── Processors (scoped so they share the same DbContext per request) ──
        services.AddScoped<HistoryDataProcessor>();
        services.AddScoped<OldManProcessor>();
        services.AddScoped<AlarmProcessor>();
        services.AddScoped<AfPreprocessor>();
        services.AddScoped<EcgPreprocessor>();
        services.AddScoped<SleepPreprocessor>();

        return services;
    }
}
