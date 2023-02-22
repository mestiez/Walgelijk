using System.Runtime.CompilerServices;
using Walgelijk.Onion.Layout;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public struct Button : IControl
{
    public static void Click(string name, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Button).GetHashCode(), identity, site), new Button());
        {
            node.RequestedLocalOrder = 5;
            //(new RelativeLayout(0, 0) as ISingleLayout).Calculate(ControlParams.CreateFor(node));
            //TODO het probleem hiermee is dat als deze control niet meer wordt geroepen, worden zijn kinderen ook niet meer geroepen. zijn kinderen hebben geen idee dat die parent nog wel bestaat... dat mag niet.
            //controls moeten hun kinderen kunnen registreren ofzo of iets weet ik veel misschien een functie net als OnProcess maar dan met OnStructure of iets 
        }
        Onion.Tree.End();
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.Rendered;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.Rendered;

        this.ConsiderParentScroll();

        //TODO misschien moet hier een utility voor zijn of moet het automatisch gebeuren ofzo... weet ik het

        //if (p.Instance.State.HasFlag(ControlState.Scroll))
        //    p.Instance.InnerScrollOffset += Onion.Input.ScrollDelta; 
        //TODO dit moet ergens anders... 
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, LayoutState layout, GameState state, Node node, ControlInstance instance) = p;

        var animation = node.Alive ? Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) : 1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        animation = Easings.Cubic.InOut(animation);

        instance.Rects.Rendered = instance.Rects.Target.Scale(Utilities.Lerp(animation, 1, 0.6f));

        switch (instance.State)
        {
            case ControlState.None:
                Draw.Colour = Colors.Red;
                break;
            case ControlState.Hover or ControlState.Scroll or (ControlState.Hover | ControlState.Scroll): //TODO dit moet makkelijker kunnen
                Draw.Colour = Colors.Red.Brightness(0.2f);
                break;
            case ControlState.Active:
                Draw.Colour = Colors.Red.Brightness(0.8f);
                break;
        }
        Draw.Colour.A = (animation * animation * animation);
        var r = instance.Rects.Rendered.Translate(instance.InnerScrollOffset);
        Draw.Quad(r);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}