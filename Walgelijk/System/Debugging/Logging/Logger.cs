using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Walgelijk;

/// <summary>
/// Access the logging implementation
/// </summary>
public static class Logger
{
    /// <summary>
    /// Log information
    /// </summary>
    public static void Debug<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        GetLogger()?.LogDebug("{Message} ({Source})", message, source);
    }

    /// <summary>
    /// Log information
    /// </summary>
    public static void Log<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        GetLogger()?.LogInformation("{Message} ({Source})", message, source);
    }

    /// <summary>
    /// Log a warning
    /// </summary>
    public static void Warn<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        GetLogger()?.LogWarning("{Message} ({Source})", message, source);
    }

    /// <summary>
    /// Log an error
    /// </summary>
    public static void Error<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        GetLogger()?.LogError("{Message} ({Source})", message, source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ILogger? GetLogger() => Game.Main?.Logger;
}
