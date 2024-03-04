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
    public readonly SortedSet<int> Children = new(new NodeComparer());

    public IEnumerable<Node> GetChildren()
    {
        foreach (var item in Children)
        {
            if (Onion.Tree.Nodes.TryGetValue(item, out var n))
                yield return n;
        }
    }

    public IEnumerable<Node> GetLivingChildren()
    {
        foreach (var item in Children)
        {
            if (Onion.Tree.Nodes.TryGetValue(item, out var n) && n.AliveLastFrame)
                yield return n;
        }
    }

    public IControl Behaviour;
    public readonly string Name;

    /// <summary>
    /// Desired local order within the parent
    /// </summary>
    public int RequestedLocalOrder;

    /// <summary>
    /// Should this node be considered when calculating the parent scroll bounds?
    /// </summary>
    public bool ContributeToParentScrollRect = true;

    /// <summary>
    /// This control will always be considered to be on top if this is true. 
    /// Used for things like tooltips or dropdown menus.
    /// Note that the behaviour of this is undefined if multiple nodes are always on top.
    /// </summary>
    public bool AlwaysOnTop;

    public int ChronologicalPositionLastFrame;
    public int ChronologicalPosition;
    public int SiblingIndex;
    public bool Alive;
    public bool AliveLastFrame;
    public float SecondsAlive;
    public float SecondsDead;

    /// <summary>
    /// Keeps track of how many times this node has been created this frame. This should always be either 0 or 1.
    /// </summary>
    internal int CreationCount;

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
        var inst = tree.EnsureInstance(Identity);
        var rects = inst.Rects;
        Rect previous;
        if (rects.DrawBounds.HasValue)
        {
            if (!AlwaysOnTop && tree.DrawboundStack.TryPeek(out previous)) // i have a parent with drawbounds!!
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
        if (!AliveLastFrame && p.Node.GetAnimationTime() <= float.Epsilon)
            return;

        var drawBounds = GetFinalDrawBounds(p.Tree).Quantise();
        p.Tree.DrawboundStack.Push(drawBounds);

        Draw.BlendMode = BlendMode.AlphaBlend;
        Draw.ScreenSpace = true;
        Draw.ImageMode = default;
        Draw.Font = p.Theme.Font;
        Draw.FontSize = p.Theme.FontSize[p.Instance.State];
        Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer, p.Node.ComputedGlobalOrder);
        p.Instance.Rects.ComputedDrawBounds = drawBounds;
        p.Instance.Rects.Rendered = p.Instance.Rects.ComputedGlobal;

        foreach (var decorator in p.Instance.Decorators)
            decorator.RenderBefore(p);

        Draw.DrawBounds = new DrawBounds(p.Instance.Rects.ComputedDrawBounds, true);
        if (p.Instance.Rects.ComputedDrawBounds.Width > 0 && p.Instance.Rects.ComputedDrawBounds.Height > 0)
        {
            Behaviour.OnRender(p);
            foreach (var child in GetChildren())
                child.Render(
                    new ControlParams(child, p.Tree.EnsureInstance(child.Identity)));

            ProcessScrollbars(p);
        }

        p.Tree.DrawboundStack.Pop();

        foreach (var decorator in p.Instance.Decorators)
            decorator.RenderAfter(p);
    }

    private static void ProcessScrollbars(ControlParams p)
    {
        /* TODO
         * Horizontal scrollbar
         * Scrollbar proper focus & raycast
         * Dropdown has issues with it
         */
        if (p.Instance.Theme.ShowScrollbars && p.Instance.CaptureFlags.HasFlag(CaptureFlags.Scroll) && p.Instance.Rects.ComputedScrollBounds.Height != 0)
        {
            Draw.ResetTexture();
            Draw.DrawBounds = new DrawBounds(p.Instance.Rects.ComputedDrawBounds, true);
            Draw.ResetTransformation();
            Draw.BlendMode = BlendMode.AlphaBlend;
            Draw.ScreenSpace = true;
            Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer, p.Node.ComputedGlobalOrder);
            Draw.OutlineWidth = 0;

            var scrollbarRect = p.Instance.Rects.ComputedGlobal with { MinX = p.Instance.Rects.ComputedGlobal.MaxX - p.Instance.Theme.ScrollbarWidth };
            var padding = p.Instance.Theme.Padding;
            var trackerRect = scrollbarRect;
            var ratio = scrollbarRect.Height / (scrollbarRect.Height + p.Instance.Rects.ComputedScrollBounds.Height);
            trackerRect.MaxY = trackerRect.MinY + scrollbarRect.Height * ratio;
            trackerRect = trackerRect.Translate(0, -(scrollbarRect.Height - trackerRect.Height) * (p.Instance.InnerScrollOffset.Y / p.Instance.Rects.ComputedScrollBounds.Height));
            Draw.Colour = p.Instance.Theme.Background.Default.Color;

            if (scrollbarRect.ContainsPoint(Onion.Input.MousePosition))
            {
                Game.Main.Window.CursorStack.SetCursor(DefaultCursor.Pointer);
                Draw.Colour = Utilities.Lerp(Draw.Colour, p.Instance.Theme.Accent.Default, 0.2f);
            }
            Draw.Quad(scrollbarRect, 0, p.Instance.Theme.Rounding);
            Draw.Colour = p.Instance.Theme.Accent.Default;
            Draw.Quad(trackerRect.Expand(-padding), 0, p.Instance.Theme.Rounding);

            if (Onion.Input.MousePrimaryHeld && scrollbarRect.ContainsPoint(Onion.Input.MousePosition))
            {
                p.Instance.InnerScrollOffset.Y -= Onion.Input.MouseDelta.Y / ratio;
            }
        }
    }

    public void ApplyParentLayout(in ControlParams p)
    {
        if (!AliveLastFrame)
            return;

        if (Parent != null && Parent.ChildrenLayout != null)
            foreach (var layout in Parent.ChildrenLayout)
                layout.Apply(new ControlParams(Parent, p.Tree.EnsureInstance(Parent.Identity)), SiblingIndex, Identity);

        foreach (var child in GetLivingChildren())
            child.ApplyParentLayout(new ControlParams(child, p.Tree.EnsureInstance(child.Identity)));
    }

    public void Process(in ControlParams p)
    {
        if (AliveLastFrame)
        {
            SecondsAlive += p.GameState.Time.DeltaTimeUnscaled;
            SecondsDead = 0;
        }
        else
        {
            SecondsAlive = 0;
            SecondsDead += p.GameState.Time.DeltaTimeUnscaled;
        }

        //p.Instance.Rects.Intermediate.MaxX *= Onion.GlobalScale;
        //p.Instance.Rects.Intermediate.MaxY *= Onion.GlobalScale;

        QuantiseRects(p);

        if (AliveLastFrame || SecondsDead <= p.Instance.AllowedDeadTime)
        {
            p.Instance.Rects.ComputedGlobal = p.Instance.Rects.Intermediate;
            ControlUtils.ConsiderParentScroll(p);
            if (Parent != null && p.Tree.Instances.TryGetValue(Parent.Identity, out var parentInst))
                p.Instance.Rects.ComputedGlobal = p.Instance.Rects.ComputedGlobal.Translate(parentInst.Rects.ComputedGlobal.BottomLeft).Translate(parentInst.Rects.InnerContentRectAdjustment.XY());

            Behaviour.OnProcess(p);

            AdjustRaycastRect(p);
            EnforceScrollBounds(p);
        }

        foreach (var child in GetChildren())
            child.Process(new ControlParams(child, p.Tree.EnsureInstance(child.Identity)));
    }

    private void QuantiseRects(in ControlParams p)
    {
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Quantise();

        if (p.Instance.Rects.Raycast.HasValue)
            p.Instance.Rects.Raycast = p.Instance.Rects.Raycast.Value.Quantise();
    }

    private static void AdjustRaycastRect(in ControlParams p)
    {
        if (!p.Instance.Rects.Raycast.HasValue)
            return;

        var newRect = p.Instance.Rects.Raycast.Value.Intersect(p.Instance.Rects.ComputedDrawBounds);
        if (newRect.Width <= 0 || newRect.Height <= 0)
            p.Instance.Rects.Raycast = null;
        else
            p.Instance.Rects.Raycast = newRect;
    }

    private static void EnforceScrollBounds(in ControlParams p)
    {
        var childContent = p.Instance.Rects.ComputedChildContent;//.Expand(5);
        bool childrenFitInsideParent = p.Instance.Rects.Intermediate.ContainsRect(childContent);

        if (childrenFitInsideParent)
        {
            p.Instance.InnerScrollOffset = Vector2.Zero;
            p.Instance.Rects.ComputedScrollBounds = default;
        }
        else
        {
            var rects = p.Instance.Rects;
            var scrollHorizontal = p.Instance.OverflowBehaviour.HasFlag(OverflowBehaviour.ScrollHorizontal);
            var scrollVertical = p.Instance.OverflowBehaviour.HasFlag(OverflowBehaviour.ScrollVertical);

            //all we need is the size lol
            var newLocal = rects.Intermediate;
            newLocal.MaxX -= rects.Intermediate.MinX + p.Theme.Padding;
            newLocal.MaxY -= rects.Intermediate.MinY + p.Theme.Padding;
            newLocal.MinX = newLocal.MinY = 0;

            var remainingSpaceLeft =  scrollHorizontal ? MathF.Max(newLocal.MinX - childContent.MinX, 0) : 0;
            var remainingSpaceRight = scrollHorizontal ? MathF.Max(childContent.MaxX - newLocal.MaxX, 0) : 0;

            p.Instance.InnerScrollOffset.X = MathF.Min(p.Instance.InnerScrollOffset.X, remainingSpaceLeft);
            p.Instance.InnerScrollOffset.X = MathF.Max(p.Instance.InnerScrollOffset.X, -remainingSpaceRight);

            var remainingSpaceAbove = scrollVertical ? MathF.Max(newLocal.MinY - childContent.MinY, 0) : 0;
            var remainingSpaceBelow = scrollVertical ? MathF.Max(childContent.MaxY - newLocal.MaxY, 0) : 0;

            p.Instance.InnerScrollOffset.Y = MathF.Min(p.Instance.InnerScrollOffset.Y, remainingSpaceAbove);
            p.Instance.InnerScrollOffset.Y = MathF.Max(p.Instance.InnerScrollOffset.Y, -remainingSpaceBelow);

            p.Instance.Rects.ComputedScrollBounds = new Rect
            {
                MinY = -remainingSpaceBelow,
                MaxY = remainingSpaceAbove,

                MinX = -remainingSpaceRight,
                MaxX = remainingSpaceLeft,
            };
        }
    }

    public void RecursiveDeleteChild(int id)
    {
        if (id == Identity)
            return; // IM GOING TO DIEEEEEEEEEE

        if (!Children.Remove(id))
        {
            foreach (var child in GetChildren())
                child.RecursiveDeleteChild(id);
        }
    }

    public void RefreshChildrenList(ControlTree tree, float dt)
    {
        ChronologicalPosition = -1;
        var inst = tree.EnsureInstance(Identity);
        inst.Rects.ComputedChildContent = inst.Rects.Local;
        inst.Rects.ComputedChildContentSize = Vector2.Zero;

        // remove dead children from the child list
        //var toDelete = ArrayPool<int>.Shared.Rent(Children.Count);
        //var length = 0;
        int siblingIndex = 0;
        foreach (var item in GetChildren())
        {
            var childInst = tree.EnsureInstance(item.Identity);
            if (!item.AliveLastFrame)
            {
                // BYE this is handled by the ControlTree using RecursiveDeleteChild
                //if (childInst.AllowedDeadTime <= item.SecondsDead)
                //    toDelete[length++] = item.Identity;
            }
            else
            {
                //living child should count towards child content rect
                if (item.ContributeToParentScrollRect)
                {
                    inst.Rects.ComputedChildContent = inst.Rects.ComputedChildContent.StretchToContain(childInst.Rects.Intermediate);
                    inst.Rects.ComputedChildContentSize = Vector2.Max(inst.Rects.ComputedChildContentSize, childInst.Rects.ComputedGlobal.TopRight);
                }
            }

            item.SiblingIndex = siblingIndex++;
        }
        inst.Rects.ComputedChildContentSize -= inst.Rects.ComputedGlobal.BottomLeft;
        //for (int i = 0; i < length; i++) // TODO
        //    Children.Remove(toDelete[i]);
        //ArrayPool<int>.Shared.Return(toDelete);

        foreach (var item in GetChildren())
            item.RefreshChildrenList(tree, dt);
    }

    public override string? ToString() => $"{Name} [{(AliveLastFrame ? "Alive" : "Dead")}] (#{ChronologicalPosition})";
}
