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

    public void Reset()
    {
        Layouts.Clear();
        Constraints.Clear();
    }

    public void Offset(float x, float y)
    {
        Constraints.Enqueue(new PositionLayout(x, y));
    }

    public void Size(float w, float h)
    {
        Constraints.Enqueue(new WidthLayout(w));
        Constraints.Enqueue(new HeightLayout(h));
    }

    public void Width(float w)
    {
        Constraints.Enqueue(new WidthLayout(w));
    }

    public void Height(float h)
    {
        Constraints.Enqueue(new HeightLayout(h));
    }

    /// <summary>
    /// Fit the container.
    /// </summary>
    /// <param name="w">Optional value ranging from 0 to 1 where 1 is 100% of the parent width</param>
    /// <param name="h">Optional value ranging from 0 to 1 where 1 is 100% of the parent height</param>
    public void FitContainer(float? w, float? h)
    {
        Constraints.Enqueue(new FitContainer(w, h));
    }

    public void CenterHorizontal() => Center(false, true);
    public void CenterVertical() => Center(true, false);

    public void Center(bool vertically = true, bool horizontally = true)
    {
        Constraints.Enqueue(new CenterInParent(horizontally, vertically));
    }

    public void HorizontalLayout()
    {
        Layouts.Enqueue(new HorizontalLayout());
    }

    public void VerticalLayout()
    {
        Layouts.Enqueue(new VerticalLayout());
    }
}
