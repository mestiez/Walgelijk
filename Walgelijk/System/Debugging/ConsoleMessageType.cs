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
        /// <summary>
        /// Just raw text, man
        /// </summary>
        None = byte.MinValue,
        /// <summary>
        /// Debug message
        /// </summary>
        Debug = 1,
        /// <summary>
        /// Info message
        /// </summary>
        Info = 2,
        /// <summary>
        /// Warning message
        /// </summary>
        Warning = 4,
        /// <summary>
        /// Error message
        /// </summary>
        Error = 8,
    }
}
