using System;
using System.Linq;
using System.Numerics;

namespace Walgelijk;

public class DebugConsoleUi : IDisposable
{
    public const int MaxLineSize = 256;
    public readonly ActionRenderTask RenderTask;

    public int Padding = 8;
    public int Height = 350;
    public int FilterWidth = 128;
    public int InputHeight = 32;
    public int LineHeight = 16;
    public float AnimationDuration = 0.2f;
    public float BackgroundIntensity = 1;

    public float EffectiveHeight => float.Min(Height, debugConsole.Game.Window.Height);

    public Color BackgroundColour = new(25, 25, 25, 250);
    public Color InputBoxColour = new(15, 15, 15);
    public Color InputTextColour = new(240, 240, 240);
    public Color FilterBoxColour = new(35, 35, 35);
    public Color CaretColour = new(250, 250, 250);
    public IReadableTexture? BackgroundTexture = DebugConsoleAssets.DefaultBg;

    public int MaxLineCount => ((int)EffectiveHeight - InputHeight - Padding) / LineHeight;
    public int VisibleLineCount { get; private set; }

    public float CaretBlinkTime = 0;

    private readonly DebugConsole debugConsole;
    private readonly Material mat;
    private readonly VertexBuffer textMesh;
    private readonly TextMeshGenerator textMeshGenerator = new();
    private float animationProgress = 0;
    private int dropdownOffset;
    private Color flashColour;
    private float flashTime;

    private Rect backgroundRect;
    private Rect inputBoxRect;
    private Rect filterBoxRect;

    private readonly Line[] lines = new Line[1024];
    private readonly FilterIcon[] filterButtons;
    private int? resizeDragOffset;

    private class Line
    {
        public readonly ClampedArray<char> Buffer = new(MaxLineSize);
        public Color Color = Colors.Magenta;
        public ConsoleMessageType Type = ConsoleMessageType.Plain;
        public bool Complete = true;
    }

    private struct FilterIcon
    {
        public Rect Rect;
        public readonly Color Color;
        public readonly IReadableTexture Texture;
        public readonly ConsoleMessageType MessageType;

        public bool Hover;

        public FilterIcon(IReadableTexture texture, ConsoleMessageType messageType, Color color)
        {
            Rect = default;
            Texture = texture;
            MessageType = messageType;
            Color = color;
            Hover = false;
        }
    }

    public DebugConsoleUi(DebugConsole debugConsole)
    {
        this.debugConsole = debugConsole;
        const int maxTextMeshVerts = 2048;

        RenderTask = new ActionRenderTask(Render);

        textMesh = new VertexBuffer(new Vertex[maxTextMeshVerts], new uint[(int)(maxTextMeshVerts * 1.5f)])
        {
            Dynamic = true,
            AmountOfIndicesToRender = 0,
        };

        mat = new Material(new Shader(ShaderDefaults.WorldSpaceVertex, DebugConsoleAssets.FragmentShader));
        mat.SetUniform("tint", Colors.Magenta);
        mat.SetUniform("mainTex", Texture.White);
        mat.SetUniform("bgTex", Texture.White);

        filterButtons =
        [
            new FilterIcon(DebugConsoleAssets.InfoIcon, ConsoleMessageType.Info, new Color(240, 240, 240) ),
            new FilterIcon(DebugConsoleAssets.AlertIcon, ConsoleMessageType.Error, new Color(1, 0.25f, 0.1f) ),
            new FilterIcon(DebugConsoleAssets.AlertIcon, ConsoleMessageType.Warning, new Color(1, 0.7f, 0.2f) ),
            new FilterIcon(DebugConsoleAssets.DebugIcon, ConsoleMessageType.Debug, new Color(0.8f, 0.2f, 1) ),
        ];

        for (int i = 0; i < lines.Length; i++)
            lines[i] = new Line();
    }

    public void Flash(Color colour)
    {
        flashTime = 1;
        flashColour = colour;
    }

    public void Dispose()
    {
        mat.Dispose();
    }

