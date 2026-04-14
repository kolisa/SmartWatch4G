using SampleApi.Calculation;
using SampleApi.Parser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Disable default camelCase
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<HistoryDataProcessor>();
builder.Services.AddSingleton<OldManProcessor>();
builder.Services.AddSingleton<AlarmProcessor>();

builder.Services.AddSingleton<AfPreprocessor>();
builder.Services.AddSingleton<EcgPreprocessor>();
builder.Services.AddSingleton<SleepPreprocessor>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new MyFileLoggerProvider("logs/myapi.log"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
