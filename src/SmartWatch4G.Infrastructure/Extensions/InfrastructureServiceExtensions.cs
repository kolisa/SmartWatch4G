using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces;
using SmartWatch4G.Infrastructure.Persistence;
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
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IDeviceSettingsService, DeviceSettingsService>();

        // ── Device status cache (singleton — shared with background polling job) ──
        services.AddSingleton<IDeviceStatusCache, DeviceStatusCache>();

        // ── Application services ──────────────────────────────────────────────
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ITrackingQueryService, TrackingQueryService>();
        services.AddScoped<IWorkerQueryService, WorkerQueryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAlertQueryService, AlertQueryService>();

        // ── Processors ────────────────────────────────────────────────────────
        services.AddSingleton<OldManProcessor>();
        services.AddSingleton<HistoryDataProcessor>();
        services.AddSingleton<AlarmProcessor>();
        services.AddSingleton<AfPreprocessor>();
        services.AddSingleton<EcgPreprocessor>();
        services.AddSingleton<SleepPreprocessor>();

        // ── iwown command service ─────────────────────────────────────────────
        services.AddHttpClient<IwownService>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Iwown:CommandBaseUrl"] ?? "https://euapi.iwown.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── iwown calculation service ─────────────────────────────────────────
        services.AddHttpClient<IwownCalculationService>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Iwown:AlgoBaseUrl"] ?? "https://iwap1.iwown.com/algoservice");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
