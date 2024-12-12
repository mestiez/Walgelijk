using System;
using System.Linq;
using System.Numerics;
using static Walgelijk.TextMeshGenerator;
#pragma warning disable CA2211 // Non-constant fields should not be visible

namespace Walgelijk.SimpleDrawing;

/// <summary>
/// Main simple drawing function class
/// </summary>
public static class Draw
{
    /// <summary>
    /// The colour to draw with
    /// </summary>
    public static Color Colour = Colors.White;

    /// <summary>
    /// The order at which to draw
    /// </summary>
    public static RenderOrder Order = RenderOrder.Zero;

    /// <summary>
    /// The transformation matrix that is applied to drawings
    /// </summary>
    public static Matrix3x2 TransformMatrix = Matrix3x2.Identity;

    /// <summary>
    /// Are all given transformations in screen space?
    /// </summary>
    public static bool ScreenSpace = false;

    /// <summary>
    /// The current drawing bounds
    /// </summary>
    public static DrawBounds DrawBounds = DrawBounds.DisabledBounds;

    /// <summary>
    /// The current font
    /// </summary>
    public static Font Font = Font.Default;

    /// <summary>
    /// The current font size in px
    /// </summary>
    public static float FontSize = 12;

    /// <summary>
    /// The current outline width. No outline will be drawn if approximately 0 or lower
    /// </summary>
    public static float OutlineWidth = 0;

    /// <summary>
    /// The current blend mode. Set to null to fall back to the Material's set blend mode.
    /// </summary>
    public static BlendMode? BlendMode;

    /// <summary>
    /// The current image mode. Determines how textures are positioned within the mesh.
    /// </summary>
    public static ImageMode ImageMode;

    /// <summary>
    /// The current stencil state. 
    /// </summary>
    public static StencilState Stencil;

    /// <summary>
    /// The current outline colour. No outline will be drawn if transparent
    /// </summary>
    public static Color OutlineColour = Colors.Black;

    /// <summary>
    /// Material for shapes. <b>This has no influence over text.</b> Text uses the material provided by the <see cref="Font"/>.
    /// </summary>
    public static Material? Material = null;
    /// <summary>
    /// The texture for shapes. <b>This has no influence over text.</b> Text uses the texture provided by the <see cref="Font"/>.
    /// </summary>
    public static IReadableTexture Texture = Walgelijk.Texture.White;
    /// <summary>
    /// Text generator used for <see cref="Text"/>
    /// </summary>
    public static readonly TextMeshGenerator TextMeshGenerator = new() { ParseRichText = true };

    /// <summary>
    /// Amount of calls to generate a text mesh necessary for it to be cached. -1 means caching is disabled.
    /// </summary>
    public static int CacheTextMeshes = 200;

    /// <summary>
    /// Value from 0.0 to 1.0 that determines the percentage of the text to actually draw. This is usually used for "writing" animations for things like dialogue.
    /// </summary>
    public static float TextDrawRatio = 1;

    /// <summary>
    /// Calculate the pixel width of the given text considering the font and font size. This function doesn't care about wrapping.
    /// </summary>
    public static float CalculateTextWidth(ReadOnlySpan<char> text)
    {
        TextMeshGenerator.Font = Font;
        return TextMeshGenerator.CalculateTextWidth(text) * (FontSize / Font.Size);
    }

    /// <summary>
    /// Calculate the pixel height of the given text considering the font and font size. This function cares about wrapping.
    /// </summary>
    public static float CalculateTextHeight(ReadOnlySpan<char> text, float wrapWidth = float.PositiveInfinity)
    {
        var ratio = FontSize / Font.Size;

        TextMeshGenerator.Font = Font;
        TextMeshGenerator.WrappingWidth = wrapWidth / ratio;

        var h = TextMeshGenerator.CalculateTextHeight(text);
        return h * ratio;
    }

    /// <summary>
    /// Reference to the target <see cref="Walgelijk.RenderQueue"/>. The system will fall back to the currently active RenderQueue if this is null.s
    /// </summary>
    public static RenderQueue? RenderQueue;

    internal static DrawingTaskPool TaskPool = new(65536);
    internal static PolygonPool PolygonPool = new(65536);
    internal static TextMeshCache TextMeshCache = new();

