using System.Numerics;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public class OnionSystem : Walgelijk.System
{
    public UiDebugOverlay DebugOverlay = UiDebugOverlay.None;

    public override void Initialise()
    {
    }

    public override void Update()
    {
        // end previous frame
        Onion.Tree.End();

        // process
        Onion.Tree.Process(Time.DeltaTime);
        Onion.Input.Update(Input, Time.DeltaTime);
        Onion.Navigator.Process(Onion.Input, Time.DeltaTime);

        // next frame
        Onion.Layout.Offset(0, 0);
        Onion.Layout.Size(Window.Width, Window.Height);
        Onion.Tree.Start(0, new Dummy()); //Root node

        if (Input.IsKeyReleased(Key.F9))
            DebugOverlay = (UiDebugOverlay)(((int)DebugOverlay + 1) % Enum.GetValues<UiDebugOverlay>().Length);

        if (Onion.Configuration.ProcessCursorStack)
            Window.CursorAppearance = Window.CursorStack.ProcessRequests();
    }

    public override void Render()
    {
        Onion.Tree.Render();

        if (DebugOverlay != UiDebugOverlay.None)
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = RenderOrder.DebugUI;

            Draw.Colour = Colors.White.WithAlpha(0.2f);
            Draw.Text(DebugOverlay.ToString(), Window.Size / 2, Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Top);

            switch (DebugOverlay)
            {
                case UiDebugOverlay.CapturedEvents:
                    DrawCaptures();
                    break;
                case UiDebugOverlay.ControlTree:
                    DrawControlTree();
                    break;
                case UiDebugOverlay.LocalRect:
                case UiDebugOverlay.IntermediateRect:
                case UiDebugOverlay.GlobalRect:
                case UiDebugOverlay.RenderedRect:
                case UiDebugOverlay.RaycastRect:
                case UiDebugOverlay.ChildContentRect:
                case UiDebugOverlay.DrawBounds:
                case UiDebugOverlay.ComputedDrawBounds:
                    DrawRects(DebugOverlay);
                    break;
            }
        }
    }

    private void DrawCaptures()
    {
        var p = new Vector2(Window.Width - 32, 32);
        Draw.Colour = Colors.Yellow;
        Draw.Text($"HOVER:  {((Onion.Navigator.HoverControl?.ToString()) ?? "none")}", p, Vector2.One, HorizontalTextAlign.Right, VerticalTextAlign.Top);
        Draw.Text($"SCROLL: {((Onion.Navigator.ScrollControl?.ToString()) ?? "none")}", p += new Vector2(0, 14), Vector2.One, HorizontalTextAlign.Right, VerticalTextAlign.Top);
        Draw.Text($"FOCUS:  {((Onion.Navigator.FocusedControl?.ToString()) ?? "none")}", p += new Vector2(0, 14), Vector2.One, HorizontalTextAlign.Right, VerticalTextAlign.Top);
        Draw.Text($"ACTIVE: {((Onion.Navigator.ActiveControl?.ToString()) ?? "none")}", p += new Vector2(0, 14), Vector2.One, HorizontalTextAlign.Right, VerticalTextAlign.Top);
    }

    private void DrawRects(UiDebugOverlay overlay)
    {
        draw(Onion.Tree.Root);
        void draw(Node node)
        {
            var inst = Onion.Tree.EnsureInstance(node.Identity);

            Draw.OutlineColour = Colors.Purple;
            Draw.OutlineWidth = 4;
            Draw.Colour = Colors.Transparent;

            Rect rect = default;
            switch (overlay)
            {
                case UiDebugOverlay.LocalRect:
                    rect = inst.Rects.Local;
                    break;
                case UiDebugOverlay.IntermediateRect:
                    rect = inst.Rects.Intermediate;
                    break;
                case UiDebugOverlay.GlobalRect:
                    rect = inst.Rects.ComputedGlobal;
                    break;
                case UiDebugOverlay.RenderedRect:
                    rect = inst.Rects.Rendered;
                    break;
                case UiDebugOverlay.RaycastRect:
                    rect = inst.Rects.Raycast ?? default;
                    break;
                case UiDebugOverlay.ChildContentRect:
                    rect = inst.Rects.ChildContent;
                    break;
                case UiDebugOverlay.DrawBounds:
                    rect = inst.Rects.DrawBounds ?? default;
                    break;
                case UiDebugOverlay.ComputedDrawBounds:
                    rect = inst.Rects.ComputedDrawBounds;
                    break;
            }
            Draw.Quad(rect);

            foreach (var item in node.GetChildren())
                draw(item);
        }
    }

    private void DrawControlTree()
    {
        float h = 0.5f;
        var offset = new Vector2(32, 32);
        draw(Onion.Tree.Root);
        void draw(Node node)
        {
            var inst = Onion.Tree.EnsureInstance(node.Identity);
            Draw.Colour = Colors.Gray;
            if (node.AliveLastFrame)
                Draw.Colour = Colors.Yellow;
            if (node.Alive)
                Draw.Colour = Color.FromHsv(h, 0.6f, 1);

            if (Input.IsKeyHeld(Key.Space))
                Draw.Text($"Scroll: {inst.InnerScrollOffset.X}, {inst.InnerScrollOffset.Y}, Y: {inst.Rects.ComputedGlobal.MinY}", offset, Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Bottom);
            else
                Draw.Text((node.ToString() ?? "[untitled]") + " D: " + node.ComputedGlobalOrder, offset, Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Bottom);
            offset.X += 32;
            h += 0.15f;
            foreach (var item in node.GetChildren())
            {
                offset.Y += 16;
                draw(item);
            }
            h -= 0.15f;
            offset.X -= 32;
        }
    }
}
