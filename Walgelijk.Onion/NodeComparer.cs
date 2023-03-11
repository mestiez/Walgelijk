namespace Walgelijk.Onion;

public class NodeComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        var xOrder = Onion.Tree.Nodes[x].SiblingIndex;
        var yOrder = Onion.Tree.Nodes[y].SiblingIndex;

        return xOrder - yOrder;
    }
}
