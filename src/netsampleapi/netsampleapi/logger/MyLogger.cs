using Microsoft.Extensions.Logging;
using System;
using System.IO;

public class MyFileLogger : ILogger
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public MyFileLogger(string filePath)
    {
        _filePath = filePath;
        // Ensure the logs directory exists before any write is attempted
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
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
            using (var writer = new StreamWriter(_filePath, true))
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
