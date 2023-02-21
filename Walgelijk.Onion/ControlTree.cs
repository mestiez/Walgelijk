using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public class ControlTree
{
    public readonly Node Root = new(0, null, null);
    public readonly Dictionary<int, ControlInstance> Instances = new();
    public readonly Dictionary<int, Node> Nodes = new();

    /// <summary>
    /// Amount of seconds that expired nodes will be kept in cache for
    /// </summary>
    public float CacheTimeToLive = 15;
    public Node? CurrentNode;

    private int incrementor;
    private readonly Queue<Node> toDelete = new();

    public ControlTree()
    {
        Nodes.Add(0, Root);
    }

    public ControlInstance CreateInstance(int id)
    {
        var instance = new ControlInstance(id);
        if (!Instances.TryAdd(id, instance))
            throw new Exception("ID was already registered");
        return instance;
    }

    public ControlInstance GetControlInstance(int identity)
    {
        if (Instances.TryGetValue(identity, out var result))
            return result;
        throw new Exception("There is no control instance with id " + identity);
    }

    public ControlInstance EnsureInstance(int id)
    {
        if (Instances.TryGetValue(id, out var instance))
            return instance;
        return CreateInstance(id);
    }

    public (ControlInstance Instance, Node node) Start(int id, IControl? behaviour)
    {
        var inst = EnsureInstance(id);
        if (!Nodes.TryGetValue(id, out var node))
        {
            node = new Node(id, CurrentNode, behaviour);
            node.Alive = true;
            Nodes.Add(id, node);
            behaviour?.OnAdd(new ControlParams(this, Onion.Layout, Game.Main.State, node, inst));
        }

        // attempt to add just in case it came back alive
        CurrentNode?.Children.Add(id);

        CurrentNode = node;
        var p = new ControlParams(this, Onion.Layout, Game.Main.State, node, inst);

        node.SetLayout(Onion.Layout.SelfLayout, Onion.Layout.ChildrenLayout);
        node.ChronologicalPosition = incrementor++;
        node.Alive = true;
        Onion.Layout.Apply(p);

        node.Behaviour?.OnStart(p);

        Onion.Layout.Reset();

        return (inst, node);
    }

    public void End()
    {
        if (CurrentNode == null)
            return;

        CurrentNode.Behaviour?.OnEnd(
            new ControlParams(this, Onion.Layout, Game.Main.State, CurrentNode, EnsureInstance(CurrentNode.Identity))
            );

        CurrentNode = CurrentNode.Parent;
    }

    public void Process(float dt)
    {
        foreach (var node in Nodes.Values)
        {
            node.AliveLastFrame = node.Alive;
            node.Alive = false;

            if (!node.AliveLastFrame && node.SecondsDead > CacheTimeToLive)
                toDelete.Enqueue(node);
        }

        Root.RefreshChildrenList(this, dt);
        Root.Process(new ControlParams(this, Onion.Layout, Game.Main.State, Root, EnsureInstance(Root.Identity)));
        incrementor = 0;

        while (toDelete.TryDequeue(out var node))
            Nodes.Remove(node.Identity);
    }

    public void Render()
    {
        Draw.Reset();
        Draw.ScreenSpace = true;
        Root.Render(new ControlParams(this, Onion.Layout, Game.Main.State, Root, EnsureInstance(Root.Identity)));
    }
}
