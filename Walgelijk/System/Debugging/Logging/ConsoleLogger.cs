using System;

namespace Walgelijk;

/// <summary>
/// The default logger. Logs to the console.
/// </summary>
public class ConsoleLogger : ILogger
{
    public void Debug<T>(T message, string? source = null)
    {
        Write("[DBG]", message, source ?? "null", ConsoleColor.Magenta);
    }

    public void Log<T>(T message, string? source = null)
    {
        Write("[INF]", message, source ?? "null");
    }

    public void Warn<T>(T message, string? source = null)
    {
        Write("[WRN]", message, source ?? "null", ConsoleColor.DarkYellow);
    }

    public void Error<T>(T message, string? source = null)
    {
        Write("[ERR]", message, source ?? "null", ConsoleColor.Red);
    }

    private static void Write<T>(ReadOnlySpan<char> prefix, T message, ReadOnlySpan<char> source, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        for (int i = 0; i < prefix.Length; i++)
            Console.Write(prefix[i]);

        Console.Write(' ');

        Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(message?.ToString() ?? "NULL");

        if (source != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(' ');
            for (int i = 0; i < source.Length; i++)
                Console.Write(source[i]);
        }

        Console.Write(Environment.NewLine);
    }
}
