namespace Walgelijk.Onion;

public class ControlTree
{
    public Node Root;
}

public readonly struct ControlInstanceDict
{
    public static readonly Dictionary<int, ControlInstance> Instances = new();

    public ControlInstance Ensure(int instance) 
    {
        if (Instances.TryGetValue)
    }
}

public class Node
{
    public readonly ControlInstance Instance;
    public readonly Node? Parent;
    public readonly IControl Behaviour;
    public readonly List<Node> Children = new();

    public Node(int id, Node? parent, IControl behaviour)
    {
        Parent = parent;
        Behaviour = behaviour;
        Instance = new ControlInstance(id);
    }
}

public class ControlInstance
{
    public int Identity;
    public float SecondsSinceCreation;

    public ControlInstance(int id)
    {
        Identity = id;
    }
}

public interface IControl
{
    public void OnAdd(ControlTree tree, Node node);

    public void OnEnter(ControlTree tree, Node node);

    public void OnProcess(ControlTree tree, Node node);
    public void OnRender(ControlTree tree, Node node);

    public void OnExit(ControlTree tree, Node node);

    public void OnRemove(ControlTree tree, Node node);
}