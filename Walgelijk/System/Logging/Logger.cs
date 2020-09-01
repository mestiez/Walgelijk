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
        public static ILogger Implementation { get; set; } = new ConsoleLogger();

        /// <summary>
        /// Log information
        /// </summary>
        public static void Log(object message, object source = null) => Implementation.Log(message, source);
        /// <summary>
        /// Log a warning
        /// </summary>
        public static void Warn(object message, object source = null) => Implementation.Warn(message, source);
        /// <summary>
        /// Log an error
        /// </summary>
        public static void Error(object message, object source = null) => Implementation.Error(message, source);
    }
}