    public void Update(Game game)
    {
        var input = game.State.Input;
        var width = game.Window.Width;
        var dt = game.State.Time.DeltaTimeUnscaled / AnimationDuration;

        if (!debugConsole.IsActive)
            dt *= -1;

        ParseLines();

        flashTime = Utilities.Clamp(flashTime - game.State.Time.DeltaTimeUnscaled * 1.5f);

        animationProgress = Utilities.Clamp(animationProgress + dt);
        dropdownOffset = (int)(animationProgress * EffectiveHeight - EffectiveHeight);
        mat.SetUniform("time", game.State.Time.SecondsSinceLoadUnscaled * 0.04f);
        var bottom = EffectiveHeight + dropdownOffset;

        backgroundRect = new Rect(0, 0, width, EffectiveHeight).Translate(0, dropdownOffset);
        inputBoxRect = new Rect(0, -InputHeight, width - FilterWidth, 0).Translate(0, EffectiveHeight + dropdownOffset);

        const float iconSize = 24;
        var filtersPos = new Vector2(width - FilterWidth, bottom - InputHeight / 2);
        var iconRect = new Rect(-iconSize / 2, -iconSize / 2, iconSize / 2, iconSize / 2).Translate(filtersPos);
        float padding = filterBoxRect.Height - iconSize;

        filterBoxRect = new Rect(filtersPos.X, filtersPos.Y - InputHeight / 2, filtersPos.X + FilterWidth, filtersPos.Y + InputHeight / 2);

        for (int i = 0; i < filterButtons.Length; i++)
        {
            float x = Utilities.MapRange(0, 1, padding, filterBoxRect.Width - iconSize - padding, i / ((float)filterButtons.Length - 1));
            var r = filterButtons[i].Rect = iconRect.Translate(x + 12, 0);
            var hover = filterButtons[i].Hover = r.ContainsPoint(input.WindowMousePosition);
            if (input.IsButtonPressed(MouseButton.Left) && hover)
                debugConsole.Filter ^= filterButtons[i].MessageType;
        }

        if (debugConsole.IsActive)
        {
            if (float.Abs(input.WindowMousePosition.Y - filterBoxRect.MaxY) < 8)
            {
                game.Window.CursorStack.SetCursor(DefaultCursor.VerticalResize);

                if (!resizeDragOffset.HasValue && input.IsButtonPressed(MouseButton.Left))
                    resizeDragOffset = (Height - (int)input.WindowMousePosition.Y);
            }

            if (resizeDragOffset.HasValue)
            {
                game.Window.CursorStack.SetCursor(DefaultCursor.VerticalResize);

                Height = (int)Utilities.Snap(resizeDragOffset.Value + input.WindowMousePosition.Y, LineHeight);
                Height = int.Clamp(Height, 60, game.Window.Height);

                if (!input.IsButtonHeld(MouseButton.Left))
                    resizeDragOffset = null;
            }

            if (filterBoxRect.ContainsPoint(input.WindowMousePosition) &&
                filterButtons.Any(r => r.Rect.ContainsPoint(input.WindowMousePosition)))
                game.Window.CursorStack.SetCursor(DefaultCursor.Pointer);

            if (inputBoxRect.ContainsPoint(input.WindowMousePosition))
                game.Window.CursorStack.SetCursor(DefaultCursor.Text);

            if (resizeDragOffset.HasValue)
                game.Window.CursorStack.SetCursor(DefaultCursor.VerticalResize);
        }
        else
            resizeDragOffset = null;

        if (debugConsole.IsActive)
            game.Window.CursorAppearance = game.Window.CursorStack.ProcessRequests();
    }

