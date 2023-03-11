using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct ImageButton : IControl
{
    public readonly IReadableTexture Texture;
    public readonly ImageContainmentMode ContainmentMode;

    public ImageButton(IReadableTexture texture, ImageContainmentMode containmentMode)
    {
        Texture = texture;
        ContainmentMode = containmentMode;
    }

    public static bool Hold(IReadableTexture texture, ImageContainmentMode containmentMode = ImageContainmentMode.Stretch, int identity = 0, [CallerLineNumber] int site = 0)
    {
        return CreateButton(texture, containmentMode, identity, site).HasFlag(ControlState.Active);
    }

    public static bool Click(IReadableTexture texture, ImageContainmentMode containmentMode = ImageContainmentMode.Stretch, int identity = 0, [CallerLineNumber] int site = 0)
    {
        return CreateButton(texture, containmentMode, identity, site).HasFlag(ControlState.Hover) && Onion.Input.MousePrimaryPressed;
    }

    private static ControlState CreateButton(IReadableTexture texture, ImageContainmentMode containmentMode, int identity = 0, int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(ImageButton).GetHashCode(), identity, site), new ImageButton(texture, containmentMode));
        instance.RenderFocusBox = false;
        Onion.Tree.End();
        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
    }

    public void OnProcess(in ControlParams p)
    {
        ControlUtils.ProcessButtonLike(p);
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var animation = node.Alive ?
            Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) :
            1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        animation = Easings.Cubic.InOut(animation);

        instance.Rects.Rendered = instance.Rects.Rendered.Scale(Utilities.Lerp(animation, 1, 0.6f));

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        if (instance.State.HasFlag(ControlState.Hover))
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            Draw.Colour = fg.Color.Brightness(1.2f);
        }
        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = fg.Color.Brightness(0.9f);

        Draw.Colour.A = (animation * animation * animation);
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);

        Draw.ResetMaterial();
        Draw.Colour = Colors.White.WithAlpha(Draw.Colour.A);
        Draw.Image(Texture, instance.Rects.Rendered.Scale(0.9f), ContainmentMode, 0, Onion.Theme.Rounding);
        Draw.ResetTexture();
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