    /// <summary>
    /// Clear all task pools
    /// </summary>
    [Command(Alias = "ClearDrawPools")]
    public static void ClearPools()
    {
        foreach (var item in TaskPool.GetAllInUse().ToArray())
            TaskPool.ReturnToPool(item);
        foreach (var item in PolygonPool.GetAllInUse().ToArray())
            PolygonPool.ReturnToPool(item);

        TaskPool = new(65536);
        PolygonPool = new(65536);
    }

    /// <summary>
    /// Dispose of all cached text meshes. This frees a bit of memory (both GPU and CPU), but may briefly slow down the application 
    /// </summary>
    public static void ClearTextMeshCache()
    {
        TextMeshCache.UnloadAll();
    }

    private static RenderQueue Queue => RenderQueue ?? Game.Main.RenderQueue;
    private static readonly VertexBuffer textMesh = new(Array.Empty<Vertex>(), Array.Empty<uint>()) { Dynamic = true };

    private static StencilRenderTask clearMaskTask = new StencilRenderTask(StencilState.Clear);
    private static StencilRenderTask disableMaskTask = new StencilRenderTask(StencilState.Disabled);

    private class StencilRenderTask(StencilState state) : IRenderTask
    {
        public readonly StencilState State = state;

        public void Execute(IGraphics graphics)
        {
            graphics.Stencil = State;
        }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void Enqueue(Drawing drawing)
    {
        var obj = TaskPool.RequestObject(drawing);
        if (obj != null)
            Queue.Add(obj, Order);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Drawing DrawingFor(VertexBuffer vtx, Vector2 position, Vector2 scale, float degrees)
        => new(
            vtx,
            position,
            scale,
            degrees * Utilities.DegToRad, Colour,
            Material ?? DrawingMaterialCreator.Cache.Load(Texture ?? Walgelijk.Texture.White),
            Texture ?? Walgelijk.Texture.White, ScreenSpace, DrawBounds, OutlineWidth, OutlineColour, ImageMode)
        {
            Stencil = Stencil,
            Transformation = TransformMatrix,
            BlendMode = BlendMode
        };

    /// <summary>
    /// Reset the drawing state: <br></br><br></br>
    /// material = default drawing material, <br></br>
    /// texture = white pixel, <br></br>
    /// colour = white, <br></br>
    /// font = default font,<br></br>
    /// font size = 12, <br></br>
    /// drawbounds = none,<br></br>
    /// order = 0, <br></br>
    /// screenspace = false,<br></br>
    /// outline width = 0,<br></br>
    /// outline colour = white,<br></br>
    /// textdrawratio = 1,<br></br>
    /// transform matrix = identity,<br></br>
    /// stencil = disabled
    /// </summary>
    public static void Reset()
    {
        ResetTexture();
        ResetDrawBounds();
        ResetMaterial();
        BlendMode = null;
        Order = default;
        Font = Font.Default;
        FontSize = 12;
        ScreenSpace = false;
        Colour = Colors.White;
        OutlineWidth = 0;
        OutlineColour = default;
        TextDrawRatio = 1;
        TransformMatrix = Matrix3x2.Identity;
        ImageMode = default;
        Stencil = StencilState.Disabled;
    }

    /// <summary>
    /// Reset the texture for shapes to white.
    /// </summary>
    public static void ResetTexture() => Texture = Walgelijk.Texture.White;

    /// <summary>
    /// Reset the transformation matrix.
    /// </summary>
    public static void ResetTransformation() => TransformMatrix = Matrix3x2.Identity;

    /// <summary>
    /// Reset the draw bounds to a disabled state.
    /// </summary>
    public static void ResetDrawBounds() => DrawBounds = DrawBounds.DisabledBounds;

    /// <summary>
    /// Reset the material for shapes to white.
    /// </summary>
    public static void ResetMaterial() => Material = null;

    /// <summary>
    /// Disable interaction with the stencil buffer entirely
    /// </summary>
    public static void DisableMask()
    {
        Stencil = StencilState.Disabled;
        Queue.Add(disableMaskTask, Order);
    }

    /// <summary>
    /// Set the stencil state to "write" mode, meaning everything drawn will determine the mask
    /// </summary>
    public static void WriteMask() => Stencil = StencilState.WriteMask;

    /// <summary>
    /// Only allow drawing inside the mask
    /// </summary>
    public static void InsideMask() => Stencil = StencilState.InsideMask;

    /// <summary>
    /// Only allow drawing outside the mask
    /// </summary>
    public static void OutsideMask() => Stencil = StencilState.OutsideMask;

    /// <summary>
    /// Clear the mask to black
    /// </summary>
    public static void ClearMask()
    {
        Stencil = StencilState.Clear;
        Queue.Add(clearMaskTask, Order);
    }

    /// <summary>
    /// Clear the screen
    /// </summary>
    public static void Clear(Color color = default)
    {
        Queue.Add(new ClearRenderTask(color), Order);
    }

    /// <summary>
    /// Draw a <see cref="DrawingPrimitives.Quad"/>
    /// </summary>
    public static void Quad(Rect rect, float degrees = 0, float roundness = 0) =>
        Quad(ScreenSpace ? rect.BottomLeft : rect.TopLeft, rect.GetSize(), degrees, roundness);

    public static void Quad(Vector2 topLeft, Vector2 size, float degrees = 0, float roundness = 0)
    {
        var drawing = DrawingFor(DrawingPrimitives.Quad, topLeft, size, degrees);
        drawing.Roundness = roundness;
        Enqueue(drawing);
    }

    /// <summary>
    /// Draw a <see cref="DrawingPrimitives.Circle"/>
    /// </summary>
    public static void Circle(Vector2 center, Vector2 radius, float degrees = 0)
    {
        Enqueue(DrawingFor(DrawingPrimitives.Circle, center, new Vector2(MathF.Abs(radius.X), MathF.Abs(radius.Y)), degrees) with
        {
            CircleMorph = 1
        });
    }

    /// <summary>
    /// Draw a <see cref="DrawingPrimitives.IsoscelesTriangle"/>
    /// </summary>
    public static void TriangleIsco(Vector2 topLeft, Vector2 size, float degrees = 0)
    {
        Enqueue(DrawingFor(DrawingPrimitives.IsoscelesTriangle, topLeft, size, degrees));
    }

    /// <summary>
    /// Draw a <see cref="DrawingPrimitives.CenteredIsoscelesTriangle"/>
    /// </summary>
    public static void TriangleIscoCentered(Vector2 center, Vector2 size, float degrees = 0)
    {
        Enqueue(DrawingFor(DrawingPrimitives.CenteredIsoscelesTriangle, center, size, degrees));
    }

    /// <summary>
    /// Draw a <see cref="DrawingPrimitives.RightAngledTriangle"/>
    /// </summary>
    public static void TriangleRight(Vector2 topLeft, Vector2 size, float degrees = 0)
    {
        Enqueue(DrawingFor(DrawingPrimitives.RightAngledTriangle, topLeft, size, degrees));
    }

    /// <summary>
    /// Draw text.
    /// </summary>
    /// <param name="text">The text to draw. Supported characters depends on the font.</param>
    /// <param name="pivot">The pivot point.</param>
    /// <param name="scale">The scale multiplier. The size of the text is determined by <see cref="FontSize"/>. This parameter will usually just be <see cref="Vector2.One"/></param>.
    /// <param name="halign">How to horizontally align the text to the pivot point</param>
    /// <param name="valign">How to vertically align the text to the pivot point</param>
    /// <param name="textBoxWidth">The width before the text starts wrapping</param>
    /// <param name="degrees">Text rotation in degrees</param>
    public static void Text(
        string text, Vector2 pivot, Vector2 scale, HorizontalTextAlign halign = HorizontalTextAlign.Left, VerticalTextAlign valign = VerticalTextAlign.Top,
        float textBoxWidth = float.PositiveInfinity, float degrees = 0)
    {
        Vector2 calculatedScale = scale * (FontSize / Font.Size);
        TextDrawing textDrawing = new()
        {
            Font = Font,
            Text = text,
            HorizontalAlign = halign,
            VerticalAlign = valign,
            TextBoxWidth = textBoxWidth / calculatedScale.X,
            TextDrawRatio = TextDrawRatio
        };
        Enqueue(new Drawing(textMesh, pivot, calculatedScale, degrees * Utilities.DegToRad, Colour, ScreenSpace, textDrawing, DrawBounds)
        {
            Stencil = Stencil,
            Transformation = TransformMatrix,
            BlendMode = BlendMode
        });
    }

    /// <summary>
    /// Draw a line from A to B
    /// </summary>
    public static void Line(Vector2 from, Vector2 to, float width, float roundness = 50)
    {
        var delta = to - from;
        var distance = delta.Length();
        var dir = delta / distance;

        from -= dir * width / 2;
        from += new Vector2(dir.Y, -dir.X) * width * 0.5f * (ScreenSpace ? 1 : -1);

        float deg = Utilities.VectorToAngle(dir);
        Quad(from, new Vector2(distance + width, width), -deg, roundness);
    }

    /// <summary>
    /// Draw a quadrilateral shape
    /// </summary>
    public static void Quad(Vector2 topleft, Vector2 topright, Vector2 bottomleft, Vector2 bottomright)
    {
        var vtx = PolygonPool.RequestObject(true);
        if (vtx == null)
        {
            Logger.Warn($"Could not draw quadrilateral because there are {PolygonPool.MaximumCapacity} (the limit) vertex buffers being used in the pool");
            return;
        }

        if (vtx.Vertices == null || vtx.VertexCount != 4)
            vtx.Vertices = new Vertex[4];
        if (vtx.Indices == null || vtx.IndexCount != 6)
            vtx.Indices = new uint[6];

        DrawingPrimitives.Quad.Indices.CopyTo(vtx.Indices, 0);

        vtx.Vertices[0] = new Vertex(new Vector3(bottomleft, 0), new Vector2(0, 0), Colors.White);
        vtx.Vertices[1] = new Vertex(new Vector3(bottomright, 0), new Vector2(1, 0), Colors.White);
        vtx.Vertices[2] = new Vertex(new Vector3(topright, 0), new Vector2(1, 1), Colors.White);
        vtx.Vertices[3] = new Vertex(new Vector3(topleft, 0), new Vector2(0, 1), Colors.White);

        vtx.ForceUpdate();

        var drawing = DrawingFor(vtx, default, Vector2.One, 0);
        drawing.ReturnVertexBufferToPool = true;
        Enqueue(drawing);
    }

    /// <summary>
    /// Draw an image inside a given rectangle. Notice that this sets Draw.Texture
    /// </summary>
    public static Rect Image(IReadableTexture texture, Rect rect, ImageContainmentMode containmentMode, float degrees = 0, float roundness = 0)
    {
        var textureSize = texture.Size;
        var size = rect.GetSize();
        var topLeft = rect.BottomLeft;

        Vector2 imageSize;
        Vector2 imagePos = default;

        switch (containmentMode)
        {
            case ImageContainmentMode.Stretch:
                imageSize = size;
                break;
            case ImageContainmentMode.Contain:
            case ImageContainmentMode.Cover:
                var aspectRatio = textureSize.X / textureSize.Y;

                imageSize = size;
                bool a = size.X / aspectRatio > size.Y;

                if (containmentMode == ImageContainmentMode.Contain)
                    a = !a;

                if (a)
                    imageSize.Y = size.X / aspectRatio;
                else
                    imageSize.X = size.Y * aspectRatio;

                imagePos = size / 2 - imageSize / 2;
                break;
            case ImageContainmentMode.Center:
                imageSize = textureSize;
                imagePos = size / 2 - imageSize / 2;
                break;
            default:
            case ImageContainmentMode.OriginalSize:
                imageSize = textureSize;
                break;
        }

        Draw.Texture = texture;
        Draw.Quad(topLeft + imagePos, imageSize, degrees, roundness);

        return new Rect(topLeft.X + imagePos.X, topLeft.Y + imagePos.Y, topLeft.X + imagePos.X + imageSize.X, topLeft.Y + imagePos.Y + imageSize.Y);
    }
}
#pragma warning restore CA2211 // Non-constant fields should not be visible