    public void Render(IGraphics graphics)
    {
        if (animationProgress < float.Epsilon)
            return;

        var target = graphics.CurrentTarget;
        var op = target.ProjectionMatrix;
        var ov = target.ViewMatrix;
        target.ProjectionMatrix = target.OrthographicMatrix;
        target.ViewMatrix = Matrix4x4.Identity;
        graphics.Stencil = default;
        graphics.DrawBounds = DrawBounds.DisabledBounds;
        {
            graphics.DrawBounds = new DrawBounds(new Vector2(target.Size.X, EffectiveHeight), Vector2.Zero);

            DrawPanels(graphics);
            DrawFilterButtons(graphics);
            DrawConsoleBuffer(graphics);
            DrawInputText(graphics);

            // draw flash
            if (flashTime > float.Epsilon)
            {
                graphics.DrawBounds = new DrawBounds(inputBoxRect);
                mat.SetUniform("mainTex", Texture.White);
                mat.SetUniform("bgIntensity", 0f);
                DrawQuad(graphics, inputBoxRect, flashColour * Easings.Quad.In(flashTime));
            }

            graphics.DrawBounds = DrawBounds.DisabledBounds;
        }
        target.ProjectionMatrix = op;
        target.ViewMatrix = ov;
    }

    public void ParseLines()
    {
        foreach (var item in lines)
            item.Buffer.Clear();

        VisibleLineCount = 0;
        int lineIndex = 0;
        var b = debugConsole.GetBuffer();
        int width = 0;
        var charWidth = (int)textMeshGenerator.Font.Glyphs['x'].Advance;

        for (int i = 0; i < b.Length; i++)
        {
            var c = (char)b[i];
            var line = lines[lineIndex];
            bool shouldEndLine = false;

            if (i == b.Length - 1)
            {
                line.Complete = true;
                shouldEndLine = true;
            }
            else
            {
                switch (c)
                {
                    case '\n':
                        shouldEndLine = true;
                        line.Complete = true;
                        break;
                    default:
                        if (!char.IsControl(c))
                        {
                            if (width + charWidth > backgroundRect.Width - Padding * 3 && debugConsole.PassesFilter(debugConsole.DetectMessageType(line.Buffer.AsSpan().Trim())))
                            {
                                line.Complete = false;
                                shouldEndLine = true;
                                if (width > charWidth)
                                    i--;
                            }
                            else if (line.Buffer.Count < line.Buffer.Capacity)
                            {
                                width += charWidth;
                                line.Buffer.Add(c);
                            }
                        }
                        break;
                }
            }

            if (shouldEndLine)
            {
                width = 0;
                var cleanLine = line.Buffer.AsSpan().Trim();
                var lastLine = lineIndex == 0 ? line : lines[lineIndex - 1];
                line.Type = !lastLine.Complete ? lastLine.Type : debugConsole.DetectMessageType(cleanLine);
                line.Color = !lastLine.Complete ? lastLine.Color : GetColourForMessageType(line.Type);

                if (cleanLine.IsEmpty || !debugConsole.PassesFilter(line.Type))
                    line.Buffer.Clear();  // this line is bs, go away
                else  // this line should be considered
                {
                    VisibleLineCount++;
                    lineIndex++;

                    if (lineIndex >= lines.Length)
                        return;
                }
            }
        }
    }

    private void DrawConsoleBuffer(IGraphics graphics)
    {
        var lineRect = new Rect(0, 0, graphics.CurrentTarget.Size.X, LineHeight).Expand(-Padding).Translate(0, dropdownOffset);
        var totalRect = (backgroundRect with { MaxY = EffectiveHeight - InputHeight }).Translate(0, dropdownOffset);
        int lineOffset = debugConsole.ScrollOffset;
        graphics.DrawBounds = new DrawBounds(totalRect);

        for (int i = Math.Max(lineOffset, 0); i < VisibleLineCount; i++)
        {
            var line = lines[i];
            int offsetLineIndex = i - lineOffset;

            var t = line.Buffer.AsSpan().Trim();
            DrawText(graphics, t, lineRect.Translate(0, LineHeight * offsetLineIndex), line.Color, false);

            if (offsetLineIndex >= MaxLineCount - 1)
                return;
        }
    }

    public Color GetColourForMessageType(ConsoleMessageType type)
        => type switch
        {
            ConsoleMessageType.Debug => Colors.Purple,
            ConsoleMessageType.Info => Colors.White,
            ConsoleMessageType.Warning => Colors.Orange,
            ConsoleMessageType.Error => Colors.Red,
            _ => InputTextColour.WithAlpha(0.8f),
        };

