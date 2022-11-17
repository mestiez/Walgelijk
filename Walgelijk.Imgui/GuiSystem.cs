using System;
using System.Numerics;
using System.Text;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui;

public class GuiSystem : Walgelijk.System
{
    public override void Initialise()
    {
        int length = Enum.GetValues<Button>().Length;
        Gui.Input.ButtonsDown = new bool[length];
        Gui.Input.ButtonsHeld = new (bool, bool)[length];
        Gui.Input.ButtonsUp = new bool[length];

        length = 348;
        Gui.Input.KeysDown = new bool[length];
        Gui.Input.KeysHeld = new (bool, bool)[length];
        Gui.Input.KeysUp = new bool[length];
    }

    private void BeginFrame()
    {
        if (Gui.DebugFlags.HasFlag(DebugFlags.ScrollEater))
        {
            foreach (var item in Gui.Context.Identities)
            {
                var id = item.Value;
                if (id.LocalInputState.HasScrollFocus && (id.ExistedLastFrame || id.Exists))
                {
                    Draw.Reset();
                    Draw.ScreenSpace = true;
                    Draw.Order = RenderOrder.Top;
                    Draw.Colour = Colors.Blue.WithAlpha(0.5f);
                    Draw.OutlineWidth = 0;
                    Draw.Quad(id.DrawBounds.Position, id.DrawBounds.Size);
                }
            }
        }

        if (Gui.Context.Hot != null && !Gui.Context.Hot.Exists)
            Gui.Context.Hot = null;

        if (Gui.Context.Active != null && !Gui.Context.Active.Exists)
            Gui.Context.Active = null;

        Gui.Context.CurrentControlIndex = 0;
        Gui.Context.AbsoluteLayoutCounter = 0;
        Gui.Context.UnscaledTime = Time.SecondsSinceLoadUnscaled;
        ProcessRenderFrameInputState(Gui.Input);

        var dt = TimeSpan.FromSeconds(Time.DeltaTime);
        foreach (var item in Gui.Context.Identities)
        {
            if (item.Value.ExistedLastFrame = item.Value.Exists)
                item.Value.Lifetime += dt;
            else
                item.Value.Lifetime = TimeSpan.Zero;
            item.Value.Exists = false;
            item.Value.WantsToEatScrollInput = false;
        }

        if (Gui.DebugFlags.HasFlag(DebugFlags.DrawBounds))
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = RenderOrder.Top;
            Draw.OutlineWidth = 1;
            Draw.OutlineColour = Colors.Yellow;
            float f = 0;
            foreach (var item in Gui.Context.Identities)
            {
                f += 0.1f;
                var p = Time.SecondsSinceLoadUnscaled;
                p += f;
                p = 1 - (MathF.Abs(p) % 1f);
                Draw.Colour = Colors.Purple.WithAlpha(p * p * p * 0.1f);

                if (item.Value.DrawBounds.Enabled && item.Value.ExistedLastFrame)
                    Draw.Quad((item.Value.DrawBounds.Position), item.Value.DrawBounds.Size);

                if (item.Value.ExistedLastFrame && item.Value.ChildrenExtendOutOfBounds)
                    Draw.Quad((item.Value.InnerContentBounds.BottomLeft), item.Value.InnerContentBounds.GetSize());
            }
        }

