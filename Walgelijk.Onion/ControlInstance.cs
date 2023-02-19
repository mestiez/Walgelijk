namespace Walgelijk.Onion;

/// <summary>
/// A control instance. Instances represent each instance of the controls that exist in the tree.
/// They are meant to track the non-hierarchical, general state of a control.
/// </summary>
public class ControlInstance
{
    public readonly int Identity;

    /// <summary>
    /// The requested rectangle for this control
    /// </summary>
    public Rect TargetRect;    
    
    /// <summary>
    /// The final rendered rectangle for this control
    /// </summary>
    public Rect FinalRect;

    /// <summary>
    /// Amount of seconds that this control will exist for even when no longer being called 
    /// (useful for exit animations)
    /// </summary>
    public float AllowedDeadTime = 0.3f;

    public ControlInstance(int id)
    {
        Identity = id;
    }
}
