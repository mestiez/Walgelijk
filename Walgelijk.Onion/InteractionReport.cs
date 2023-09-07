using System.Numerics;

namespace Walgelijk.Onion;

/// <summary>
/// Can be returned by a control instead of a boolean to provide more precise control over what should be responded to
/// </summary>
public readonly struct InteractionReport
{
    public readonly int Identity;

    public readonly bool Down, Held, Up;
    public readonly ControlState State;
    public readonly Vector2 MousePosition;

    private readonly CastingBehaviour castingBehaviour;
    private readonly bool castCondition;

    public InteractionReport(int identity, bool down, bool held, bool up,
        ControlState state, Vector2 mousePosition, CastingBehaviour castingBehaviour = CastingBehaviour.Down, bool castCondition = true)
    {
        Identity = identity;
        Down = down;
        Held = held;
        Up = up;
        State = state;
        MousePosition = mousePosition;
        this.castingBehaviour = castingBehaviour;
        this.castCondition = castCondition;
    }

    public InteractionReport(ControlInstance inst, Node node, CastingBehaviour castingBehaviour, bool castCondition = true)
    {
        Identity = node.Identity;
        Down = Onion.Input.MousePrimaryPressed && inst.IsActive;
        Held = Onion.Input.MousePrimaryHeld && inst.IsActive;
        Up = Onion.Input.MousePrimaryRelease && inst.IsActive;
        State = inst.State;
        MousePosition = Onion.Input.MousePosition;
        this.castingBehaviour = castingBehaviour;
        this.castCondition = castCondition;
    }

    public ControlInstance Instance => Onion.Tree.EnsureInstance(Identity);
    public Node Node => Onion.Tree.Nodes[Identity];

    public static implicit operator bool(InteractionReport r)
    {
        if (!r.castCondition)
            return false;

        bool v = true;
        v &= !r.castingBehaviour.HasFlag(CastingBehaviour.Down) || r.Down;
        v &= !r.castingBehaviour.HasFlag(CastingBehaviour.Up) || r.Up;
        v &= !r.castingBehaviour.HasFlag(CastingBehaviour.Held) || r.Held;
        return v;
    }

    [Flags]
    public enum CastingBehaviour
    {
        None = 0,
        Down = 1,
        Held = 2,
        Up = 4
    }
}
