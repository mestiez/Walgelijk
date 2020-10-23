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
        public object Message;
        /// <summary>
        /// Object that sent the message. Can be null.
        /// </summary>
        public object Source;
        /// <summary>
        /// Level of this message
        /// </summary>
        public LogLevel Level;

        public LogMessage(object message, object source, LogLevel level)
        {
            this.Message = message;
            this.Source = source;
            this.Level = level;
        }

        public override bool Equals(object obj)
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

        public void Deconstruct(out object message, out object source, out LogLevel level)
        {
            message = this.Message;
            source = this.Source;
            level = this.Level;
        }

        public static implicit operator (object message, object source, LogLevel level)(LogMessage value)
        {
            return (value.Message, value.Source, value.Level);
        }

        public static implicit operator LogMessage((object message, object source, LogLevel level) value)
        {
            return new LogMessage(value.message, value.source, value.level);
        }
    }
}
