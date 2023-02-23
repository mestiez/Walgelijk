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

    public void Position(float x, float y)
    {
        Constraints.Enqueue(new PositionLayout(x, y));
    }

    public void Size(float w, float h)
    {
        Constraints.Enqueue(new SizeLayout(w, h));
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

    public void HorizontalLayout()
    {
        Layouts.Enqueue(new HorizontalLayout());
    }
}