        if (Gui.DebugFlags.HasFlag(DebugFlags.RaycastRect))
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = RenderOrder.Top;
            Draw.OutlineWidth = 4;
            Draw.OutlineColour = Colors.Aqua.WithAlpha(Utilities.MapRange(0, 1, 0.6f, 0.9f, 1 - (Time.SecondsSinceLoad * 2) % 1));
            Draw.Colour = Colors.Orange.WithAlpha(0.02f);
            foreach (var item in Gui.Context.Identities)
            {
                if (item.Value.RaycastRectangle.HasValue && item.Value.ExistedLastFrame)
                    Draw.Quad((item.Value.RaycastRectangle.Value.BottomLeft), item.Value.RaycastRectangle.Value.GetSize());
            }
        }

        var windowIdentity = Gui.Context.StartControl(Gui.WindowIdentity);
        windowIdentity.Size = Scene.Game.Window.Size;
        windowIdentity.TopLeft = Vector2.Zero;
        windowIdentity.DrawBounds = new DrawBounds(windowIdentity.Size, windowIdentity.TopLeft, true);
    }

    private void EndFrame()
    {
        Gui.Context.EndControl();
        Gui.Context.RebuildBuffer();
        Gui.Context.PreviouslyHot = Gui.Context.Hot;
        Gui.Context.PreviouslyActive = Gui.Context.Active;

        ControlInputStateResolver.SetIdentities(Gui.Context.GetIdentityBuffer());
        ControlInputStateResolver.UpdateInputStates(Gui.Input);

        foreach (var item in Gui.Context.Identities)
        {
            if (item.Value.Cursor.HasValue && item.Value.Exists)
                Window.CursorStack.SetCursor(item.Value.Cursor.Value, item.Key);
            if (item.Value.ChildCount > 0)
                PositioningUtils.ForceCalculateInnerBounds(Gui.Context, item.Value, out _, out _);
        }

        Gui.Input.TextEntered = string.Empty;

        if (Gui.SoundToPlay != null)
        {
            Audio.PlayOnce(Gui.SoundToPlay, Gui.SoundVolume, 1, Gui.Track);
            Gui.SoundToPlay = null;
        }

        if (Gui.DebugFlags.HasFlag(DebugFlags.RaycastHit))
        {
            var mouseHit = ControlInputStateResolver.Raycast(Gui.Input.WindowMousePos);
            if (mouseHit.HasValue)
            {
                var control = Gui.Context.Identities[mouseHit.Value];

                if (control.RaycastRectangle.HasValue)
                {
                    Draw.Reset();
                    Draw.ScreenSpace = true;
                    Draw.Colour = Colors.Red.WithAlpha(0.05f);
                    Draw.OutlineColour = Colors.Green.WithAlpha(Utilities.MapRange(-1, 1, 0.8f, 1f, MathF.Sin(Time.SecondsSinceSceneChangeUnscaled * 25)));
                    Draw.OutlineWidth = 5;
                    Draw.Order = RenderOrder.Top;
                    Draw.Quad(control.RaycastRectangle.Value.BottomLeft, control.RaycastRectangle.Value.GetSize());
                }
            }
        }

        if (Gui.DebugFlags.HasFlag(DebugFlags.Bounds))
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = RenderOrder.Top;
            Draw.OutlineWidth = 2;
            foreach (var item in Gui.Context.Identities)
            {
                if (!item.Value.Exists || item.Key == Gui.WindowIdentity)
                    continue;

                var rect = new Rect(item.Value.TopLeft.X, item.Value.TopLeft.Y, item.Value.TopLeft.X + item.Value.Size.X, item.Value.TopLeft.Y + item.Value.Size.Y);
                float proximity = Utilities.MapRange(0, 256, 1, 0, (Gui.Input.WindowMousePos - rect.ClosestPoint(Gui.Input.WindowMousePos)).Length());
                proximity = Utilities.Clamp(proximity);
                var c = Hsv.HSVToRGB((MathF.Abs(item.Key) * 0.00001f) % 1, 1, 1);
                Draw.OutlineColour = ((Color)(Vector4.One - c)).WithAlpha(proximity);
                Draw.Colour = c.WithAlpha(0.1f * proximity);

                if (rect.Width * rect.Height > float.Epsilon)
                    Draw.Quad(rect.BottomLeft, rect.GetSize());
            }
        }

        if (Gui.SetCursorStack)
            Window.CursorAppearance = Window.CursorStack.ProcessRequests();
    }

    public override void Update()
    {
        EndFrame();
        CopyUpdateFrameInputState(Input, Gui.Input);
        BeginFrame();
    }

    private static void CopyUpdateFrameInputState(InputState source, UiInputState target)
    {
        target.LastWindowMousePos = target.WindowMousePos;
        target.WindowMousePos = source.WindowMousePosition;
        target.WindowMousePosDelta = target.WindowMousePos - target.LastWindowMousePos;
        target.TextEntered += source.TextEntered;
        target.ScrollDelta = source.MouseScrollDelta;

        for (int i = 0; i < target.ButtonsHeld.Length; i++)
            target.ButtonsHeld[i].current = source.IsButtonHeld((Button)i);

        for (int i = 0; i < target.KeysHeld.Length; i++)
            target.KeysHeld[i].current = source.IsKeyHeld((Key)i);
    }

    private static void ProcessRenderFrameInputState(UiInputState input)
    {
        for (int i = 0; i < input.ButtonsDown.Length; i++)
            input.ButtonsDown[i] = input.ButtonsHeld[i].current && !input.ButtonsHeld[i].prev;

        for (int i = 0; i < input.ButtonsUp.Length; i++)
            input.ButtonsUp[i] = !input.ButtonsHeld[i].current && input.ButtonsHeld[i].prev;

        for (int i = 0; i < input.ButtonsHeld.Length; i++)
            input.ButtonsHeld[i].prev = input.ButtonsHeld[i].current;

        for (int i = 0; i < input.KeysDown.Length; i++)
            input.KeysDown[i] = input.KeysHeld[i].current && !input.KeysHeld[i].prev;

        for (int i = 0; i < input.KeysUp.Length; i++)
            input.KeysUp[i] = !input.KeysHeld[i].current && input.KeysHeld[i].prev;

        for (int i = 0; i < input.KeysHeld.Length; i++)
            input.KeysHeld[i].prev = input.KeysHeld[i].current;
    }
}


internal struct Hsv
{
    /// <summary>
    /// Converts HSV color values to RGB
    /// </summary>
    public static Color HSVToRGB(float hue, float saturation, float value)
    {
        int h, s, v;

        h = (int)MathF.Round(Utilities.Clamp(hue) * 360);
        s = (int)MathF.Round(Utilities.Clamp(saturation) * 100);
        v = (int)MathF.Round(Utilities.Clamp(value) * 100);

        var rgb = new int[3];

        var baseColor = (h + 60) % 360 / 120;
        var shift = (h + 60) % 360 - (120 * baseColor + 60);
        var secondaryColor = (baseColor + (shift >= 0 ? 1 : -1) + 3) % 3;

        //Setting Hue
        rgb[baseColor] = 255;
        rgb[secondaryColor] = (int)((MathF.Abs(shift) / 60.0f) * 255.0f);

        //Setting Saturation
        for (var i = 0; i < 3; i++)
            rgb[i] += (int)((255 - rgb[i]) * ((100 - s) / 100.0f));

        //Setting Value
        for (var i = 0; i < 3; i++)
            rgb[i] -= (int)(rgb[i] * (100 - v) / 100.0f);

        return new Color(
            (byte)rgb[0],
            (byte)rgb[1],
            (byte)rgb[2]);
    }
}