    private void DrawInputText(IGraphics graphics)
    {
        float cursorPos = 0;
        var r = inputBoxRect.Expand(-Padding);

        if (!string.IsNullOrWhiteSpace(debugConsole.CurrentInput))
        {
            graphics.DrawBounds = new DrawBounds(inputBoxRect);
            DrawText(graphics, debugConsole.CurrentInput.AsSpan(0, Math.Min(debugConsole.CurrentInput.Length, 256)), r.Translate(0, -3), InputTextColour, false, HorizontalTextAlign.Left, VerticalTextAlign.Bottom);
            cursorPos = textMeshGenerator.CalculateTextWidth(debugConsole.CurrentInput.AsSpan(0, debugConsole.CursorPosition));
        }

        if (CaretBlinkTime % 1 < 0.5f)
        {
            mat.SetUniform("mainTex", Texture.White);
            DrawQuad(graphics,
                new Rect(new Vector2(cursorPos + Padding + 2, (r.MinY + r.MaxY) / 2), new Vector2(1, r.Height)),
                CaretColour);
        }
    }

    private void DrawPanels(IGraphics graphics)
    {
        mat.SetUniform("mainTex", Texture.White);
        mat.SetUniform("bgIntensity", BackgroundIntensity);
        mat.SetUniform("bgTex", BackgroundTexture ?? Texture.White);
        DrawQuad(graphics, backgroundRect, BackgroundColour);

        mat.SetUniform("bgIntensity", 0f);
        DrawQuad(graphics, inputBoxRect, InputBoxColour);
        DrawQuad(graphics, filterBoxRect, FilterBoxColour);
    }

    private void DrawFilterButtons(IGraphics graphics)
    {
        foreach (var item in filterButtons)
        {
            mat.SetUniform("mainTex", item.Texture);
            var enabled = debugConsole.Filter.HasFlag(item.MessageType);

            var col = enabled ? item.Color : InputBoxColour;
            var r = item.Rect;

            if (item.Hover)
                col.A = 0.7f;

            DrawQuad(graphics, r, col);
        }
    }

    private void DrawQuad(IGraphics graphics, Rect rect, Color color)
    {
        graphics.CurrentTarget.ModelMatrix =
            new Matrix4x4(
                Matrix3x2.CreateScale(rect.GetSize()) *
                Matrix3x2.CreateTranslation(rect.BottomLeft)
            );
        mat.SetUniform("tint", color);
        graphics.Draw(PrimitiveMeshes.Quad, mat);
    }

    private TextMeshResult DrawText(IGraphics graphics, ReadOnlySpan<char> text, Rect rect, Color color,
        bool wrap = true, HorizontalTextAlign horizontalTextAlign = HorizontalTextAlign.Left, VerticalTextAlign verticalTextAlign = VerticalTextAlign.Top)
    {
        if (text.IsEmpty || text.IsWhiteSpace())
            return default;

        if (verticalTextAlign == VerticalTextAlign.Bottom)
            rect = rect.Translate(0, rect.Height);

        graphics.CurrentTarget.ModelMatrix =
            new Matrix4x4(
                Matrix3x2.CreateScale(1, -1) *
                Matrix3x2.CreateTranslation(rect.BottomLeft)
            );

        textMeshGenerator.Color = color;
        textMeshGenerator.ParseRichText = false;
        textMeshGenerator.HorizontalAlign = horizontalTextAlign;
        textMeshGenerator.VerticalAlign = verticalTextAlign;
        textMeshGenerator.WrappingWidth = wrap ? rect.Width : float.MaxValue;
        textMeshGenerator.Font = Font.Default;
        textMeshGenerator.Multiline = false;

        var result = textMeshGenerator.Generate(text, textMesh.Vertices, textMesh.Indices);
        textMesh.AmountOfIndicesToRender = result.IndexCount;
        textMesh.ForceUpdate();

        mat.SetUniform("mainTex", Font.Default.Page);
        mat.SetUniform("bgIntensity", 0f);
        mat.SetUniform("tint", Colors.White);
        graphics.Draw(textMesh, mat);

        return result;
    }
}
