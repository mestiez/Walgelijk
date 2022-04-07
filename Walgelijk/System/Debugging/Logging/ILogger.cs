using System;

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
        public void Debug(string message, string? source = null);
        /// <summary>
        /// Log information
        /// </summary>
        public void Log(string message, string? source = null);
        /// <summary>
        /// Log a warning
        /// </summary>
        public void Warn(string message, string? source = null);
        /// <summary>
        /// Log an error
        /// </summary>
        public void Error(string message, string? source = null);
    }
}
