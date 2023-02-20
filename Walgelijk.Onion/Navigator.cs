﻿namespace Walgelijk.Onion;

public class Navigator
{
    public const int MaxDepth = 8;

    public int? HotControl;
    public int? FocusedControl;
    public int? ActiveControl;

    private readonly Stack<int> orderStack = new();
    private SortedNode[]? sortedByDepth = null;

    public void Process(in InputState input)
    {
        RefreshOrder();
    }

    public void ComputeGlobalOrder()
    {
        orderStack.Clear();
        int counter = 0;
        mark(Onion.Tree.Root);

        void mark(Node node)
        {
            orderStack.Push(node.ComputedGlobalOrder = GetGlobalDepth(node.RequestedLocalOrder + counter++));

            foreach (var child in node.Children)
                mark(child.Value);

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
        foreach (var item in Onion.Tree.Nodes)
            sortedByDepth[i++] = new SortedNode(item.Identity, item.ComputedGlobalOrder);

        Array.Sort(sortedByDepth, static (a, b) => a.Order - b.Order);
    }

    public int? Raycast(float x, float y)
    {
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