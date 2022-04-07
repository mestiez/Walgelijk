using System;

namespace Walgelijk
{
    /// <summary>
    /// The default logger. Logs to the console.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        public void Debug(string message, string? source = null)
        {
            Write("[DBG]", message, source ?? "null", ConsoleColor.Magenta);
        }

        public void Log(string message, string? source = null)
        {
            Write("[LOG]", message, source ?? "null");
        }

        public void Warn(string message, string? source = null)
        {
            Write("[WRN]", message, source ?? "null", ConsoleColor.DarkYellow);
        }

        public void Error(string message, string? source = null)
        {
            Write("[ERR]", message, source ?? "null", ConsoleColor.Red);
        }

        private void Write(ReadOnlySpan<char> prefix, ReadOnlySpan<char> message, ReadOnlySpan<char> source, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            for (int i = 0; i < prefix.Length; i++)
                Console.Write(prefix[i]);

            Console.Write(' ');

            Console.ForegroundColor = ConsoleColor.Gray;
            for (int i = 0; i < message.Length; i++)
                Console.Write(message[i]);

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
}
