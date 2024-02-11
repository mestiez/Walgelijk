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
        });

        /// <summary>
        /// Log information
        /// </summary>
        public static void Debug<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Debug(message, source);
        }

        /// <summary>
        /// Log information
        /// </summary>
        public static void Log<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Log(message, source);
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        public static void Warn<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Warn(message, source);
        }

        /// <summary>
        /// Log an error
        /// </summary>
        public static void Error<T>(in T message, [global::System.Runtime.CompilerServices.CallerMemberName] in string? source = null)
        {
            foreach (var impl in Implementations)
                impl.Error(message, source);
        }

        public static void Dispose()
        {
            foreach (var impl in Implementations)
                if (impl != null && impl is IDisposable disp)
                    disp.Dispose();
        }
    }
}
