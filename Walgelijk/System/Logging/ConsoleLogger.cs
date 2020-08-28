using System;

namespace Walgelijk
{
    /// <summary>
    /// The default logger. Logs to the console.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        public void Log(object message)
        {
            Write("[LOG]", message.ToString());
        }

        public void Warn(object message)
        {
            Write("[WRN]", message.ToString(), ConsoleColor.DarkYellow);
        }

        public void Error(object message)
        {
            Write("[ERR]", message.ToString(), ConsoleColor.Red);
        }

        private void Write(string prefix, string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(prefix);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
    }
}
