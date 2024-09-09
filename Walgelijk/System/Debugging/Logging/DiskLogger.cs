using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Walgelijk;

public class DiskLogger : ILogger, IDisposable
{
    private readonly string diskLogPath;
    private readonly FileStream logFile;
    private readonly StreamWriter writer;

    public DiskLogger(string logPath)
    {
        diskLogPath = logPath;

        logFile = File.Open(diskLogPath, FileMode.OpenOrCreate);
        writer = new(logFile);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Debug:
            case LogLevel.Information:
            case LogLevel.Warning:
            case LogLevel.Error:
            case LogLevel.Critical:
                return true;
            default:
                return false;
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        writer.WriteLine(formatter(state, exception), MapLevel(logLevel));
    }

    private static ConsoleMessageType MapLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Debug => ConsoleMessageType.Debug,
            LogLevel.Information => ConsoleMessageType.Info,
            LogLevel.Warning => ConsoleMessageType.Warning,
            LogLevel.Error or LogLevel.Critical => ConsoleMessageType.Error,
            _ => ConsoleMessageType.Info,
        };
    }

    public void Dispose()
    {
        writer.Close();
        logFile.Close();
    }
}