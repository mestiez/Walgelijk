using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion;

public class ControlTree
{
    public readonly Node Root = new(0, null, null);
    public readonly Dictionary<int, ControlInstance> InstanceDictionary = new();
    public readonly HashSet<Node> Nodes = new();

    public Node? CurrentParent;

    private int incrementor;
    private readonly Queue<Node> toDelete = new();

    public ControlInstance CreateInstance(int id)
    {
        var instance = new ControlInstance(id);
        if (!InstanceDictionary.TryAdd(id, instance))
            throw new Exception("ID was already registered");
        return instance;
    }

    public ControlInstance GetControlInstance(int identity)
    {
        if (InstanceDictionary.TryGetValue(identity, out var result))
            return result;
        throw new Exception("There is no control instance with id " + identity);
    }

    public ControlInstance EnsureInstance(int id)
    {
        if (InstanceDictionary.TryGetValue(id, out var instance))
            return instance;
        return CreateInstance(id);
    }

    public (ControlInstance Instance, Node node) Start(int id, IControl? behaviour)
    {
        var inst = EnsureInstance(id);
        Node node;
        if (CurrentParent != null)
        {
            if (CurrentParent.Children.TryGetValue(id, out var n))
            {
                node = n;
                CurrentParent = node;
            }
            else
            {
                node = new Node(id, CurrentParent, behaviour);
                CurrentParent.Children.Add(id, node);
                CurrentParent = node;
                behaviour?.OnAdd(new ControlParams(this, Onion.Layout, Game.Main.State, node, inst));
                Nodes.Add(node);
            }
        }
        else
            CurrentParent = node = Root;

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
        if (CurrentParent == null)
            return;

        CurrentParent.Behaviour?.OnEnd(
            new ControlParams(this, Onion.Layout, Game.Main.State, CurrentParent, EnsureInstance(CurrentParent.Identity))
            );

        CurrentParent = CurrentParent.Parent;
    }

    public void Process(float dt)
    {
        foreach (var item in Nodes)
            if (!item.Alive)
                toDelete.Enqueue(item);

        while (toDelete.TryDequeue(out var n))
        {
            n.AliveLastFrame = false;
            // Nodes.Remove(n);
        }

        foreach (var item in Nodes)
        {
            item.AliveLastFrame = item.Alive;
            item.Alive = false;
        }

        Root.RefreshChildrenList(this, dt);
        Root.Process(new ControlParams(this, Onion.Layout, Game.Main.State, Root, EnsureInstance(Root.Identity)));
        incrementor = 0;
    }

    public void Render()
    {
        Root.Render(new ControlParams(this, Onion.Layout, Game.Main.State, Root, EnsureInstance(Root.Identity)));
    }
}
