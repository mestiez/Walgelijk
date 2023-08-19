using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public class LayoutQueue
{
    /// <summary>
    /// Layout for the next object, escaping out of the parent layout
    /// </summary>
    public readonly Queue<IConstraint> Constraints = new();

    /// <summary>
    /// Constraints applied to every control
    /// </summary>
    public readonly List<IConstraint> DefaultConstraints = new();

    /// <summary>
    /// Layout for the next object's children
    /// </summary>
    public readonly Queue<ILayout> Layouts = new();

    public void Apply(in ControlParams p)
    {
        foreach (var c in DefaultConstraints)
            c.Apply(p);    
        
        while (Constraints.TryDequeue(out var c))
            c.Apply(p);

        //while (Layouts.TryDequeue(out var l))
        //{
        //    int index = 0;
        //    foreach (var item in p.Node.Children)
        //        l.Apply(p, index++, item);
        //}
    }

    /// <summary>
    /// Reset the layout and constraint queues
    /// </summary>
    public void Reset()
    {
        Layouts.Clear();
        Constraints.Clear();
    }

    /// <summary>
    /// Add the given layout to the upcoming control
    /// </summary>
    public LayoutQueue EnqueueLayout<T>(in T l) where T : ILayout
    {
        Layouts.Enqueue(l);
        return this;
    }

    /// <summary>
    /// Add the given constraint to the upcoming control
    /// </summary>
    public LayoutQueue EnqueueConstraint<T>(in T c) where T : IConstraint
    {
        Constraints.Enqueue(c);
        return this;
    }

    /// <summary>
    /// Move the control
    /// </summary>
    public LayoutQueue Move(float x, float y) => EnqueueConstraint(new MoveConstraint(x, y));

    /// <summary>
    /// Move the control in both axes
    /// </summary>
    public LayoutQueue Move(float v) => EnqueueConstraint(new MoveConstraint(v,v));

    /// <summary>
    /// Move the control
    /// </summary>
    public LayoutQueue MoveAbs(float x, float y) => EnqueueConstraint(new MoveAbsoluteConstraint(x, y));

    /// <summary>
    /// Move the control in both axes
    /// </summary>
    public LayoutQueue MoveAbs(float v) => EnqueueConstraint(new MoveAbsoluteConstraint(v,v));

    /// <summary>
    /// Clamp the control position to fit its parent
    /// </summary>
    public LayoutQueue Clamp() => EnqueueConstraint(new ClampToContainer());

    /// <summary>
    /// Set width and height respectively
    /// </summary>
    public LayoutQueue Size(float w, float h)
    {
        EnqueueConstraint(new WidthConstraint(w));
        EnqueueConstraint(new HeightConstraint(h));
        return this;
    }

    /// <summary>
    /// Set width
    /// </summary>
    public LayoutQueue Width(float w) => EnqueueConstraint(new WidthConstraint(w));

    /// <summary>
    /// Set height
    /// </summary>
    public LayoutQueue Height(float h) => EnqueueConstraint(new HeightConstraint(h));

    /// <summary>
    /// Ignore parent scroll
    /// </summary>
    public LayoutQueue IgnoreScroll() => EnqueueConstraint(new IgnoreScroll());

    /// <summary>
    /// Offset the size by adding the given values to the width and height respectively
    /// </summary>
    public LayoutQueue Scale(float w, float h) => EnqueueConstraint(new OffsetSize(w, h));

    /// <summary>
    /// Stick to the left of parent
    /// </summary>
    public LayoutQueue StickLeft() => EnqueueConstraint(new StickLeft());
    /// <summary>
    /// Stick to the right of parent
    /// </summary>
    public LayoutQueue StickRight() => EnqueueConstraint(new StickRight());
    /// <summary>
    /// Stick to the top of parent
    /// </summary>
    public LayoutQueue StickTop() => EnqueueConstraint(new StickTop());
    /// <summary>
    /// Stick to the bottom of parent
    /// </summary>
    public LayoutQueue StickBottom() => EnqueueConstraint(new StickBottom());

    /// <summary>
    /// Make the control resizable by dragging it at the edges
    /// </summary>
    public LayoutQueue Resizable(Vector2 min, Vector2 max) => EnqueueConstraint(new Resizable(min, max));

    /// <summary>
    /// Make the control resizable by dragging it at the right-most and bottom-most edges
    /// </summary>
    public LayoutQueue Resizable() => EnqueueConstraint(new Resizable(new Vector2(64), new Vector2(1024)));

    /// <summary>
    /// Some controls calculate a preferred size. This constraint will apply this size if it is available, otherwise it will do nothing.
    /// </summary>
    public LayoutQueue PreferredSize() => EnqueueConstraint(new PreferredSize());

    /// <summary>
    /// Set the local order within its parent
    /// </summary>
    public LayoutQueue Order(int order) => EnqueueConstraint(new Order(order));

    /// <summary>
    /// Set the maximum width
    /// </summary>
    public LayoutQueue MaxWidth(float maxWidth) => EnqueueConstraint(new MaxSize(maxWidth, null));

    /// <summary>
    /// Set the maximum height
    /// </summary>
    public LayoutQueue MaxHeight(float maxHeight) => EnqueueConstraint(new MaxSize(null, maxHeight));

    /// <summary>
    /// Set the maximum size
    /// </summary>
    public LayoutQueue MaxSize(float maxWidth, float maxHeight) => EnqueueConstraint(new MaxSize(maxWidth, maxHeight));

    /// <summary>
    /// Set the size to fit the size of its children
    /// </summary>
    public LayoutQueue FitContent() => EnqueueConstraint(new FitContent(true, true));

    /// <summary>
    /// Stretch to fit the container.
    /// </summary>
    /// <param name="w">Optional value ranging from 0 to 1 where 1 is 100% of the parent width</param>
    /// <param name="h">Optional value ranging from 0 to 1 where 1 is 100% of the parent height</param>
    public LayoutQueue FitContainer(float? w, float? h, bool pad = true) => EnqueueConstraint(new FitContainer(w, h, pad));

    /// <summary>
    /// Stretch to fit the container.
    /// </summary>
    public LayoutQueue FitContainer() => EnqueueConstraint(new FitContainer(1, 1, true));

    /// <summary>
    /// Stretch to the width of the container
    /// </summary>
    public LayoutQueue FitWidth(bool pad = true) => EnqueueConstraint(new FitContainer(1, null, pad));

    /// <summary>
    /// Stretch to the height of the container
    /// </summary>
    public LayoutQueue FitHeight(bool pad = true) => EnqueueConstraint(new FitContainer(null, 1, pad));

    /// <summary>
    /// Center the control horizontally within its parent
    /// </summary>
    public LayoutQueue CenterHorizontal() => Center(true, false);

    /// <summary>
    /// Center the control vertically within its parent
    /// </summary>
    public LayoutQueue CenterVertical() => Center(false, true);

    /// <summary>
    /// Center the control within its parent
    /// </summary>
    public LayoutQueue Center(bool horizontally = true, bool vertically = true) => EnqueueConstraint(new CenterInParent(horizontally, vertically));

    /// <summary>
    /// Arrange all children horizontally within the control
    /// </summary>
    public LayoutQueue HorizontalLayout() => EnqueueLayout(new HorizontalLayout());

    /// <summary>
    /// Arrange all children vertically within the control
    /// </summary>
    public LayoutQueue VerticalLayout() => EnqueueLayout(new VerticalLayout());
}
