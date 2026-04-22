namespace KhoiWatchData.Api.Logger;

public class MyFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public MyFileLoggerProvider(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    // Suppress Quartz internals, ASP.NET Core hosting lifetime, and HTTPS redirect noise.
    private static readonly string[] _suppressedPrefixes =
    [
        "Quartz.",
        "Microsoft.Hosting.",
        "Microsoft.AspNetCore.Hosting.",
        "Microsoft.AspNetCore.HttpsPolicy.",
        "Microsoft.AspNetCore.Server.",
        "Microsoft.Extensions.Hosting.",
    ];

    public ILogger CreateLogger(string categoryName)
    {
        foreach (var prefix in _suppressedPrefixes)
            if (categoryName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return NullLogger.Instance;
        return new MyFileLogger(_filePath);
    }

    private sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    public void Dispose() { }
}
