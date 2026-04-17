using Microsoft.Extensions.Logging;
using System;
using System.IO;

public class MyFileLogger : ILogger
{
    private readonly string _directory;
    private readonly string _baseFileName;
    private readonly string _extension;
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public MyFileLogger(string filePath)
    {
        _directory = Path.GetDirectoryName(filePath) ?? ".";
        _baseFileName = Path.GetFileNameWithoutExtension(filePath);
        _extension = Path.GetExtension(filePath);
        Directory.CreateDirectory(_directory);
    }

    private string GetDailyFilePath()
    {
        var date = System.DateTime.Now.ToString("yyyy-MM-dd");
        return Path.Combine(_directory, $"{_baseFileName}_{date}{_extension}");
    }

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = $"{System.DateTime.Now}: {logLevel.ToString()} - {formatter(state, exception)}";

        Task.Run(() => WriteMessageAsync(message));
    }

    private async Task WriteMessageAsync(string message)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using (var writer = new StreamWriter(GetDailyFilePath(), true))
            {
                await writer.WriteLineAsync(message);
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new NullScope();
        public void Dispose() { }
    }
}
