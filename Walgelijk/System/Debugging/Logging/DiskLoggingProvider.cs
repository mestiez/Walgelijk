using System.IO;
using Microsoft.Extensions.Logging;

namespace Walgelijk;

public class DiskLoggingProvider : ILoggerProvider
{
    private readonly DirectoryInfo logsDir;

    public DiskLoggingProvider(DirectoryInfo logsDir)
    {
        this.logsDir = logsDir;
    }

    public ILogger CreateLogger(string categoryName) => new DiskLogger(logsDir);

    public void Dispose()
    {
    }
}
