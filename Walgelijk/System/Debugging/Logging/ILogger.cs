namespace Walgelijk
{
    /// <summary>
    /// Interface for basic logging
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Debug information
        /// </summary>
        public void Debug<T>(T message, string? source = null);
        /// <summary>
        /// Log information
        /// </summary>
        public void Log<T>(T message, string? source = null);
        /// <summary>
        /// Log a warning
        /// </summary>
        public void Warn<T>(T message, string? source = null);
        /// <summary>
        /// Log an error
        /// </summary>
        public void Error<T>(T message, string? source = null);
    }
}
