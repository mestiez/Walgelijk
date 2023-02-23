using System.Data;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public class ControlTree
{
    public readonly Node Root = new(0, null, new Dummy());
    public readonly Dictionary<int, ControlInstance> Instances = new();
    public readonly Dictionary<int, Node> Nodes = new();

    public readonly Stack<Rect> DrawboundStack = new();

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

    public (ControlInstance Instance, Node node) Start(int id) => Start(id, new Dummy());

    public (ControlInstance Instance, Node node) Start<T>(int id, T behaviour) where T : IControl
    {
        var inst = EnsureInstance(id);
        if (!Nodes.TryGetValue(id, out var node))
        {
            node = new Node(id, CurrentNode, behaviour);
            node.Alive = true;
            Nodes.Add(id, node);
            behaviour?.OnAdd(new ControlParams(node, inst));
            if (CurrentNode != null)
                node.SiblingIndex = CurrentNode.Children.Count;
        }

        if (node.Behaviour is not T)
        {
            node.Behaviour.OnEnd(new ControlParams(node, inst));
            node.Behaviour.OnRemove(new ControlParams(node, inst));
            throw new Exception($"A control with identity {id} has already been assigned a different behaviour ({node.Behaviour})");
        }

        // attempt to add just in case it came back alive
        CurrentNode?.Children.Add(id);
        CurrentNode = node;

        var p = new ControlParams(node, inst);

        node.ChronologicalPosition = incrementor++;
        node.Alive = true;

        node.Behaviour.OnStart(p);
        inst.Rects.Intermediate = inst.Rects.Local;

        if (node.Parent != null && node.Parent.ChildrenLayout != null)
            foreach (var layout in node.Parent.ChildrenLayout)
                layout.Apply(new ControlParams(node.Parent, EnsureInstance(node.Parent.Identity)), node.SiblingIndex, id);

        node.SetLayout(Onion.Layout.Constraints, Onion.Layout.Layouts);
        Onion.Layout.Apply(p);
        Onion.Layout.Reset();

        return (inst, node);
    }

    public void End()
    {
        if (CurrentNode == null)
            return;

        CurrentNode.Behaviour.OnEnd(
            new ControlParams(CurrentNode, EnsureInstance(CurrentNode.Identity))
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
        Root.Process(new ControlParams(Root, EnsureInstance(Root.Identity)));
        incrementor = 0;

        while (toDelete.TryDequeue(out var node))
            Nodes.Remove(node.Identity);
    }

    public void Render()
    {
        Draw.Reset();
        Draw.ScreenSpace = true;
        Root.Render(new ControlParams(Root, EnsureInstance(Root.Identity)));
    }

    public bool IsAlive(int value)
    {
        if (Nodes.TryGetValue(value, out var n))
            return n.AliveLastFrame;
        return false;
    }
}
