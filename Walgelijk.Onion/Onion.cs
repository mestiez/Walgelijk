using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public static class Onion
{
    public static ControlTree Tree = new();
    public static BehaviourCache BehaviourCache = new();
}

public class BehaviourCache : Cache<Type, IControl>
{
    protected override IControl CreateNew(Type t) => Activator.CreateInstance(t) as IControl ?? throw new Exception("Attempt to create a control from a non-control type: " + t);

    protected override void DisposeOf(IControl loaded) { }
}

public class OnionSystem : Walgelijk.System
{
    public bool DebugDrawTree = true;

    public override void Initialise()
    {
    }

    public override void Update()
    {
        Onion.Tree.End();
        Onion.Tree.Process(Time.DeltaTime);
        Onion.Tree.Start(0, null);
    }

    public override void Render()
    {
        Onion.Tree.Render();

        if (DebugDrawTree)
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = RenderOrder.DebugUI;

            var offset = new Vector2(64, 64);
            draw(Onion.Tree.Root);

            void draw(Node node)
            {
                Draw.Colour = Colors.Gray;
                if (node.AliveLastFrame)
                    Draw.Colour = Colors.Yellow;
                if (node.Alive)
                    Draw.Colour = Colors.Green;

                Draw.Text(node.ToString() ?? "[untitled]", offset, Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Bottom);
                offset.X += 32;
                foreach (var item in node.Children)
                {
                    offset.Y += 16;
                    draw(item.Value);
                }
                offset.X -= 32;
            }
        }
    }
}

public class ControlTree
{
    public readonly Node Root = new(0, null, null);
    public readonly Dictionary<int, ControlInstance> InstanceDictionary = new();
    public readonly HashSet<Node> Nodes = new();

    public Node? CurrentParent;

    private int incrementor;
    private Queue<Node> toDelete = new();

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

    public ControlInstance Start(int id, IControl? behaviour)
    {
        var inst = EnsureInstance(id);
        Node? node;
        if (CurrentParent != null)
        {
            if (CurrentParent.Children.TryGetValue(id, out node))
                CurrentParent = node;
            else
            {
                node = new Node(id, CurrentParent, behaviour);
                CurrentParent.Children.Add(id, node);
                CurrentParent = node;
                behaviour?.OnAdd(this, node);
                Nodes.Add(node);
            }
        }
        else
            node = CurrentParent = Root;

        if (node != null)
        {
            node.ChronologicalPosition = incrementor++;
            node.Alive = true;
            node.Behaviour?.OnStart(this, node);
        }

        return inst;
    }

    public void End()
    {
        if (CurrentParent == null)
        {
            return;
            throw new Exception("Attempt to end a null control");
        }

        CurrentParent.Behaviour?.OnEnd(this, CurrentParent);
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
        Root.Process(this, dt);
        incrementor = 0;
    }

    public void Render()
    {
        Root.Render(this);
    }
}

public class Node
{
    public readonly int Identity;
    public readonly Node? Parent;
    public readonly IControl? Behaviour;
    public readonly Dictionary<int, Node> Children = new();
    public readonly string Name;

    public int ChronologicalPosition;
    public bool Alive = false;
    public bool AliveLastFrame = false;
    public float SecondsAlive;
    public float SecondsDead;

    public Node(int id, Node? parent, IControl? behaviour)
    {
        Identity = id;
        Parent = parent;
        Behaviour = behaviour;

        Name = Identity == 0 ? "ROOT" : Identity + $"[{(Behaviour == null ? "Dummy" : Behaviour.GetType().Name)}]";
    }

    public void Render(ControlTree tree)
    {
        Behaviour?.OnRender(tree, this);
        foreach (var item in Children)
            item.Value.Render(tree);
    }

    public void Process(ControlTree tree, float dt)
    {
        if (AliveLastFrame)
        {
            SecondsAlive += dt;
            SecondsDead = 0;
        }
        else
        {
            SecondsAlive = 0;
            SecondsDead += dt;
        }

        Behaviour?.OnProcess(tree, this);

        foreach (var item in Children)
            item.Value.Process(tree, dt);
    }

    public void RefreshChildrenList(ControlTree tree, float dt)
    {
        ChronologicalPosition = -1;

        var toDelete = ArrayPool<int>.Shared.Rent(Children.Count);
        var length = 0;
        foreach (var item in Children)
            if (!item.Value.Alive)
            {
                var inst = tree.EnsureInstance(item.Key);
                if (inst.AllowedDeadTime <= item.Value.SecondsDead)
                    toDelete[length++] = item.Key;
            }
        for (int i = 0; i < length; i++)
            Children.Remove(toDelete[i], out var child);

        ArrayPool<int>.Shared.Return(toDelete);

        foreach (var item in Children)
            item.Value.RefreshChildrenList(tree, dt);
    }

    public override string? ToString() => $"{Name} [{(Alive ? "Alive" : "Dead")}](#{ChronologicalPosition})";
}

public class ControlInstance
{
    public int Identity;
    public Rect ContentRect;
    /// <summary>
    /// Amount of seconds that this control will exist for even when no longer being called (useful for exit animations)
    /// </summary>
    public float AllowedDeadTime = 0.3f;

    public ControlInstance(int id)
    {
        Identity = id;
    }
}

public interface IControl
{
    public void OnAdd(ControlTree tree, Node node);

    public void OnStart(ControlTree tree, Node node);

    public void OnProcess(ControlTree tree, Node node);
    public void OnRender(ControlTree tree, Node node);

    public void OnEnd(ControlTree tree, Node node);

    public void OnRemove(ControlTree tree, Node node);
}

public struct Button : IControl
{
    public static void Click(string name, Vector2 topleft, Vector2 size, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var instance = Onion.Tree.Start(IdGen.Hash(nameof(Button).GetHashCode(), identity, site), new Button());
        {
            instance.ContentRect = new Rect(topleft.X, topleft.Y, topleft.X + size.X, topleft.Y + size.Y);
        }

            //TODO het probleem hiermee is dat als deze control niet meer wordt geroepen, worden zijn kinderen ook niet meer geroepen. zijn kinderen hebben geen idee dat die parent nog wel bestaat... dat mag niet.
            //controls moeten hun kinderen kunnen registreren ofzo of iets weet ik veel misschien een functie net als OnProcess maar dan met OnStructure of iets 
        Onion.Tree.Start(instance.Identity + 4, null);
        Onion.Tree.End();

        Onion.Tree.End();
    }

    public void OnAdd(ControlTree tree, Node node) { }

    public void OnRemove(ControlTree tree, Node node) { }

    public void OnStart(ControlTree tree, Node node)
    {
    }

    public void OnProcess(ControlTree tree, Node node)
    {
    }

    public void OnRender(ControlTree tree, Node node)
    {
        var instance = tree.EnsureInstance(node.Identity);

        var progress = node.Alive ? Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) : 1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        progress = Easings.Cubic.InOut(progress);

        Draw.Colour = Colors.Red.WithAlpha(progress);
        Draw.Quad(instance.ContentRect with { Height = instance.ContentRect.Height * progress });
    }

    public void OnEnd(ControlTree tree, Node node)
    {
    }
}