using System.IO.Compression;
using System.Threading.RateLimiting;

using Asp.Versioning;

using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Serilog;

using SmartWatch4G.Api;
using SmartWatch4G.Infrastructure.Extensions;
using SmartWatch4G.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // ── Kestrel — tune for high device-count concurrency ─────────────────────
    builder.WebHost.ConfigureKestrel(kestrel =>
    {
        kestrel.Limits.MaxConcurrentConnections = 10_000;
        kestrel.Limits.MaxConcurrentUpgradedConnections = 10_000;
        // Devices send small protobuf payloads; 4 MB is more than enough per request.
        kestrel.Limits.MaxRequestBodySize = 4 * 1024 * 1024;
        kestrel.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(130);
        kestrel.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    });

    // ── Serilog (replaces original MyFileLoggerProvider) ──────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("Application", "SmartWatch4G")
           .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
           .WriteTo.Console(
               outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
           .WriteTo.File(
               path: "logs/smartwatch4g-.log",
               rollingInterval: RollingInterval.Day,
               retainedFileCountLimit: 14,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

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
    //   "dashboard-api" — app / dashboard endpoints: 300 requests / 60 s per IP (sliding window)
    //   "device-write"  — device-facing write endpoints: 120 requests / 60 s per IP (fixed window)
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy("dashboard-api", httpContext =>
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
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy = null; // keep PascalCase
            // Wire in the source-generated context for allocation-free serialization.
            opts.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
        });

    // ── Response compression (Brotli preferred, Gzip fallback) ───────────────
    // Fleet endpoints with 100 000 devices return large JSON arrays; compression
    // can reduce payload size by 70–90% which saves bandwidth and improves latency.
    builder.Services.AddResponseCompression(opts =>
    {
        opts.EnableForHttps = true;
        opts.Providers.Add<BrotliCompressionProvider>();
        opts.Providers.Add<GzipCompressionProvider>();
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Append("application/json");
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(opts =>
        opts.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(opts =>
        opts.Level = CompressionLevel.Fastest);

    // ── Output caching removed — device data changes continuously from 100 000+
    // devices; serving a stale snapshot is not acceptable. Compression (above)
    // reduces payload size without sacrificing freshness.

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

    // ── Health checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    // ── Infrastructure (DB, repositories, processors) ────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    WebApplication app = builder.Build();

    // ── Auto-migrate on startup (Development only; Production uses CI/CD pipeline migrations) ──
    if (app.Environment.IsDevelopment())
    {
        using IServiceScope scope = app.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    // ── Swagger (all environments) ────────────────────────────────────────────
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "4G Wearable API v1"));

    app.UseCors();
    app.UseResponseCompression();
    app.UseSerilogRequestLogging(opts =>
    {
        // Enrich every request log entry with the response status code and elapsed ms.
        // These appear in the structured log as {StatusCode} and {Elapsed}.
        opts.EnrichDiagnosticContext = (diag, ctx) =>
        {
            diag.Set("RequestHost", ctx.Request.Host.Value ?? string.Empty);
            diag.Set("RequestScheme", ctx.Request.Scheme);
            diag.Set("RemoteIp", ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        };
        // Suppress health-check and swagger noise from the request log.
        opts.GetLevel = (ctx, elapsed, ex) =>
        {
            if (ex is not null || ctx.Response.StatusCode >= 500)
                return Serilog.Events.LogEventLevel.Error;
            if (ctx.Response.StatusCode >= 400)
                return Serilog.Events.LogEventLevel.Warning;
            return Serilog.Events.LogEventLevel.Information;
        };
    });
    app.UseMiddleware<SmartWatch4G.Api.Middleware.GlobalExceptionMiddleware>();
    app.UseRateLimiter();
    app.UseHttpsRedirection();
    app.MapHealthChecks("/health");
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
