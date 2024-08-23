using Microsoft.Extensions.Logging;

namespace Walgelijk;

/// <summary>
/// A struct you can return that the command processor will interpret
/// </summary>
public struct CommandResult
{
    /// <summary>
    /// Message to return
    /// </summary>
    public string Message;
    /// <summary>
    /// Message type
    /// </summary>
    public LogLevel Type;

    /// <summary>
    /// Create a <see cref="CommandResult"/> of type Info
    /// </summary>
    public static CommandResult Info(string message) => new CommandResult
    {
        Message = message,
        Type = LogLevel.Information
    };

    /// <summary>
    /// Create a <see cref="CommandResult"/> of type Warn
    /// </summary>
    public static CommandResult Warn(string message) => new CommandResult
    {
        Message = message,
        Type = LogLevel.Warning
    };

    /// <summary>
    /// Create a <see cref="CommandResult"/> of type Error
    /// </summary>
    public static CommandResult Error(string message) => new CommandResult
    {
        Message = message,
        Type = LogLevel.Error
    };

    /// <summary>
    /// Implicit string conversion
    /// </summary>
    public static implicit operator CommandResult(string value)
    {
        return Info(value);
    }
}
