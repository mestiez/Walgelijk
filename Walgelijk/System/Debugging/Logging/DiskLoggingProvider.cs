using Microsoft.Extensions.Logging;

namespace Walgelijk;

public class DiskLoggingProvider : ILoggerProvider
{
    private readonly string logPath;

    public DiskLoggingProvider(string logPath)
    {
        this.logPath = logPath;
    }

    public ILogger CreateLogger(string categoryName) => new DiskLogger(logPath);

    public void Dispose()
    {
    }
}
