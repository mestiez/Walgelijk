using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.SimpleDrawing
{
    /// <summary>
    /// A drawing instruction
    /// </summary>
    public struct Drawing
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
        public Matrix4x4 Transformation = Matrix4x4.Identity;

        /// <summary>
        /// Roundness, if applicable
        /// </summary>
        public float Roundness;
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
            float outlineWidth, Color outlineColour)
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
        }

        /// <summary>
        /// Create a drawing for text
        /// </summary>
        public Drawing(VertexBuffer vertexBuffer, Vector2 position, Vector2 scale, float rotationRadians, Color color, bool screenSpace, TextDrawing textDrawing, DrawBounds drawBounds)
        {
            VertexBuffer = vertexBuffer;
            Material = textDrawing.Font?.Material ?? Font.Default.Material;
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
            return obj != null && obj is Drawing drawing &&
                   EqualityComparer<VertexBuffer>.Default.Equals(VertexBuffer, drawing.VertexBuffer) &&
                   EqualityComparer<Material>.Default.Equals(Material, drawing.Material) &&
                   ReferenceEquals(Texture, drawing.Texture) &&
                   TextDrawing.Equals(drawing.TextDrawing) &&
                   ScreenSpace.Equals(drawing.ScreenSpace) &&
                   VectorEquals(Position, drawing.Position) &&
                   Approximately(RotationRadians, drawing.RotationRadians) &&
                   Approximately(Roundness, drawing.Roundness) &&
                   VectorEquals(Scale, drawing.Scale) &&
                   ColorEquals(Color, drawing.Color);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VertexBuffer, Material, Position, RotationRadians, Scale, Color);
        }

        public static bool operator ==(Drawing left, Drawing right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Drawing left, Drawing right)
        {
            return !(left == right);
        }
    }
}