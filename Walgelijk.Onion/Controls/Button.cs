using System.Numerics;
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
        Onion.Layout.Apply(p);
    }

    public void OnProcess(in ControlParams p)
    {
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, LayoutState layout, GameState state, Node node, ControlInstance instance) = p;

        var animation = node.Alive ? Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) : 1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        animation = Easings.Cubic.InOut(animation);

        Draw.Colour = Colors.Red.WithAlpha(animation * animation * animation);
        Draw.Quad(instance.TargetRect.Scale(Utilities.Lerp(animation, 1, 0.6f)));
    }

    public void OnEnd(in ControlParams p)
    {
    }
}