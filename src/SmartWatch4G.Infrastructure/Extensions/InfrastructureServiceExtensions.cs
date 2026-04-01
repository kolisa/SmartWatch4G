using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
using SmartWatch4G.Infrastructure.Persistence;
using SmartWatch4G.Infrastructure.Persistence.Repositories;
using SmartWatch4G.Infrastructure.Processors;
using SmartWatch4G.Infrastructure.Services;

namespace SmartWatch4G.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .ReplaceService<IMigrationsModelDiffer, SafeMigrationsModelDiffer>());

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

        // ── iwown algorithm service (sleep / ECG / AF) ────────────────────────
        var algoOpts = configuration.GetSection(WownAlgoOptions.SectionName).Get<WownAlgoOptions>()
                       ?? new WownAlgoOptions();
        services.Configure<WownAlgoOptions>(configuration.GetSection(WownAlgoOptions.SectionName));
        services.AddHttpClient<IWownAlgoClient, WownAlgoClient>(client =>
        {
            client.BaseAddress = new Uri(algoOpts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── iwown command service (send commands to devices / entservice) ─────
        var cmdOpts = configuration.GetSection(WownCommandOptions.SectionName).Get<WownCommandOptions>()
                      ?? new WownCommandOptions();
        services.Configure<WownCommandOptions>(configuration.GetSection(WownCommandOptions.SectionName));
        services.AddHttpClient<IWownCommandClient, WownCommandClient>(client =>
        {
            client.BaseAddress = new Uri(cmdOpts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── Domain service implementations ────────────────────────────────────
        services.AddScoped<ISleepQueryService, SleepQueryService>();
        services.AddScoped<IProtobufPacketHandler, ProtobufPacketDispatcher>();

        // ── Application query services (API layer depends on these, never on repositories) ──
        services.AddScoped<IDeviceQueryService, DeviceQueryService>();
        services.AddScoped<IHealthQueryService, HealthQueryService>();
        services.AddScoped<IAlarmQueryService, AlarmQueryService>();
        services.AddScoped<ILocationQueryService, LocationQueryService>();
        services.AddScoped<ICallLogQueryService, CallLogQueryService>();
        services.AddScoped<ISpo2QueryService, Spo2QueryService>();
        services.AddScoped<IEcgQueryService, EcgQueryService>();
        services.AddScoped<IRriQueryService, RriQueryService>();
        services.AddScoped<IAccelerometerQueryService, AccelerometerQueryService>();

        // ── Processors (scoped — share same DbContext per request) ────────────
        services.AddScoped<HistoryDataProcessor>();
        services.AddScoped<OldManProcessor>();
        services.AddScoped<AlarmProcessor>();
        services.AddScoped<AfPreprocessor>();
        services.AddScoped<EcgPreprocessor>();
        services.AddScoped<SleepPreprocessor>();

        return services;
    }
}
