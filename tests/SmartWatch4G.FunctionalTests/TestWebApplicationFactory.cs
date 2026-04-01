using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
using SmartWatch4G.FunctionalTests.Stubs;
using SmartWatch4G.Infrastructure.Persistence;

namespace SmartWatch4G.FunctionalTests;

/// <summary>
/// Spins up the real ASP.NET pipeline in "Testing" mode.
/// All repository interfaces are replaced with in-memory stubs so no SQL Server is required.
/// The "Testing" environment flag causes Program.cs to skip the EF Core migration call.
/// The server is pre-warmed in the constructor so concurrent Theory test constructors
/// calling <see cref="WebApplicationFactory{T}.CreateClient()"/> do not race during startup.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory()
    {
        // Force server startup now (single-threaded, before any test runs).
        _ = Server;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace AppDbContext with an in-memory database so EF Core
            // infrastructure registrations don't fail when processors are resolved.
            RemoveRegistration<DbContextOptions<AppDbContext>>(services);
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("SmartWatch4G_Test"));

            // Replace all repository interfaces with stubs.
            Replace<IDeviceInfoRepository, StubDeviceInfoRepository>(services);
            Replace<IDeviceStatusRepository, StubDeviceStatusRepository>(services);
            Replace<IAlarmRepository, StubAlarmRepository>(services);
            Replace<ICallLogRepository, StubCallLogRepository>(services);
            Replace<IHealthDataRepository, StubHealthDataRepository>(services);
            Replace<IGnssTrackRepository, StubGnssTrackRepository>(services);
            Replace<ISleepDataRepository, StubSleepDataRepository>(services);
            Replace<IRriDataRepository, StubRriDataRepository>(services);
            Replace<ISpo2DataRepository, StubSpo2DataRepository>(services);
            Replace<IAccDataRepository, StubAccDataRepository>(services);

            // Replace domain services.
            Replace<ISleepQueryService, StubSleepQueryService>(services);
        });
    }

    private static void Replace<TService, TImplementation>(IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        RemoveRegistration<TService>(services);
        services.AddScoped<TService, TImplementation>();
    }

    private static void RemoveRegistration<T>(IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor is not null)
            services.Remove(descriptor);
    }
}
