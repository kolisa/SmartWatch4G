using Microsoft.Extensions.Logging;
using System;

public class MyFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public MyFileLoggerProvider(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new MyFileLogger(_filePath);
    }

    public void Dispose()
    {
        // Implement if needed to dispose resources
    }
}
