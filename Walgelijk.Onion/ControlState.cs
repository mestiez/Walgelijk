namespace Walgelijk.Onion;

public enum ControlState
{
    /// <summary>
    /// Inactive state (idle)
    /// </summary>
    None,
    /// <summary>
    /// The user is probably about to interact with this control
    /// </summary>
    Hot,
    /// <summary>
    /// The user is currently interacting with this control
    /// </summary>
    Active,
}
