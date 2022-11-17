using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

#pragma warning disable CA2211 // Non-constant fields should not be visible
namespace Walgelijk.Imgui;

/// <summary>
/// The main GUI function class
/// </summary>
public static class Gui
{
    /// <summary>
    /// The ID of the window (container of all 👁) control
    /// </summary>
    public const int WindowIdentity = 0;

    /// <summary>
    /// The relevant UI context
    /// </summary>
    public static readonly Context Context = new();

    /// <summary>
    /// The base style. Will fall back to the default style if null.
    /// </summary>
    public static Style? BaseStyle;

    /// <summary>
    /// Debug flags. Allows for things like drawing the bounding rectangles of every control.
    /// </summary>
    public static DebugFlags DebugFlags = DebugFlags.None;

    /// <summary>
    /// Track to play the sounds on, if applicable
    /// </summary>
    public static AudioTrack? Track;
    /// <summary>
    /// Sound to play when a control responds to being hovered over
    /// </summary>
    public static Sound? HoverSound;
    /// <summary>
    /// Sound to play when a control responds to being pressed down
    /// </summary>
    public static Sound? MouseDownSound;
    /// <summary>
    /// Sound to play when a control responds to being released
    /// </summary>
    public static Sound? MouseUpSound;

    /// <summary>
    /// Volume from 0 to 1 (inclusive) that the UI audio should be played at
    /// </summary>
    public static float SoundVolume = 1;

    /// <summary>
    /// Set the window cursor to the computed result of the <see cref="Window.CursorStack"/>. This can also be done manually every update.
    /// </summary>
    public static bool SetCursorStack = true;

    internal static Sound? SoundToPlay;

    /// <summary>
    /// Play hover sound
    /// </summary>
    public static void PlayHoverSound() => SoundToPlay = HoverSound;

    /// <summary>
    /// Play mouse down sound
    /// </summary>
    public static void PlayMouseDownSound() => SoundToPlay = MouseDownSound;

    /// <summary>
    /// Play mouse up sound
    /// </summary>
    public static void PlayMouseUpSound() => SoundToPlay = MouseUpSound;

    /// <summary>
    /// The processed input state
    /// </summary>
    public static readonly UiInputState Input = new();

    private static readonly Color fallbackFarBackground = new Color(37, 32, 43);
    private static readonly StyleProperty<Color> fallbackBackgroundColour = (new Color(63, 51, 76), new Color(78, 63, 94), new Color(53, 43, 64));
    private static readonly StyleProperty<Color> fallbackTextColour = new(new Color(0.93f, 0.93f, 0.93f, 1));
    private static readonly StyleProperty<Color> fallbackForegroundColour = new(Colors.White.WithAlpha(0.5f));
    private static readonly StyleProperty<float> fallbackFontSize = 12;
    private static readonly StyleProperty<float> fallbackRoundness = 0;
    private static readonly StyleProperty<float> fallbackPadding = 5;
    private static readonly StyleProperty<float> fallbackOutlineWidth = 0;
    private static readonly StyleProperty<Color> fallbackOutlineColour = Colors.Transparent;

