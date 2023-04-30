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
    public Layout Enqueue(in ILayout l)
    {
        Layouts.Enqueue(l);
        return this;
    }

    /// <summary>
    /// Add the given constraint to the upcoming control
    /// </summary>
    public Layout Enqueue(in IConstraint c)
    {
        Constraints.Enqueue(c);
        return this;
    }

    /// <summary>
    /// Move the control
    /// </summary>
    public Layout Move(float x, float y) => Enqueue(new PositionLayout(x, y));

    /// <summary>
    /// Clamp the control position to fit its parent
    /// </summary>
    public Layout Clamp() => Enqueue(new ClampToContainer());

    /// <summary>
    /// Set width and height respectively
    /// </summary>
    public Layout Size(float w, float h)
    {
        Enqueue(new WidthConstraint(w));
        Enqueue(new HeightConstraint(h));
        return this;
    }

    /// <summary>
    /// Set width
    /// </summary>
    public Layout Width(float w) => Enqueue(new WidthConstraint(w));

    /// <summary>
    /// Set height
    /// </summary>
    public Layout Height(float h) => Enqueue(new HeightConstraint(h));

    /// <summary>
    /// Offset the size by adding the given values to the width and height respectively
    /// </summary>
    public Layout Scale(float w, float h) => Enqueue(new OffsetSize(w, h));

    /// <summary>
    /// Stick to the left of parent
    /// </summary>
    public Layout StickLeft() => Enqueue(new StickLeft());
    /// <summary>
    /// Stick to the right of parent
    /// </summary>
    public Layout StickRight() => Enqueue(new StickRight());
    /// <summary>
    /// Stick to the top of parent
    /// </summary>
    public Layout StickTop() => Enqueue(new StickTop());
    /// <summary>
    /// Stick to the bottom of parent
    /// </summary>
    public Layout StickBottom() => Enqueue(new StickBottom());

    /// <summary>
    /// Make the control resizable by dragging it at the edges
    /// </summary>
    public Layout Resizable(Vector2 min, Vector2 max) => Enqueue(new Resizable(min, max));

    /// <summary>
    /// Make the control resizable by dragging it at the edges
    /// </summary>
    public Layout Resizable() => Enqueue(new Resizable(new Vector2(64), new Vector2(1024)));

    /// <summary>
    /// Fit the container.
    /// </summary>
    /// <param name="w">Optional value ranging from 0 to 1 where 1 is 100% of the parent width</param>
    /// <param name="h">Optional value ranging from 0 to 1 where 1 is 100% of the parent height</param>
    public Layout FitContainer(float? w, float? h) => Enqueue(new FitContainer(w, h));

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
    public Layout Center(bool horizontally = true, bool vertically = true) => Enqueue(new CenterInParent(horizontally, vertically));

    /// <summary>
    /// Arrange all children horizontally within the control
    /// </summary>
    public Layout HorizontalLayout() => Enqueue(new HorizontalLayout());

    /// <summary>
    /// Arrange all children vertically within the control
    /// </summary>
    public Layout VerticalLayout() => Enqueue(new VerticalLayout());
}
