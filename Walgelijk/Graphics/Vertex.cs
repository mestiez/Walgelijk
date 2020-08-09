using System.Numerics;

namespace Walgelijk
{
    public struct Vertex
    {
        /// <summary>
        /// Size of an instance of this struct in bytes
        /// </summary>
        public const int Stride = sizeof(float) * (3 + 2 + Color.Stride);

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
        /// 
        /// </summary>
        /// <param name="position"></param>
        public Vertex(Vector3 position)
        {
            Position = position;
            TexCoords = Vector2.Zero;
            Color = Color.White;
        }
    }
}
