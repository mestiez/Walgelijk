using System.Numerics;
using System.Xml.Linq;
using Walgelijk.Onion.Controls;
using Walgelijk.Onion.Layout;
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
    public Node LastNode { get => lastNode ?? Root; set => lastNode = value; }
    public ControlInstance GetLastInstance() => EnsureInstance(LastNode.Identity);

    private int incrementor;
    private readonly Queue<Node> toDelete = new();
    private float focusAnimationProgress = 0;
    private int? lastFocus;
    private Node? lastNode;

    internal int NodesCreatedThisFrame = 0;

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
        NodesCreatedThisFrame++;

        bool alreadyExisted = Instances.ContainsKey(id);
        var inst = EnsureInstance(id);

        inst.IsNew = !alreadyExisted;

        if (!Nodes.TryGetValue(id, out var node))
        {
            node = new Node(id, CurrentNode, behaviour);
            node.Alive = true;
            Nodes.Add(id, node);
            behaviour?.OnAdd(new ControlParams(node, inst));
            if (CurrentNode != null)
                node.SiblingIndex = CurrentNode.Children.Count;
        }
        else
            node.Behaviour = behaviour;

        node.CreationCount++;
        if (node.CreationCount > 1)
            Logger.Error("An Onion control node has been created multiple times in a single frame!");

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

        inst.Rects.InnerContentRectAdjustment = Vector4.Zero;
        if (inst.Rects.ComputedScrollBounds.Height > 0 && p.Instance.Theme.ShowScrollbars && p.Instance.CaptureFlags.HasFlag(CaptureFlags.Scroll))
            inst.Rects.InnerContentRectAdjustment.Z -= p.Theme.ScrollbarWidth;
        node.Behaviour.OnStart(p);
        inst.Rects.Intermediate = inst.Rects.Local;

        node.SetLayout(Onion.Layout.Constraints, Onion.Layout.Layouts);
        Onion.Layout.Apply(p);
        Onion.Layout.Reset();

        Onion.Animation.Process(inst);
        Onion.Decorators.Process(inst);
        Onion.Theme.ApplyTo(inst);

        return (inst, node);
    }

    public void End()
    {
        if (CurrentNode == null)
            return;

        var inst = EnsureInstance(CurrentNode.Identity);
        var p = new ControlParams(CurrentNode, inst);

        lastNode = CurrentNode;

        CurrentNode.Behaviour.OnEnd(p);
        CurrentNode = CurrentNode.Parent;
    }

    public void Process(float dt)
    {
        Onion.Animation.Clear();
        Onion.Decorators.Clear();
        Onion.Layout.Reset();
        Onion.Theme.Reset();

        lastNode = null;
        NodesCreatedThisFrame = 0;

        foreach (var node in Nodes.Values)
        {
            node.ChronologicalPositionLastFrame = node.ChronologicalPosition;
            node.AliveLastFrame = node.Alive;
            node.Alive = false;
            node.CreationCount = 0;

            // TODO MAJOR::: DEleting nodes... causes issues :(
            //if (!node.AliveLastFrame && node.SecondsDead > CacheTimeToLive)
            //    toDelete.Enqueue(node);
        }
        var p = new ControlParams(Root, EnsureInstance(Root.Identity));

        Onion.Layout.Apply(p);
        Onion.Layout.Reset();

        Onion.Animation.Process(p.Instance);
        Onion.Decorators.Process(p.Instance);
        Onion.Theme.ApplyTo(p.Instance);

        Root.ApplyParentLayout(p);
        Root.RefreshChildrenList(this, dt);
        Root.Process(p);

        incrementor = 0;

        while (toDelete.TryDequeue(out var node))
        {
            Root.RecursiveDeleteChild(node.Identity);
            if (!Nodes.Remove(node.Identity))
                Logger.Warn($"Onion: failed to delete {node}");
        }

        focusAnimationProgress = Utilities.Clamp(focusAnimationProgress + dt, 0, 1);

        if (lastFocus != Onion.Navigator.FocusedControl && Onion.Navigator.FocusedControl != null)
            focusAnimationProgress = 0;

        lastFocus = Onion.Navigator.FocusedControl;
    }

    public void Render()
    {
        Draw.Reset();
        Draw.ScreenSpace = true;
        //Draw.Material = Onion.ControlMaterial;

        Root.Render(new ControlParams(Root, EnsureInstance(Root.Identity)));

        var focus = Onion.Navigator.FocusedControl;
        Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer, int.MaxValue);
        if (Game.Main.Window.HasFocus && focus.HasValue && Instances.TryGetValue(focus.Value, out var inst) && inst.RenderFocusBox)
        {
            float expand = Onion.Theme.Base.FocusBoxSize;

            if (inst.Rects.ComputedDrawBounds.Width > 0 && inst.Rects.ComputedDrawBounds.Height > 0)
            {
                var drawBounds = inst.Rects.ComputedDrawBounds.Expand(expand);

                Draw.DrawBounds = new DrawBounds(
                    drawBounds.GetSize(),
                    drawBounds.BottomLeft);

                Draw.Colour = Colors.Transparent;
                Draw.OutlineColour = Onion.Theme.Base.FocusBoxColour.WithAlpha(Utilities.MapRange(1, 0, 0.6f, 1, Easings.Cubic.In(focusAnimationProgress)));
                Draw.OutlineWidth = Onion.Theme.Base.FocusBoxWidth;
                Draw.Quad(inst.Rects.Rendered.Expand(expand), 0, Onion.Theme.Base.Rounding + expand);
                Draw.OutlineWidth = 0;
            }
        }
    }

    public bool IsAlive(int value)
    {
        if (Nodes.TryGetValue(value, out var n))
            return n.AliveLastFrame;
        return false;
    }

    public void Clear()
    {
        CurrentNode = null;
        incrementor = 0;
        DrawboundStack.Clear();
        toDelete.Clear();
        foreach (var item in Nodes)
            item.Value.Children.Clear();
        Nodes.Clear();
        Nodes.Add(0, Root);
        Instances.Clear();
    }
}
