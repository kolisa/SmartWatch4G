using KhoiWatchData.Api.Logger;
using KhoiWatchData.Api.Middleware;
using KhoiWatchData.Api.Storage;
using SmartWatch4G.Domain.Interfaces;
using SmartWatch4G.Infrastructure.Extensions;
using SmartWatch4G.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Force DatabaseService singleton resolution so InitializeSchema() runs at startup.
//if (app.Environment.IsEnvironment("Testing"))
//    app.Services.GetRequiredService<IDatabaseService>();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

await app.RunAsync();
