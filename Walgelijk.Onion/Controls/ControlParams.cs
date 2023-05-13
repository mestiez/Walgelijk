namespace Walgelijk.Onion.Controls;

public readonly struct ControlParams
{
    public readonly ControlTree Tree;
    public readonly Layout.LayoutQueue Layout;
    public readonly Input Input;
    public readonly GameState GameState;
    public readonly Node Node;
    public readonly ControlInstance Instance;

    public int Identity => Node.Identity;

    public ControlParams(
        ControlTree controlTree,
        Layout.LayoutQueue layoutState,
        Input input,
        GameState gameState,
        Node node,
        ControlInstance instance)
    {
        Tree = controlTree;
        Layout = layoutState;
        Input = input;
        GameState = gameState;
        Node = node;
        Instance = instance;
    }

    public ControlParams(Node node, ControlInstance instance)
    {
        Tree = Onion.Tree;
        Layout = Onion.Layout;
        Input = Onion.Input;
        GameState = Game.Main.State;
        Node = node;
        Instance = instance;
    }

    public void Deconstruct(out ControlTree tree, out Layout.LayoutQueue layout, out Input input, out GameState state, out Node node, out ControlInstance instance)
    {
        tree = Tree;
        layout = Layout;
        input = Input;
        state = GameState;
        node = Node;
        instance = Instance;
    }
}
