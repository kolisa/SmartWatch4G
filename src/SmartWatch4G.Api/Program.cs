using System.Threading.RateLimiting;

using Asp.Versioning;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Serilog;

using SmartWatch4G.Infrastructure.Extensions;
using SmartWatch4G.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // ── Serilog (replaces original MyFileLoggerProvider) ──────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File(
               path: "logs/fourgdata-.log",
               rollingInterval: RollingInterval.Day,
               retainedFileCountLimit: 14));

    // ── API Versioning ────────────────────────────────────────────────────────
    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    // ── Rate Limiting ─────────────────────────────────────────────────────────
    // Two named policies:
    //   "app-read"    — app / dashboard endpoints: 300 requests / 60 s per IP (sliding window)
    //   "device-write"— legacy device-facing write endpoints: 120 requests / 60 s per IP (fixed window)
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy("app-read", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 300,
                    Window = TimeSpan.FromSeconds(60),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        options.AddPolicy("device-write", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromSeconds(60),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    // ── MVC / API ─────────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
            opts.JsonSerializerOptions.PropertyNamingPolicy = null); // keep PascalCase

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "4G Wearable Data API",
            Version = "v1",
            Description = "Read-only app/dashboard API for the 4G wearable data platform."
        });
    });

    // ── Infrastructure (DB, repositories, processors) ────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    WebApplication app = builder.Build();

    // ── Auto-migrate on startup (skipped in Testing environment) ─────────────
    if (!app.Environment.IsEnvironment("Testing"))
    {
        using IServiceScope scope = app.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    // ── HTTP pipeline ─────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "4G Wearable API v1"));
    }

    app.UseCors();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<SmartWatch4G.Api.Middleware.GlobalExceptionMiddleware>();
    app.UseRateLimiter();
    app.UseHttpsRedirection();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Required for WebApplicationFactory<Program> in functional tests.
public partial class Program { }
