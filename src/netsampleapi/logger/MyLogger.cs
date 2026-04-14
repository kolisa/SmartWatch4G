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
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        // Define the minimum log level to be written to the file
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
}
