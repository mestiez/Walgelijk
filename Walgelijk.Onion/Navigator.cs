using System;
using System.Buffers;
using System.Numerics;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public class Navigator
{
    public const int MaxDepth = 7;

    /// <summary>
    /// Control currently capturing the cursor
    /// </summary>
    public int? HoverControl;

    /// <summary>
    /// Control currently capturing the scroll wheel (scroll view, sliders, dropdowns, etc.)
    /// </summary>
    public int? ScrollControl;

    /// <summary>
    /// Control that is currently selected (textboxes, dropdowns, sliders, etc.)
    /// </summary>
    public int? FocusedControl;

    /// <summary>
    /// Control that is actively being used (buttons held, dropdowns open, sliders sliding, etc.)
    /// </summary>
    public int? ActiveControl;

    private readonly Stack<int> orderStack = new();
    private SortedNode[]? sortedByDepth = null;
    private readonly Dictionary<int, float> highlightControls = new();

    public void Process(Input input, float dt)
    {
        ProcessHighlights(dt);

        RefreshOrder();
        HoverControl = Raycast(input.MousePosition.X, input.MousePosition.Y, CaptureFlags.Hover);

        if (input.ScrollDelta.LengthSquared() > float.Epsilon)
            ScrollControl = Raycast(input.MousePosition, CaptureFlags.Scroll);
        else
            ScrollControl = null;

        if (ActiveControl.HasValue)
            if (!Onion.Tree.IsAlive(ActiveControl.Value))
                ActiveControl = null;

        if (FocusedControl.HasValue)
        {
            if (!Onion.Tree.IsAlive(FocusedControl.Value))
                FocusedControl = null;
            else if (HoverControl == null && input.MousePrimaryPressed)
                FocusedControl = null;
            else
            {
                if (input.TabReleased)
                {
                    int targetOrder = Onion.Tree.Nodes[FocusedControl.Value].ChronologicalPositionLastFrame;
                    bool greaterThan = !input.ShiftHeld;
                    Func<Node, bool> predicate = greaterThan ? 
                        (Node n) => n.ChronologicalPositionLastFrame > targetOrder : 
                        (Node n) => n.ChronologicalPositionLastFrame < targetOrder;

                    var found = Onion.Tree.Nodes
                        .Select(static c => c.Value)
                        .Where(static c =>
                        {
                            var n = Onion.Tree.EnsureInstance(c.Identity);
                            return (c.AliveLastFrame &&
                                    n.CaptureFlags.HasFlag(CaptureFlags.Hover) &&
                                    n.Rects.Raycast.HasValue && 
                                    n.Rects.Raycast.Value.Area > float.Epsilon);
                        })
                        .OrderBy(c => greaterThan ? c.ChronologicalPositionLastFrame : -c.ChronologicalPositionLastFrame)
                        .FirstOrDefault(predicate);

                    if (found != null)
                    {
                        FocusedControl = found.Identity;
                        HighlightControl(found.Identity, 0.5f);
                    }
                }
                else if (input.DirectionKeyReleased.LengthSquared() > 0)
                {
                    var hit = FindInDirection(FocusedControl.Value, input.DirectionKeyReleased);
                    if (hit.HasValue)
                        FocusedControl = hit;
                }
            }
        }
    }

    private void ProcessHighlights(float dt)
    {
        int removeCount = 0;
        var removeBuffer = ArrayPool<int>.Shared.Rent(highlightControls.Count);
        foreach (var item in highlightControls.Keys)
        {
            var v = highlightControls[item];
            if (v > 0)
                highlightControls[item] = v - dt;
            else
                removeBuffer[removeCount++] = item;
        }
        foreach (var key in removeBuffer.AsSpan(0, removeCount))
            highlightControls.Remove(key);
        ArrayPool<int>.Shared.Return(removeBuffer);

        Draw.Reset();
        Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer, int.MaxValue);
        Draw.ScreenSpace = true;
        Draw.Colour = Colors.Transparent;
        Draw.OutlineWidth = Onion.Theme.FocusBoxWidth;
        Draw.OutlineColour = Onion.Theme.Highlight;

        foreach (var item in highlightControls)
        {
            var id = item.Key;
            var p = item.Value;
            var inst = Onion.Tree.EnsureInstance(id);

            Draw.OutlineColour.A = Utilities.Clamp(Easings.Expo.Out(p));
            Draw.Quad(inst.Rects.Rendered.Expand(Onion.Theme.FocusBoxSize), 0, Onion.Theme.Rounding + Onion.Theme.FocusBoxSize);
        }
    }

    public void HighlightControl(int id, float duration = 1)
    {
        if (!highlightControls.TryAdd(id, duration))
            highlightControls[id] = duration;
    }

    public void ComputeGlobalOrder()
    {
        orderStack.Clear();
        int counter = 0;
        RecurseNodeOrder(Onion.Tree.Root, ref counter);
    }

    private void RecurseNodeOrder(Node node, ref int counter)
    {
        //je moet global order doen
        int globalOrder;
        if (node.AlwaysOnTop)
            globalOrder = node.ComputedGlobalOrder = (int)MathF.Pow(10, MaxDepth + 1);
        else
            globalOrder = node.ComputedGlobalOrder = GetGlobalDepth(node.RequestedLocalOrder + counter);

        counter++;
        orderStack.Push(globalOrder);

        foreach (var child in node.GetChildren())
            RecurseNodeOrder(child, ref counter);

        orderStack.Pop();
    }

    private int GetGlobalDepth(int depth)
    {
        depth = Math.Max(depth, 0);
        if (orderStack.Count == 0)
            return depth * (int)MathF.Pow(10, MaxDepth);
        return orderStack.Peek() + depth * (int)MathF.Pow(10, MaxDepth - orderStack.Count);
    }

    private void RefreshOrder()
    {
        ComputeGlobalOrder();

        if (sortedByDepth == null)
            sortedByDepth = new SortedNode[Onion.Tree.Nodes.Count];
        else if (sortedByDepth.Length != Onion.Tree.Nodes.Count)
            Array.Resize(ref sortedByDepth, Onion.Tree.Nodes.Count);

        int i = 0;
        foreach (var item in Onion.Tree.Nodes.Values)
            sortedByDepth[i++] = new SortedNode(item.Identity, item.ComputedGlobalOrder);

        Array.Sort(sortedByDepth, static (a, b) => a.Order - b.Order);
    }

    public int? Raycast(Vector2 pos, CaptureFlags captureFlags) => Raycast(pos.X, pos.Y, captureFlags);

    public int? Raycast(float x, float y, CaptureFlags captureFlags)
    {
        if (sortedByDepth == null)
            return null;

        var v = new Vector2(x, y);

        for (int i = sortedByDepth.Length - 1; i >= 0; i--)
        {
            var c = sortedByDepth[i];
            var node = Onion.Tree.Nodes[c.Identity];
            if (!node.AliveLastFrame)
                continue;

            var inst = Onion.Tree.EnsureInstance(c.Identity);
            if (inst.Rects.Raycast.HasValue && (inst.CaptureFlags & captureFlags) != 0)
            {
                if (inst.Rects.Raycast.Value.ContainsPoint(v))
                    return c.Identity;
            }
        }

        return null;
    }

    public int? FindInDirection(int origin, Vector2 direction)
    {
        direction = Vector2.Normalize(direction);
        direction.Y *= -1;

        var originNode = Onion.Tree.Nodes[origin];
        var originInstance = Onion.Tree.EnsureInstance(origin);
        var originPos = originInstance.Rects.ComputedGlobal.GetCenter();
        var originOrder = originNode.ComputedGlobalOrder;

        List<DirectionalNode> found = new();

        foreach (var item in Onion.Tree.Nodes)
        {
            var id = item.Key;

            if (id == origin)
                continue;

            var node = item.Value;
            var inst = Onion.Tree.EnsureInstance(id);

            if (!node.AliveLastFrame ||
                inst.Rects.ComputedDrawBounds.Area <= float.Epsilon ||
                !inst.Rects.Raycast.HasValue ||
                inst.Rects.Raycast.Value.Area <= float.Epsilon
                )
                continue;

            if (!inst.CaptureFlags.HasFlag(CaptureFlags.Hover))
                continue;

            var pos = inst.Rects.ComputedGlobal.GetCenter();
            var dot = Vector2.Dot(pos - originPos, direction);

            if (dot < .5f)
                continue;

            var d = (pos - originPos).LengthSquared();

            found.Add(new DirectionalNode(id, dot, d, Math.Abs(originOrder - node.ComputedGlobalOrder)));
        }

        found.Sort(static (a, b) => (int)(a.Dot * 100000) - (int)(b.Dot * 100000));
        found.Sort(static (a, b) => a.OrderDelta - b.OrderDelta);
        found.Sort(static (a, b) => a.Distance > b.Distance ? 1 : -1);

        int? v = found.Any() ? found.First().Identity : null;

        if (v.HasValue)
            HighlightControl(v.Value, 0.5f);

        return v;
    }

    private readonly struct DirectionalNode
    {
        public readonly int Identity;
        public readonly float Dot;
        public readonly float Distance;
        public readonly int OrderDelta;

        public DirectionalNode(int identity, float dot, float distance, int orderDelta)
        {
            Identity = identity;
            Dot = dot;
            Distance = distance;
            OrderDelta = orderDelta;
        }
    }

    public readonly struct SortedNode
    {
        public readonly int Identity;
        public readonly int Order;

        public SortedNode(int identity, int order)
        {
            Identity = identity;
            Order = order;
        }
    }
}