    /// <summary>
    /// The the far background colour for the given style (can be null)
    /// </summary>
    public static Color GetFarBackgroundColour(Style? elementStyle) => elementStyle?.FarBackground ?? BaseStyle?.FarBackground ?? fallbackFarBackground;
    /// <summary>
    /// Get the text colour for the given style (can be null) and state
    /// </summary>
    public static Color GetTextColour(Style? elementStyle, State state) => (elementStyle?.Text ?? BaseStyle?.Text ?? fallbackTextColour).GetFor(state);
    /// <summary>
    /// Get the background colour for the given style (can be null) and state
    /// </summary>
    public static Color GetBackgroundColour(Style? elementStyle, State state) => (elementStyle?.Background ?? BaseStyle?.Background ?? fallbackBackgroundColour).GetFor(state);
    /// <summary>
    /// Get the foreground colour for the given style (can be null) and state
    /// </summary>
    public static Color GetForegroundColour(Style? elementStyle, State state) => (elementStyle?.Foreground ?? BaseStyle?.Foreground ?? fallbackForegroundColour).GetFor(state);
    /// <summary>
    /// Get the outline colour for the given style (can be null) and state
    /// </summary>
    public static Color GetOutlineColour(Style? elementStyle, State state) => (elementStyle?.OutlineColour ?? BaseStyle?.OutlineColour ?? fallbackOutlineColour).GetFor(state);
    /// <summary>
    /// Get the font size for the given style (can be null) and state
    /// </summary>
    public static float GetFontSize(Style? elementStyle, State state) => (elementStyle?.FontSize ?? BaseStyle?.FontSize ?? fallbackFontSize).GetFor(state);
    /// <summary>
    /// Get the roundness for the given style (can be null) and state
    /// </summary>
    public static float GetRoundness(Style? elementStyle, State state) => (elementStyle?.Roundness ?? BaseStyle?.Roundness ?? fallbackRoundness).GetFor(state);
    /// <summary>
    /// Get the padding for the given style (can be null) and state
    /// </summary>
    public static float GetPadding(Style? elementStyle, State state) => (elementStyle?.Padding ?? BaseStyle?.Padding ?? fallbackPadding).GetFor(state);
    /// <summary>
    /// Get the outlinde width for the given style (can be null) and state
    /// </summary>
    public static float GetOutlineWidth(Style? elementStyle, State state) => (elementStyle?.OutlineWidth ?? BaseStyle?.OutlineWidth ?? fallbackOutlineWidth).GetFor(state);
    /// <summary>
    /// Get the font for the given style (can be null)
    /// </summary>
    public static Font GetFont(Style? elementStyle) => elementStyle?.Font ?? BaseStyle?.Font ?? Font.Default;
    /// <summary>
    /// Get the state of the given control ID
    /// </summary>
    public static State GetStateFor(Identity id) =>
        Context.IsHot(id) ? (Context.IsActive(id) ? State.Active : State.Hover) : State.Inactive;

    /// <summary>
    /// Prepare the drawer for drawing a UI element
    /// </summary>
    public static void PrepareDrawer()
    {
        Draw.ResetTexture();
        Draw.ScreenSpace = true;
        Draw.Order = Context.Order;
        Draw.OutlineWidth = 0;
        Draw.BlendMode = null;
    }

    /// <summary>
    /// Draw text in a textbox. This is not a UI control. This is a utility function because controls often need to draw text in a box.
    /// </summary>
    public static void DrawTextInRect(Rect rect, string text, Color color, float fontSize, HorizontalTextAlign halign, VerticalTextAlign valign)
    {
        PrepareDrawer();
        Draw.Colour = color;
        Draw.FontSize = fontSize;
        var rectCenter = rect.GetCenter();
        float textX = halign switch
        {
            HorizontalTextAlign.Left => rect.MinX + 5,
            HorizontalTextAlign.Center => rectCenter.X,
            HorizontalTextAlign.Right => rect.MaxX - 5,
            _ => rect.MinX,
        };
        float textY = valign switch
        {
            VerticalTextAlign.Top => rect.MaxY + 5,
            VerticalTextAlign.Middle => rectCenter.Y,
            VerticalTextAlign.Bottom => rect.MinY - 5,
            _ => rect.MinX,
        };
        Draw.Text(text, new Vector2(textX, textY), Vector2.One, halign, valign, rect.Width);
    }

    /// <summary>
    /// Process a button-like control. This is not a UI control. This is a utility function because controls often act like buttons.
    /// </summary>
    public static void ProcessButtonLike(Identity id, Rect rect, out bool held, out bool pressed, out bool released, bool allowImmediateFocus = false)
    {
        held = false;
        pressed = false;
        released = false;

        if (id.DrawBounds.Enabled)
            rect = PositioningUtils.GetRectIntersection(rect, id.DrawBounds.ToRect());

        PositioningUtils.ApplyRaycastRect(id, rect, false);

        if (Context.IsActive(id))
            held = true;

        if ((allowImmediateFocus ? !Context.IsActive(id) : Context.Active == null) && Context.IsHot(id) && Input.IsButtonPressed(Walgelijk.Button.Left))
        {
            Context.Active = id;
            held = true;
            pressed = true;
            Gui.PlayMouseDownSound();
        }

        if (Context.IsActive(id) && Input.IsButtonReleased(Walgelijk.Button.Left))
        {
            Context.Active = null;
            released = true;
            held = true;
            Gui.PlayMouseUpSound();
        }

        if (ContainsMouse(id, false))
        {
            if (!Context.IsHot(id))
                Gui.PlayHoverSound();
            Context.Hot = id;
        }
        else if (Context.IsHot(id))
            Context.Hot = null;
    }

