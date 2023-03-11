using System;

namespace Walgelijk
{
    /// <summary>
    /// Attribute that registers a command to the command processor registry
    /// </summary>
    public class CommandAttribute : Attribute
    {
        public string? Alias = null;

        /// <summary>
        /// String that is displayed when the command is incorrectly used or when help is requested
        /// </summary>
        public string HelpString = string.Empty;

    }
}
