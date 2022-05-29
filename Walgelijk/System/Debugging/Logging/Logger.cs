using System;
using System.Collections.Concurrent;

namespace Walgelijk
{
    /// <summary>
    /// Access the logging implementation
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// The logging implementation used. Set to <see cref="ConsoleLogger"/> by default
        /// </summary>
        public static ConcurrentBag<ILogger> Implementations { get; set; } = new ConcurrentBag<ILogger>(new ILogger[] {
            new ConsoleLogger(),
            new DiskLogger()
        });

        /// <summary>
        /// Log information
        /// </summary>
        public static void Debug(in string message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Debug(message, source);

            OnLog.Dispatch((message, source, LogLevel.Debug));
        }

        /// <summary>
        /// Log information
        /// </summary>
        public static void Log(in string message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Log(message, source);

            OnLog.Dispatch((message, source, LogLevel.Info));
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        public static void Warn(in string message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Warn(message, source);

            OnLog.Dispatch((message, source, LogLevel.Warn));
        }

        /// <summary>
        /// Log an error
        /// </summary>
        public static void Error(in string message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Error(message, source);

            OnLog.Dispatch((message, source, LogLevel.Error));
        }

        /// <summary>
        /// Event dispatched when a message is logged at any level
        /// </summary>
        public static readonly Hook<LogMessage> OnLog = new Hook<LogMessage>();

        public static void Dispose()
        {
            foreach (var impl in Implementations)
                if (impl != null && impl is IDisposable disp)
                    disp.Dispose();
        }
    }
}