    /// <summary>
    /// Does the given control raycast rectangle contain the mouse, considering its order?
    /// </summary>
    public static bool ContainsMouse(Identity id, bool includeChildren)
    {
        var mouseHit = ControlInputStateResolver.Raycast(Input.WindowMousePos);
        if (mouseHit.HasValue && Gui.Context.Identities.TryGetValue(mouseHit.Value, out var hit))
        {
            if (id == hit)
                return true;

            if (includeChildren && hit.Parent == id.Raw)
                return true;

            return false;
        }

        return false;
    }

    /// <summary>
    /// Reset the anchors.
    /// </summary>
    public static void ResetAnchor()
    {
        Context.CurrentHorizontalAnchor = null;
        Context.CurrentVerticalAnchor = null;
        Context.CurrentWidthAnchor = null;
        Context.CurrentHeightAnchor = null;
    }

    /// <summary>
    /// Escape the layout. The next immediate control will ignore all layouts and anchors. Its position will be absolute.
    /// </summary>
    public static void EscapeLayout() => Context.AbsoluteLayoutCounter++;

    /// <summary>
    /// Force absolute width for the next immediate control.
    /// </summary>
    public static void ForceAbsoluteWidth() => Context.AbsoluteWidthCounter++;
    /// <summary>
    /// Force absolute height for the next immediate control.
    /// </summary>
    public static void ForceAbsoluteHeight() => Context.AbsoluteHeightCounter++;

    /// <summary>
    /// Anchor the next immediate control to the left side of its container
    /// </summary>
    public static void AnchorLeft() => Context.CurrentHorizontalAnchor = BasicAnchorFunctions.Left;
    /// <summary>
    /// Anchor the next immediate control to the right side of its container
    /// </summary>
    public static void AnchorRight() => Context.CurrentHorizontalAnchor = BasicAnchorFunctions.Right;
    /// <summary>
    /// Anchor the next immediate control to the top side of its container
    /// </summary>
    public static void AnchorTop() => Context.CurrentVerticalAnchor = BasicAnchorFunctions.Top;
    /// <summary>
    /// Anchor the next immediate control to the bottom side of its container
    /// </summary>
    public static void AnchorBottom() => Context.CurrentVerticalAnchor = BasicAnchorFunctions.Bottom;

    /// <summary>
    /// Center the next immediate control horizontally within its container
    /// </summary>
    public static void AnchorHorizontalCenter() => Context.CurrentHorizontalAnchor = BasicAnchorFunctions.HorizontalCenter;
    /// <summary>
    /// Center the next immediate control vertically within its container
    /// </summary>
    public static void AnchorVerticalCenter() => Context.CurrentVerticalAnchor = BasicAnchorFunctions.VerticalCenter;

    /// <summary>
    /// Stretch the next immediate control horizontally within its container
    /// </summary>
    public static void AnchorStretchWidth() => Context.CurrentWidthAnchor = BasicAnchorFunctions.StretchWidth;
    /// <summary>
    /// Stretch the next immediate control vertically within its container
    /// </summary>
    public static void AnchorStretchHeight() => Context.CurrentHeightAnchor = BasicAnchorFunctions.StretchHeight;

    /// <summary>
    /// Horizontally squish next immediate control so that is shrinks if it exceeds the width of its container.
    /// </summary>
    public static void AnchorContainWidth() => Context.CurrentWidthAnchor = BasicAnchorFunctions.ContainWidth;
    /// <summary>
    /// Vertically squish next immediate control so that is shrinks if it exceeds the height of its container.
    /// </summary>
    public static void AnchorContainHeight() => Context.CurrentHeightAnchor = BasicAnchorFunctions.ContainHeight;

    /// <summary>
    /// Absolutely translate the next immediate control.
    /// </summary>
    public static void Translate(Vector2 absoluteTranslation) => Context.AbsoluteTranslation = absoluteTranslation;

