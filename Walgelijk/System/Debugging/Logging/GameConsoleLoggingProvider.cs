using Microsoft.Extensions.Logging;

namespace Walgelijk;

public class GameConsoleLoggingProvider : ILoggerProvider
{
    private readonly Game game;

    public GameConsoleLoggingProvider(Game game)
    {
        this.game = game;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new GameConsoleLogger(game);
    }

    public void Dispose()
    {
    }
}
