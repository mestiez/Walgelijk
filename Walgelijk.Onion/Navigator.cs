﻿using System.Numerics;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public class Navigator
{
    public const int MaxDepth = 8;

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

    public void Process(Input input)
    {
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
        }
    }

    public void ComputeGlobalOrder()
    {
        orderStack.Clear();
        int counter = 0;
        mark(Onion.Tree.Root);

        void mark(Node node)
        {
            orderStack.Push(node.ComputedGlobalOrder = GetGlobalDepth(node.RequestedLocalOrder + counter++));

            foreach (var child in node.GetChildren())
                mark(child);

            orderStack.Pop();
        }
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
