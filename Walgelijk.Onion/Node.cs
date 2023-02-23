using System.Buffers;
using System.Numerics;
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
    public readonly SortedSet<int> Children = new();

    public IEnumerable<Node> GetChildren()
    {
        foreach (var item in Children)
            yield return Onion.Tree.Nodes[item];
    }

    public readonly IControl Behaviour;
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

    public IConstraint[]? SelfLayout = null;
    public ILayout[]? ChildrenLayout = null;

    internal void SetLayout(Queue<IConstraint> single, Queue<ILayout> children)
    {
        SelfLayout ??= new IConstraint[single.Count];
        ChildrenLayout ??= new ILayout[children.Count];

        if (SelfLayout.Length != single.Count)
            Array.Resize(ref SelfLayout, single.Count);

        if (ChildrenLayout.Length != children.Count)
            Array.Resize(ref ChildrenLayout, children.Count);

        single.CopyTo(SelfLayout, 0);
        children.CopyTo(ChildrenLayout, 0);
    }

    public Node(int id, Node? parent, IControl behaviour)
    {
        Identity = id;
        Parent = parent;
        Behaviour = behaviour;

        Name = Identity == 0 ?
            "ROOT" :
            Identity + $"[{Behaviour.GetType().Name}]";
    }

    public Rect GetFinalDrawBounds(ControlTree tree)
    {
        var rects = tree.EnsureInstance(Identity).Rects;
        Rect previous;
        if (rects.DrawBounds.HasValue)
        {
            if (tree.DrawboundStack.TryPeek(out previous)) // i have a parent with drawbounds!!
                return rects.DrawBounds.Value.Intersect(previous);
            return rects.DrawBounds.Value;
        }

        if (tree.DrawboundStack.TryPeek(out previous)) // i have no drawbounds assigned so I shouldnt affect the chain
            return previous;

        var size = Game.Main.Window.Size;
        return new Rect(0, 0, size.X, size.Y); //i have no parent so my drawbounds should be as big as the window
        //TODO this is fucked up
    }

    public void Render(in ControlParams p)
    {
        var drawBounds = GetFinalDrawBounds(p.Tree);
        p.Tree.DrawboundStack.Push(drawBounds);

        Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer, p.Node.ComputedGlobalOrder);
        Draw.DrawBounds = new DrawBounds(drawBounds.GetSize(), drawBounds.BottomLeft, true);
        p.Instance.Rects.ComputedDrawBounds = drawBounds;

        p.Instance.Rects.Rendered = p.Instance.Rects.ComputedGlobal;

        Behaviour.OnRender(p);
        foreach (var child in GetChildren())
            child.Render(
                new ControlParams(child, p.Tree.EnsureInstance(child.Identity)));

        p.Tree.DrawboundStack.Pop();
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

        ControlUtils.ConsiderParentScroll(p);

        p.Instance.Rects.ComputedGlobal = p.Instance.Rects.Intermediate;
        if (Parent != null && p.Tree.Instances.TryGetValue(Parent.Identity, out var parentInst))
            p.Instance.Rects.ComputedGlobal = p.Instance.Rects.ComputedGlobal.Translate(parentInst.Rects.ComputedGlobal.BottomLeft);

        Behaviour.OnProcess(p);
        var childContent = p.Instance.Rects.ChildContent;
        bool childrenFitInsideParent =
            childContent.MinX >= 0 && childContent.MaxX <= p.Instance.Rects.Intermediate.MaxX &&
            childContent.MinY >= 0 && childContent.MaxY <= p.Instance.Rects.Intermediate.MaxY;

        if (childrenFitInsideParent)
            p.Instance.InnerScrollOffset = Vector2.Zero;
        else
        {
            var rects = p.Instance.Rects;

            //all we need is the size lol
            var newLocal = rects.Intermediate;
            newLocal.MaxX -= rects.Intermediate.MinX;
            newLocal.MaxY -= rects.Intermediate.MinY;
            newLocal.MinX = newLocal.MinY = 0;

            var remainingSpaceLeft = MathF.Max(newLocal.MinX - rects.ChildContent.MinX, 0);
            var remainingSpaceRight = MathF.Max(rects.ChildContent.MaxX - newLocal.MaxX, 0);
            p.Instance.InnerScrollOffset.X = MathF.Min(p.Instance.InnerScrollOffset.X, remainingSpaceLeft);
            p.Instance.InnerScrollOffset.X = MathF.Max(p.Instance.InnerScrollOffset.X, -remainingSpaceRight);

            var remainingSpaceAbove = MathF.Max(newLocal.MinY - rects.ChildContent.MinY, 0);
            var remainingSpaceBelow = MathF.Max(rects.ChildContent.MaxY - newLocal.MaxY, 0);
            p.Instance.InnerScrollOffset.Y = MathF.Min(p.Instance.InnerScrollOffset.Y, remainingSpaceAbove);
            p.Instance.InnerScrollOffset.Y = MathF.Max(p.Instance.InnerScrollOffset.Y, -remainingSpaceBelow);
        }

        foreach (var child in GetChildren())
            child.Process(
                new ControlParams(child, p.Tree.EnsureInstance(child.Identity)));
    }

    public void RefreshChildrenList(ControlTree tree, float dt)
    {
        ChronologicalPosition = -1;
        var inst = tree.EnsureInstance(Identity);
        inst.Rects.ChildContent = inst.Rects.Local;

        // remove dead children from the child list
        var toDelete = ArrayPool<int>.Shared.Rent(Children.Count);
        var length = 0;
        foreach (var item in GetChildren())
        {
            var childInst = tree.EnsureInstance(item.Identity);
            if (!item.AliveLastFrame)
            {
                if (childInst.AllowedDeadTime <= item.SecondsDead)
                    toDelete[length++] = item.Identity;
            }
            else
            {
                //living child should count towards child content rect
                //  Alle controls moeten relative zijn.
                inst.Rects.ChildContent = inst.Rects.ChildContent.StretchToContain(childInst.Rects.Intermediate);
            }
        }
        for (int i = 0; i < length; i++)
            Children.Remove(toDelete[i]);
        ArrayPool<int>.Shared.Return(toDelete);

        foreach (var item in GetChildren())
            item.RefreshChildrenList(tree, dt);
    }

    public override string? ToString() => $"{Name} [{(AliveLastFrame ? "Alive" : "Dead")}] (#{ChronologicalPosition})";
}
