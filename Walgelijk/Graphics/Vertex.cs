using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// A vertex
    /// </summary>
    public struct Vertex
    {
        /// <summary>
        /// Size of an instance of this struct in bytes
        /// </summary>
        public const int Stride = sizeof(float) * (3 + 2) + Color.Stride;

        /// <summary>
        /// Vertex position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Vertex UV coordinates
        /// </summary>
        public Vector2 TexCoords;
        /// <summary>
        /// Vertex colour
        /// </summary>
        public Color Color;

        /// <summary>
        /// Create a vertex with a position, UV, and colour
        /// </summary>
        /// <param name="position"></param>
        /// <param name="texCoords"></param>
        /// <param name="color"></param>
        public Vertex(Vector3 position, Vector2 texCoords, Color color)
        {
            Position = position;
            TexCoords = texCoords;
            Color = color;
        }        
        
        /// <summary>
        /// Create a vertex with a position. White and zero UV by default;
        /// </summary>
        /// <param name="position"></param>
        public Vertex(Vector3 position)
        {
            Position = position;
            TexCoords = Vector2.Zero;
            Color = Color.White;
        }

        /// <summary>
        /// Create a vertex with a position.  White and zero UV by default;
        /// </summary>
        /// <param name="position"></param>
        public Vertex(float x, float y, float z = 0)
        {
            Position = new Vector3(x,y,z);
            TexCoords = Vector2.Zero;
            Color = Color.White;
        }
    }
}
