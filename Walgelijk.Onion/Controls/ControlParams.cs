using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion.Controls;

public readonly struct ControlParams
{
    public readonly ControlTree ControlTree;
    public readonly LayoutState Layout;
    public readonly GameState GameState;
    public readonly Node Node;
    public readonly ControlInstance Instance;

    public ControlParams(
        ControlTree controlTree, 
        LayoutState layoutState, 
        GameState gameState, 
        Node node, 
        ControlInstance instance)
    {
        ControlTree = controlTree;
        Layout = layoutState;
        GameState = gameState;
        Node = node;
        Instance = instance;
    }

    public void Deconstruct(out ControlTree tree, out LayoutState layout, out GameState state, out Node node, out ControlInstance instance)
    {
        tree = ControlTree;
        layout = Layout;
        state = GameState;
        node = Node;
        instance = Instance;
    }

    public static ControlParams CreateFor(Node node) => new ControlParams(Onion.Tree, Onion.Layout, Game.Main.State, node, Onion.Tree.EnsureInstance(node.Identity));
}