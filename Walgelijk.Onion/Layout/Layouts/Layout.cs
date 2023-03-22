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

    public void Reset()
    {
        Layouts.Clear();
        Constraints.Clear();
    }

    public void Enqueue(in ILayout l) => Layouts.Enqueue(l);
    public void Enqueue(in IConstraint c) => Constraints.Enqueue(c);

    public void Offset(float x, float y)
    {
        Enqueue(new PositionLayout(x, y));
    }

    public void Clamp()
    {
        Enqueue(new ClampToContainer());
    }

    public void Size(float w, float h)
    {
        Enqueue(new WidthConstraint(w));
        Enqueue(new HeightConstraint(h));
    }

    public void Width(float w)
    {
        Enqueue(new WidthConstraint(w));
    }

    public void Height(float h)
    {
        Enqueue(new HeightConstraint(h));
    }

    public void OffsetSize(float w, float h)
    {
        Enqueue(new OffsetSize(w, h));
    }

    public void StickLeft() => Enqueue(new StickLeft());
    public void StickRight() => Enqueue(new StickRight());
    public void StickTop() => Enqueue(new StickTop());
    public void StickBottom() => Enqueue(new StickBottom());

    /// <summary>
    /// Fit the container.
    /// </summary>
    /// <param name="w">Optional value ranging from 0 to 1 where 1 is 100% of the parent width</param>
    /// <param name="h">Optional value ranging from 0 to 1 where 1 is 100% of the parent height</param>
    public void FitContainer(float? w, float? h)
    {
        Enqueue(new FitContainer(w, h));
    }

    public void CenterHorizontal() => Center(true, false);

    public void CenterVertical() => Center(false, true);

    public void Center(bool horizontally = true, bool vertically = true)
    {
        Enqueue(new CenterInParent(horizontally, vertically));
    }

    public void HorizontalLayout()
    {
        Enqueue(new HorizontalLayout());
    }

    public void VerticalLayout()
    {
        Enqueue(new VerticalLayout());
    }
}
