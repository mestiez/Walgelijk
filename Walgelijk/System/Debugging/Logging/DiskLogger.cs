using System;
using System.IO;

namespace Walgelijk;

/// <summary>
/// A disk logger. Logs to the <see cref="TargetPath"/>
/// </summary>
public class DiskLogger : ILogger, IDisposable
{
    public string TargetPath = "output.log";
    private StreamWriter? output;

    public DiskLogger()
    {
        output = new StreamWriter(TargetPath, false, global::System.Text.Encoding.UTF8);
    }

    public void Debug<T>(T message, string? source = null)
    {
        output?.WriteLine("[DBG] {0} ({1})", message, source);
    }

    public void Error<T>(T message, string? source = null)
    {
        output?.WriteLine("[ERR] {0} ({1})", message, source);
    }

    public void Log<T>(T message, string? source = null)
    {
        output?.WriteLine("[INF] {0} ({1})", message, source);
    }

    public void Warn<T>(T message, string? source = null)
    {
        output?.WriteLine("[WRN] {0} ({1})", message, source);
    }

    public void Dispose()
    {
        output?.Close();
        output?.Dispose();
        output = null;
    }
}