    /// <summary>
    /// A spacer control. Just an empty box that renders nothing and only takes up space.
    /// </summary>
    public static void Spacer(float size, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
    {
        Spacer(default, new Vector2(size), style, site, optionalId);
    }

    /// <summary>
    /// A spacer control. Just an empty box that renders nothing and only takes up space.
    /// </summary>
    public static void Spacer(Vector2 topLeft, Vector2 size, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
    {
        var id = Context.StartControl(IdGen.Hash(site, optionalId));
        PositioningUtils.ApplyCurrentLayout(Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyCurrentAnchors(Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyAbsoluteTranslation(Context, id, ref topLeft);
        PositioningUtils.ApplyParentScrollOffset(Context, id, ref topLeft);
        Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Context, id, new DrawBounds(size, topLeft));
        Context.EndControl();
    }

    /// <summary>
    /// Start a horizontal layout control.
    /// </summary>
    public static void StartHorizontalLayout(Vector2 topLeft, Vector2 size, ArrayLayoutMode layout = ArrayLayoutMode.Start, ArrayScaleMode verticalScaling = ArrayScaleMode.None, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0) =>
        Controls.ArrayLayout.HorizontalLayout(topLeft, size, layout, verticalScaling, style, site, optionalId);

    /// <summary>
    /// Start a vertical layout control.
    /// </summary>
    public static void StartVerticalLayout(Vector2 topLeft, Vector2 size, ArrayLayoutMode layout = ArrayLayoutMode.Start, ArrayScaleMode horizontalScaling = ArrayScaleMode.None, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0) =>
        Controls.ArrayLayout.VerticalLayout(topLeft, size, layout, horizontalScaling, style, site, optionalId);

    /// <summary>
    /// End an ongoing horizontal layout control.
    /// </summary>
    public static void StopHorizontalLayout() =>
        Controls.ArrayLayout.StopHorizontalLayout();

    /// <summary>
    /// End an ongoing vertical layout control.
    /// </summary>
    public static void StopVerticalLayout() =>
        Controls.ArrayLayout.StopVerticalLayout();

    /// <summary>
    /// Start a new grid layout
    /// </summary>
    public static void StartGridLayout(Vector2 topLeft, Vector2 size, int columns, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.GridLayout.StartGridLayout(topLeft, size, columns, style, site, optionalId);

    /// <summary>
    /// Stop an ongoing grid layout
    /// </summary>
    public static void StopGridLayout()
        => Controls.GridLayout.StopGridLayout();

    /// <summary>
    /// Start a group of controls.
    /// </summary>
    public static void StartGroup(Vector2 topLeft, Vector2 size, bool drawBackground = false, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0) =>
        Controls.Group.StartGroup(topLeft, size, drawBackground, style, site, optionalId);

    /// <summary>
    /// End an ongoing group.
    /// </summary>
    public static void StopGroup() =>
        Controls.Group.StopGroup();

    /// <summary>
    /// An image control. Simply draws an image.
    /// </summary>
    public static void Image(Vector2 topLeft, Vector2 size, IReadableTexture texture, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Image.BasicImage(topLeft, size, texture, style, site, optionalId);

    /// <summary>
    /// An image control that makes the image fit inside a box and allows for an optional background image.
    /// </summary>
    public static void ImageBox(Vector2 topLeft, Vector2 size, IReadableTexture? texture, IReadableTexture? background, ImageContainmentMode containmentMode, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Image.ImageBox(topLeft, size, texture, background, containmentMode, style, site, optionalId);

    /// <summary>
    /// An image button
    /// </summary>
    public static bool ImageButton(Vector2 topLeft, Vector2 size, IReadableTexture? texture, IReadableTexture? background, ImageContainmentMode containmentMode, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Image.ImageButton(topLeft, size, texture, background, containmentMode, style, site, optionalId);

    /// <summary>
    /// A label control. This control is usually preceded by a <see cref="AnchorStretchWidth"/> anchor but it's not necessary.
    /// </summary>
    public static void Label(string text, Vector2 topLeft, HorizontalTextAlign halign = HorizontalTextAlign.Left, VerticalTextAlign valign = VerticalTextAlign.Top, float wrapWidth = float.PositiveInfinity, float? height = null, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Label.Process(text, topLeft, halign, valign, wrapWidth, height, style, site, optionalId);

    /// <summary>
    /// Text box control. Width has to be specified, but height can be left at null to fall back to automatic scaling.
    /// </summary>
    public static void TextBox(string text, Vector2 topLeft, float width, float? height = null, bool drawBackground = false, HorizontalTextAlign halign = HorizontalTextAlign.Left, VerticalTextAlign valign = VerticalTextAlign.Top, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.TextBox.Process(text, topLeft, width, height, drawBackground, halign, valign, style, site, optionalId);

    /// <summary>
    /// Basic button control. Returns true while the button is held.
    /// </summary>
    /// <returns></returns>
    public static bool Button(string text, Vector2 topLeft, Vector2 size, out bool wasPressed, out bool wasReleased, HorizontalTextAlign halign = HorizontalTextAlign.Center, VerticalTextAlign valign = VerticalTextAlign.Middle, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Button.Process(text, topLeft, size, out wasPressed, out wasReleased, halign, valign, style, site, optionalId);

    /// <summary>
    /// Click button control. Identical to a normal button, but only returns true when the button is released.
    /// </summary>
    public static bool ClickButton(string text, Vector2 topLeft, Vector2 size, HorizontalTextAlign halign = HorizontalTextAlign.Center, VerticalTextAlign valign = VerticalTextAlign.Middle, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
    {
        if (Button(text, topLeft, size, out var _, out var wasReleased, halign, valign, style, site, optionalId))
            return wasReleased;
        return false;
    }

    /// <summary>
    /// A dropdown control. Returns true when the control is interacted with.
    /// </summary>
    public static bool Dropdown(Vector2 topLeft, Vector2 size, IEnumerable<string> options, ref int selectedIndex, int maxItemsBeforeScrolling = 5, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Dropdown.Process(topLeft, size, options, ref selectedIndex, maxItemsBeforeScrolling, style, site, optionalId);

    /// <summary>
    /// An enum dropdown control. Returns true when the control is interacted with.
    /// </summary>
    public static bool DropdownEnum<T>(Vector2 topLeft, Vector2 size, ref T selected, int maxItemsBeforeScrolling = 5, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0) where T : notnull, global::System.Enum
        => Controls.Dropdown.ProcessEnumDropdown(topLeft, size, ref selected, maxItemsBeforeScrolling, style, site, optionalId);
    
    /// <summary>
    /// A dropdown control that works with a string-value collections. Returns true when the control is interacted with.
    /// </summary>
    public static bool Dropdown<T>(Vector2 topLeft, Vector2 size, (string, T)[] options, ref T? selected, int maxItemsBeforeScrolling = 5, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0) where T:notnull
        => Controls.Dropdown.Process(topLeft, size, options, ref selected, maxItemsBeforeScrolling, style, site, optionalId);

    /// <summary>
    /// A checkbox control. Returns true when the control is interacted with.
    /// </summary>
    public static bool Checkbox(string label, Vector2 topLeft, Vector2 size, ref bool value, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Checkbox.Process(label, topLeft, size, ref value, style, site, optionalId);

    /// <summary>
    /// A horizontal value slider. Returns true when the control is interacted with.
    /// </summary>
    public static bool SliderHorizontal(string format, Vector2 topLeft, Vector2 size, ref float value, float min, float max, int decimals = 2, float step = float.NaN, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0, Func<float, float>? displayTransformer = null)
        => Controls.Slider.ProcessHorizontal(format, topLeft, size, ref value, min, max, decimals, step, displayTransformer, style, site, optionalId);

    /// <summary>
    /// A vertical value slider. Returns true when the control is interacted with.
    /// </summary>
    public static bool SliderVertical(string format, Vector2 topLeft, Vector2 size, ref float value, float min, float max, int decimals = 2, float step = float.NaN, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0, Func<float, float>? displayTransformer = null)
        => Controls.Slider.ProcessVertical(format, topLeft, size, ref value, min, max, decimals, step, displayTransformer, style, site, optionalId);

    /// <summary>
    /// Float text input
    /// </summary>
    public static bool DecimalInput(ref float value, Vector2 topLeft, Vector2 size, string? placeholder = null, int maxChar = 9, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.TextInput.DecimalInput(ref value, topLeft, size, placeholder, maxChar, style, site, optionalId);

    /// <summary>
    /// Integer horizontal
    /// </summary>
    public static bool SliderHorizontal(string format, Vector2 topLeft, Vector2 size, ref int value, int min, int max, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Slider.IntegerSlider(format, topLeft, size, ref value, min, max, style, site, optionalId);

    //TODO multiline
    /// <summary>
    /// A basic text input control. Returns true when the control is interacted with.
    /// </summary>
    public static bool TextInput(Vector2 topLeft, Vector2 size, ref string value, TextInputMode mode = TextInputMode.All, bool multiline = false, string? placeholder = null, int maxChar = int.MaxValue, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.TextInput.Process(topLeft, size, ref value, mode, multiline, placeholder, maxChar, style, site, optionalId);

    /// <summary>
    /// A progress bar control. Not interactable. It just shows a progress bar and optionally draws a percentage label.
    /// </summary>
    public static void ProgressBar(Vector2 topLeft, Vector2 size, float progress0to1, bool showPercentage = false, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.ProgressBar.Process(topLeft, size, progress0to1, showPercentage, style, site, optionalId);

    /// <summary>
    /// A colourpicker control.
    /// </summary>
    public static bool Colourpicker(Vector2 topLeft, Vector2 size, bool editableAlpha, ref Color color, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        => Controls.Colourpicker.Process(topLeft, size, editableAlpha, ref color, style, site, optionalId);
}
#pragma warning restore CA2211 // Non-constant fields should not be visible
