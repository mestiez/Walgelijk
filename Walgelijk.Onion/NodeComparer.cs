﻿namespace Walgelijk.Onion;

public class NodeComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        if (Onion.Tree.Nodes.TryGetValue(x, out var xNode) && Onion.Tree.Nodes.TryGetValue(y, out var yNode))
        {
            var xOrder = xNode.ChronologicalPosition;
            var yOrder = yNode.ChronologicalPosition;

            return yOrder - xOrder;
        }

        return 0;
    }
}
