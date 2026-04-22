namespace KhoiWatchData.Api.Logger;

public class MyFileLogger : ILogger
{
    private readonly string _directory;
    private readonly string _baseFileName;
    private readonly string _extension;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public MyFileLogger(string filePath)
    {
        _directory    = Path.GetDirectoryName(filePath) ?? ".";
        _baseFileName = Path.GetFileNameWithoutExtension(filePath);
        _extension    = Path.GetExtension(filePath);
        Directory.CreateDirectory(_directory);
    }

    private string GetDailyFilePath() =>
        Path.Combine(_directory, $"{_baseFileName}_{System.DateTime.Now:yyyy-MM-dd}{_extension}");

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = $"{System.DateTime.Now}: {logLevel} - {formatter(state, exception)}";
        Task.Run(() => WriteAsync(message));
    }

    private async Task WriteAsync(string message)
    {
        await _semaphore.WaitAsync();
        try
        {
            await using var writer = new StreamWriter(GetDailyFilePath(), append: true);
            await writer.WriteLineAsync(message);
        }
        finally { _semaphore.Release(); }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
