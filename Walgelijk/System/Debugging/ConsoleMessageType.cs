using System;

namespace Walgelijk
{
    /// <summary>
    /// Type of console message. Used for filtering
    /// </summary>
    [Flags]
    public enum ConsoleMessageType : byte
    {
        // if you decide to expand this, don't forget to update DebugConsoleRenderer.filterButtons

        /// <summary>
        /// Shows up regardless of filter
        /// </summary>
        All = byte.MaxValue,
        Plain = 1,
        /// <summary>
        /// Debug message
        /// </summary>
        Debug = 2,
        /// <summary>
        /// Info message
        /// </summary>
        Info = 4,
        /// <summary>
        /// Warning message
        /// </summary>
        Warning = 8,
        /// <summary>
        /// Error message
        /// </summary>
        Error = 16,
    }
}
