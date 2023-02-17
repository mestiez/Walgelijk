using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public class LayoutState
{
    /// <summary>
    /// Layout for the next object, escaping out of the parent layout
    /// </summary>
    public readonly Queue<ISingleLayout> SelfLayout = new();

    /// <summary>
    /// Layout for the next object's children
    /// </summary>
    public readonly Queue<IGroupLayout> ChildrenLayout = new();

    public void Apply(in ControlParams p)
    {
        while (ChildrenLayout.TryDequeue(out var layout))
        {
            int index = 0;
            foreach (var item in p.Node.Children)
                layout.Calculate(p, index++);
        }

        while (SelfLayout.TryDequeue(out var layout))
            layout.Calculate(p);
    }

    public void Reset()
    {
        ChildrenLayout.Clear();
        SelfLayout.Clear();
    }

    public void Position(float x, float y, Space space = Space.Relative)
    {
        switch (space)
        {
            case Space.Absolute:
                SelfLayout.Enqueue(new AbsoluteLayout(x, y));
                break;
            case Space.Relative:
                SelfLayout.Enqueue(new RelativeLayout(x, y));
                break;
        }
    }

    public void Size(float w, float h)
    {
        SelfLayout.Enqueue(new AbsoluteLayout(null, new Vector2(w, h)));
    }
}

public enum Space
{
    Absolute,
    Relative,
}
