using System;

namespace Walgelijk
{
    /// <summary>
    /// Type of console message. Used for filtering
    /// </summary>
    [Flags]
    public enum ConsoleMessageType
    {
        /// <summary>
        /// Shows up regardless of filter
        /// </summary>
        All = 0x1111,
        /// <summary>
        /// Debug message
        /// </summary>
        Debug = 0x0001,
        /// <summary>
        /// Info message
        /// </summary>
        Info = 0x0010,
        /// <summary>
        /// Warning message
        /// </summary>
        Warning = 0x0100,
        /// <summary>
        /// Error message
        /// </summary>
        Error = 0x1000,
    }
}
