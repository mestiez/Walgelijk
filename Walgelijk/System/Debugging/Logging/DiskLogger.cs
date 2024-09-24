using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Walgelijk;

public class DiskLogger : ILogger, IDisposable
{
    private readonly DirectoryInfo diskLogDirectory;
    private readonly FileStream logFile;
    private readonly StreamWriter writer;

    // Means 2 log files will exist at a time.
    private const int maxLogFiles = 1;

    public DiskLogger(DirectoryInfo logsDir)
    {
        diskLogDirectory = logsDir;

        if (diskLogDirectory.Exists)
        {
            var files = diskLogDirectory.GetFiles().OrderByDescending(x => x.CreationTime).ToArray();
            if (files.Length >= maxLogFiles)
            {
                foreach (var file in files.Skip(maxLogFiles))
                    file.Delete();
            }
        }

        var logFileName = string.Format("{0}\\log-{1}.txt", diskLogDirectory.Name, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        logFile = File.Open(logFileName, FileMode.OpenOrCreate);
        writer = new(logFile, Encoding.UTF8);
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

        writer.WriteLine(formatter(state, exception));
    }

    public void Dispose()
    {
        writer.Close();
        logFile.Close();
    }
}