using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public class Layout
{
    /// <summary>
    /// Layout for the next object, escaping out of the parent layout
    /// </summary>
    public readonly Queue<IConstraint> Constraints = new();

    /// <summary>
    /// Layout for the next object's children
    /// </summary>
    public readonly Queue<ILayout> Layouts = new();

    public void Apply(in ControlParams p)
    {
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
    public Layout EnqueueLayout<T>(in T l) where T : ILayout
    {
        Layouts.Enqueue(l);
        return this;
    }

    /// <summary>
    /// Add the given constraint to the upcoming control
    /// </summary>
    public Layout EnqueueConstraint<T>(in T c) where T : IConstraint
    {
        Constraints.Enqueue(c);
        return this;
    }

    /// <summary>
    /// Move the control
    /// </summary>
    public Layout Move(float x, float y) => EnqueueConstraint(new MoveConstraint(x, y));

    /// <summary>
    /// Clamp the control position to fit its parent
    /// </summary>
    public Layout Clamp() => EnqueueConstraint(new ClampToContainer());

    /// <summary>
    /// Set width and height respectively
    /// </summary>
    public Layout Size(float w, float h)
    {
        EnqueueConstraint(new WidthConstraint(w));
        EnqueueConstraint(new HeightConstraint(h));
        return this;
    }

    /// <summary>
    /// Set width
    /// </summary>
    public Layout Width(float w) => EnqueueConstraint(new WidthConstraint(w));

    /// <summary>
    /// Set height
    /// </summary>
    public Layout Height(float h) => EnqueueConstraint(new HeightConstraint(h));

    /// <summary>
    /// Offset the size by adding the given values to the width and height respectively
    /// </summary>
    public Layout Scale(float w, float h) => EnqueueConstraint(new OffsetSize(w, h));

    /// <summary>
    /// Stick to the left of parent
    /// </summary>
    public Layout StickLeft() => EnqueueConstraint(new StickLeft());
    /// <summary>
    /// Stick to the right of parent
    /// </summary>
    public Layout StickRight() => EnqueueConstraint(new StickRight());
    /// <summary>
    /// Stick to the top of parent
    /// </summary>
    public Layout StickTop() => EnqueueConstraint(new StickTop());
    /// <summary>
    /// Stick to the bottom of parent
    /// </summary>
    public Layout StickBottom() => EnqueueConstraint(new StickBottom());

    /// <summary>
    /// Make the control resizable by dragging it at the edges
    /// </summary>
    public Layout Resizable(Vector2 min, Vector2 max) => EnqueueConstraint(new Resizable(min, max));

    /// <summary>
    /// Make the control resizable by dragging it at the edges
    /// </summary>
    public Layout Resizable() => EnqueueConstraint(new Resizable(new Vector2(64), new Vector2(1024)));

    /// <summary>
    /// Stretch to fit the container.
    /// </summary>
    /// <param name="w">Optional value ranging from 0 to 1 where 1 is 100% of the parent width</param>
    /// <param name="h">Optional value ranging from 0 to 1 where 1 is 100% of the parent height</param>
    public Layout FitContainer(float? w, float? h) => EnqueueConstraint(new FitContainer(w, h));

    /// <summary>
    /// Stretch to fit the container.
    /// </summary>
    public Layout FitContainer() => EnqueueConstraint(new FitContainer(1, 1));

    /// <summary>
    /// Stretch to the width of the container
    /// </summary>
    public Layout FitWidth() => EnqueueConstraint(new FitContainer(1, null));

    /// <summary>
    /// Stretch to the height of the container
    /// </summary>
    public Layout FitHeight() => EnqueueConstraint(new FitContainer(null, 1));

    /// <summary>
    /// Center the control horizontally within its parent
    /// </summary>
    public Layout CenterHorizontal() => Center(true, false);

    /// <summary>
    /// Center the control vertically within its parent
    /// </summary>
    public Layout CenterVertical() => Center(false, true);

    /// <summary>
    /// Center the control within its parent
    /// </summary>
    public Layout Center(bool horizontally = true, bool vertically = true) => EnqueueConstraint(new CenterInParent(horizontally, vertically));

    /// <summary>
    /// Arrange all children horizontally within the control
    /// </summary>
    public Layout HorizontalLayout() => EnqueueLayout(new HorizontalLayout());

    /// <summary>
    /// Arrange all children vertically within the control
    /// </summary>
    public Layout VerticalLayout() => EnqueueLayout(new VerticalLayout());
}
