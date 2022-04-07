using System;
using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// Struct that contains all information associated with a logged message
    /// </summary>
    public struct LogMessage
    {
        /// <summary>
        /// Object sent by the source.
        /// </summary>
        public string Message;
        /// <summary>
        /// Object that sent the message. Can be null.
        /// </summary>
        public string? Source;
        /// <summary>
        /// Level of this message
        /// </summary>
        public LogLevel Level;

        public LogMessage(string message, string? source, LogLevel level)
        {
            this.Message = message;
            this.Source = source;
            this.Level = level;
        }

        public override bool Equals(object? obj)
        {
            return obj is LogMessage other &&
                   EqualityComparer<object>.Default.Equals(Message, other.Message) &&
                   EqualityComparer<object>.Default.Equals(Source, other.Source) &&
                   Level == other.Level;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Message, Source, Level);
        }

        public void Deconstruct(out string message, out string? source, out LogLevel level)
        {
            message = this.Message;
            source = this.Source;
            level = this.Level;
        }

        public static implicit operator (string message, string? source, LogLevel level)(LogMessage value)
        {
            return (value.Message, value.Source, value.Level);
        }

        public static implicit operator LogMessage((string message, string? source, LogLevel level) value)
        {
            return new LogMessage(value.message, value.source, value.level);
        }
    }
}
