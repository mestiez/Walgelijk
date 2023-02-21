using System.Buffers;
using Walgelijk.Onion.Controls;
using Walgelijk.Onion.Layout;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

/// <summary>
/// A control tree node. Nodes represent the hierarchical state of a control.
/// </summary>
public class Node
{
    public readonly int Identity;
    public readonly Node? Parent;
    public readonly Dictionary<int, Node> Children = new();

    public readonly IControl? Behaviour;
    public readonly string Name;

    /// <summary>
    /// Desired local order within the parent
    /// </summary>
    public int RequestedLocalOrder;

    public int ChronologicalPosition;
    public bool Alive;
    public bool AliveLastFrame;
    public float SecondsAlive;
    public float SecondsDead;
    /// <summary>
    /// Actual global order
    /// </summary>
    public int ComputedGlobalOrder;

    public ISingleLayout[]? SelfLayout = null;
    public IGroupLayout[]? ChildrenLayout = null;

    internal void SetLayout(Queue<ISingleLayout> single, Queue<IGroupLayout> children)
    {
        SelfLayout ??= new ISingleLayout[single.Count];
        ChildrenLayout ??= new IGroupLayout[children.Count];

        if (SelfLayout.Length != single.Count)
            Array.Resize(ref SelfLayout, single.Count);

        if (ChildrenLayout.Length != children.Count)
            Array.Resize(ref ChildrenLayout, children.Count);

        single.CopyTo(SelfLayout, 0);
        children.CopyTo(ChildrenLayout, 0);
    }

    public Node(int id, Node? parent, IControl? behaviour)
    {
        Identity = id;
        Parent = parent;
        Behaviour = behaviour;

        Name = Identity == 0 ?
            "ROOT" :
            Identity + $"[{(Behaviour == null ? "Dummy" : Behaviour.GetType().Name)}]";
    }

    public void Render(in ControlParams p)
    {
        Draw.Order = new RenderOrder(Onion.RenderLayer, p.Node.ComputedGlobalOrder);
        Behaviour?.OnRender(p);
        foreach (var child in Children)
            child.Value.Render(
                new ControlParams(p.ControlTree, p.Layout, p.GameState, child.Value, p.ControlTree.EnsureInstance(child.Key)));
    }

    public void Process(in ControlParams p)
    {
        if (AliveLastFrame)
        {
            SecondsAlive += p.GameState.Time.DeltaTime;
            SecondsDead = 0;
        }
        else
        {
            SecondsAlive = 0;
            SecondsDead += p.GameState.Time.DeltaTime;
        }

        Behaviour?.OnProcess(p);

        foreach (var child in Children)
            child.Value.Process(
                new ControlParams(p.ControlTree, p.Layout, p.GameState, child.Value, p.ControlTree.EnsureInstance(child.Key)));
    }

    public void RefreshChildrenList(ControlTree tree, float dt)
    {
        ChronologicalPosition = -1;

        // remove dead children from the child list
        var toDelete = ArrayPool<int>.Shared.Rent(Children.Count);
        var length = 0;
        foreach (var item in Children)
            if (!item.Value.Alive)
            {
                var inst = tree.EnsureInstance(item.Key);
                if (inst.AllowedDeadTime <= item.Value.SecondsDead)
                    toDelete[length++] = item.Key;
            }
        for (int i = 0; i < length; i++)
            Children.Remove(toDelete[i], out var child);
        ArrayPool<int>.Shared.Return(toDelete);

        foreach (var item in Children)
            item.Value.RefreshChildrenList(tree, dt);
    }

    public override string? ToString() => $"{Name} [{(Alive ? "Alive" : "Dead")}](#{ChronologicalPosition})";
}
