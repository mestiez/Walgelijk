using Microsoft.Extensions.Logging;
using System;

namespace Walgelijk;

public class GameConsoleLogger : ILogger
{
    private readonly Game Game;

    public GameConsoleLogger(Game game)
    {
        this.Game = game;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

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
        // we are not strict about these objects existing. the game console should never be the only logging method.
        Game?.Console?.WriteLine(formatter(state, exception), MapLevel(logLevel));
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
}
