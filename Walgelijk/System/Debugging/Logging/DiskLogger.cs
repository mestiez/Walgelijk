using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Walgelijk;

public class DiskLogger : ILogger, IDisposable
{
    private readonly DirectoryInfo diskLogDirectory;
    private readonly StreamWriter writer;

    public DiskLogger(DirectoryInfo logsDir, int maxLogFiles = 2)
    {
        diskLogDirectory = logsDir;

        if (diskLogDirectory.Exists)
        {
            FileInfo[] files = [.. diskLogDirectory.EnumerateFiles().OrderByDescending(x => x.CreationTime)];

            if (files.Length > maxLogFiles)
                foreach (var file in files.Skip(maxLogFiles))
                    file.Delete();
        }

        var logFileName = Path.Combine(diskLogDirectory.FullName, $"log-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        writer = new(logFileName, append: true, encoding: Encoding.UTF8);
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

        writer?.WriteLine(formatter(state, exception));
        writer?.Flush();
    }

    public void Dispose()
    {
        writer.Dispose();
    }
}