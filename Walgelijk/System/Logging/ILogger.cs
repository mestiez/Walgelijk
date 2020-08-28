namespace Walgelijk
{
    /// <summary>
    /// Interface for basic logging
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log information
        /// </summary>
        public void Log(object message);
        /// <summary>
        /// Log a warning
        /// </summary>
        public void Warn(object message);
        /// <summary>
        /// Log an error
        /// </summary>
        public void Error(object message);
    }
}
