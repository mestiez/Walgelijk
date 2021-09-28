using System;

namespace Walgelijk
{
    /// <summary>
    /// The default logger. Logs to the console.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        public void Debug(object message, object source)
        {
            Write("[DBG]", message.ToString(), source, ConsoleColor.Magenta);
        }

        public void Log(object message, object source)
        {
            Write("[LOG]", message.ToString(), source);
        }

        public void Warn(object message, object source)
        {
            Write("[WRN]", message.ToString(), source, ConsoleColor.DarkYellow);
        }

        public void Error(object message, object source)
        {
            Write("[ERR]", message.ToString(), source, ConsoleColor.Red);
        }

        private void Write(string prefix, string message, object source, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(prefix);
            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(message);

            if (source != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" ");
                Console.Write(source.ToString());
            }

            Console.Write(Environment.NewLine);
        }
    }
}
