using Asp.Versioning;
using KhoiWatchData.Api.Logger;
using KhoiWatchData.Api.Middleware;
using KhoiWatchData.Api.Storage;
using Microsoft.OpenApi.Models;
using SmartWatch4G.Domain.Interfaces;
using SmartWatch4G.Infrastructure.Extensions;
using SmartWatch4G.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion        = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions        = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat           = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartWatch4G API", Version = "v1" });
});

// Infrastructure: DatabaseService, processors, iwown HTTP clients
builder.Services.AddInfrastructure(builder.Configuration);

// Raw file store (local to API)
builder.Services.AddSingleton<RawDataFileStore>();

// Jobs: FileSystemWatcher + Quartz fallback poller
builder.Services.AddJobs(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new MyFileLoggerProvider(
    Path.Combine(builder.Environment.ContentRootPath, "logs", "SmartWatch4gData.log")));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWatch4G API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();

await app.RunAsync();
