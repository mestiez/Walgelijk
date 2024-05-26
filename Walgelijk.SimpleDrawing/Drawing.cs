using System;
using System.Numerics;

namespace Walgelijk.SimpleDrawing;

/// <summary>
/// A drawing instruction
/// </summary>
public struct Drawing : IEquatable<Drawing>
{
    /// <summary>
    /// Relevant VertexBuffer
    /// </summary>
    public VertexBuffer? VertexBuffer;
    /// <summary>
    /// Material to draw with
    /// </summary>
    public Material Material;
    /// <summary>
    /// Texture to draw with
    /// </summary>
    public IReadableTexture? Texture;
    /// <summary>
    /// Drawbounds to draw within
    /// </summary>
    public DrawBounds DrawBounds;
    /// <summary>
    /// Should the vertex buffer be returned to the <see cref="Draw.PolygonPool"/>?
    /// </summary>
    public bool ReturnVertexBufferToPool;

    /// <summary>
    /// Relevant position
    /// </summary>
    public Vector2 Position;
    /// <summary>
    /// Relevant rotation in radians
    /// </summary>
    public float RotationRadians;
    /// <summary>
    /// Relevant scaling vector
    /// </summary>
    public Vector2 Scale;
    /// <summary>
    /// Relevant transformation matrix
    /// </summary>
    public Matrix3x2 Transformation = Matrix3x2.Identity;
    /// <summary>
    /// Current blend mode
    /// </summary>
    public BlendMode? BlendMode = null;
    /// <summary>
    /// Current image mode
    /// </summary>
    public ImageMode ImageMode;
    /// <summary>
    /// Current stencil state
    /// </summary>
    public StencilState Stencil;

    /// <summary>
    /// Roundness, if applicable
    /// </summary>
    public float Roundness;    
    /// <summary>
    /// The closer to 1, the more the shader assumes a circular mesh
    /// </summary>
    public float CircleMorph;
    /// <summary>
    /// Outline width, if applicable
    /// </summary>
    public float OutlineWidth;
    /// <summary>
    /// Should the transformation be considered to be in screenspace?
    /// </summary>
    public bool ScreenSpace;
    /// <summary>
    /// Colour to draw with
    /// </summary>
    public Color Color;
    /// <summary>
    /// Colour to draw outlines with, if applicable
    /// </summary>
    public Color OutlineColour;

    /// <summary>
    /// The relevant <see cref="TextDrawing"/>, if applicable
    /// </summary>
    public TextDrawing? TextDrawing;

    /// <summary>
    /// Create a drawing for a simple shape with a texture
    /// </summary>
    public Drawing(
        VertexBuffer vertexBuffer, Vector2 position, Vector2 scale, float rotationRadians,
        Color color, Material material, IReadableTexture texture, bool screenSpace, DrawBounds drawBounds,
        float outlineWidth, Color outlineColour, ImageMode imageMode)
    {
        VertexBuffer = vertexBuffer;
        Material = material;
        Texture = texture;
        ScreenSpace = screenSpace;
        Position = position;
        RotationRadians = rotationRadians;
        Scale = scale;
        Color = color;
        TextDrawing = null;
        Roundness = 0;
        DrawBounds = drawBounds;
        OutlineWidth = outlineWidth;
        OutlineColour = outlineColour;
        ReturnVertexBufferToPool = false;
        ImageMode = imageMode;
        Stencil = StencilState.Disabled;
    }

    /// <summary>
    /// Create a drawing for text
    /// </summary>
    public Drawing(VertexBuffer vertexBuffer, Vector2 position, Vector2 scale, float rotationRadians, Color color, bool screenSpace, TextDrawing textDrawing, DrawBounds drawBounds)
    {
        VertexBuffer = vertexBuffer;
        Material = textDrawing.Font?.Material ?? Font.Default.Material ?? Material.DefaultTextured;
        Texture = null;
        ScreenSpace = screenSpace;
        Position = position;
        RotationRadians = rotationRadians;
        Scale = scale;
        Color = color;
        TextDrawing = textDrawing;
        Roundness = 0;
        DrawBounds = drawBounds;
        OutlineWidth = 0;
        OutlineColour = default;
        ReturnVertexBufferToPool = false;
        ImageMode = default;
        Stencil = StencilState.Disabled;
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool Approximately(float a, float b) => MathF.Abs(a - b) < 0.00001f;

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool VectorEquals(Vector2 a, Vector2 b) => Approximately(a.X, b.X) && Approximately(a.Y, b.Y);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool ColorEquals(Color a, Color b) =>
        Approximately(a.R, b.R) &&
        Approximately(a.G, b.G) &&
        Approximately(a.B, b.B) &&
        Approximately(a.A, b.A);

    public override bool Equals(object? obj)
    {
        return obj is Drawing other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(VertexBuffer);
        hashCode.Add(Material);
        hashCode.Add(Texture);
        hashCode.Add(DrawBounds);
        hashCode.Add(ReturnVertexBufferToPool);
        hashCode.Add(Position);
        hashCode.Add(RotationRadians);
        hashCode.Add(Scale);
        hashCode.Add(Transformation);
        hashCode.Add(BlendMode);
        hashCode.Add(Roundness);
        hashCode.Add(CircleMorph);
        hashCode.Add(OutlineWidth);
        hashCode.Add(ScreenSpace);
        hashCode.Add(Color);
        hashCode.Add(OutlineColour);
        hashCode.Add(TextDrawing);
        hashCode.Add(ImageMode);
        hashCode.Add(Stencil);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(Drawing left, Drawing right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Drawing left, Drawing right)
    {
        return !(left == right);
    }

    public bool Equals(Drawing other)
    {
        return Equals(VertexBuffer,
                   other.VertexBuffer) &&
               Material.Equals(other.Material) &&
               Equals(Texture,
                   other.Texture) &&
               DrawBounds.Equals(other.DrawBounds) &&
               ReturnVertexBufferToPool == other.ReturnVertexBufferToPool &&
               Position.Equals(other.Position) &&
               RotationRadians.Equals(other.RotationRadians) &&
               Scale.Equals(other.Scale) &&
               Transformation.Equals(other.Transformation) &&
               BlendMode == other.BlendMode &&
               Roundness.Equals(other.Roundness) &&
               CircleMorph.Equals(other.CircleMorph) &&
               OutlineWidth.Equals(other.OutlineWidth) &&
               ScreenSpace == other.ScreenSpace &&
               Color.Equals(other.Color) &&
               OutlineColour.Equals(other.OutlineColour) &&
               ImageMode.Equals(other.ImageMode) &&
               Stencil.Equals(other.Stencil) &&
               Nullable.Equals(TextDrawing,
                   other.TextDrawing);
    }
}