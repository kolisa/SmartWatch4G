using SampleApi.Calculation;
using SampleApi.Data;
using SampleApi.Parser;
using SampleApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Disable default camelCase
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddHostedService<LogFileMonitorService>();

builder.Services.AddSingleton<HistoryDataProcessor>();
builder.Services.AddSingleton<OldManProcessor>();
builder.Services.AddSingleton<AlarmProcessor>();

builder.Services.AddSingleton<AfPreprocessor>();
builder.Services.AddSingleton<EcgPreprocessor>();
builder.Services.AddSingleton<SleepPreprocessor>();

builder.Services.AddHttpClient<IwownService>(client =>
{
    client.BaseAddress = new Uri("https://euapi.iwown.com");
});

builder.Services.AddHttpClient<IwownCalculationService>(client =>
{
    client.BaseAddress = new Uri("https://iwap1.iwown.com/algoservice");
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new MyFileLoggerProvider("logs/myapi.log"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
