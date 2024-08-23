using Microsoft.Extensions.Logging;

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
        Game.Main.Logger.LogDebug("{Message} ({Source})", message, source);
    }

    /// <summary>
    /// Log information
    /// </summary>
    public static void Log<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        Game.Main.Logger.LogInformation("{Message} ({Source})", message, source);
    }

    /// <summary>
    /// Log a warning
    /// </summary>
    public static void Warn<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        Game.Main.Logger.LogWarning("{Message} ({Source})", message, source);
    }

    /// <summary>
    /// Log an error
    /// </summary>
    public static void Error<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
    {
        Game.Main.Logger.LogError("{Message} ({Source})", message, source);
    }
}
