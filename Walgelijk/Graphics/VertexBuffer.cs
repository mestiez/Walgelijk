using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Holds all the data needed to draw vertices to the screen
    /// </summary>
    public class VertexBuffer
    {
        private Vertex[] vertices;
        private uint[] indices;

        private readonly VertexAttributeArray[] extraAttributes = null;

        /// <summary>
        /// The way vertices are drawn
        /// </summary>
        public Primitive PrimitiveType { get; set; } = Primitive.Triangles;

        /// <summary>
        /// Create a VertexBuffer with the specified vertices and indices
        /// </summary>
        public VertexBuffer(Vertex[] vertices, uint[] indices, VertexAttributeArray[] extraAttributes = null)
        {
            this.vertices = vertices;
            this.indices = indices;

            this.extraAttributes = extraAttributes;
        }

        /// <summary>
        /// Create a VertexBuffer with the specified vertices. The indices will be set automatically
        /// </summary>
        public VertexBuffer(Vertex[] vertices)
        {
            Vertices = vertices;
            GenerateIndices();
        }

        /// <summary>
        /// Create an empty vertex buffer
        /// </summary>
        public VertexBuffer()
        {
        }

        /// <summary>
        /// Whether the data needs to be uploaded to the GPU again
        /// </summary>
        public bool HasChanged { get; set; } = false;

        // TODO dit moet automatisch op true gezet worden als iets in de extraAttributes array verandert
        /// <summary>
        /// Whether the extra data needs to be uploaded to the GPU again
        /// </summary>
        public bool ExtraDataHasChanged { get; set; } = false;

        /// <summary>
        /// Vertices to draw. <b>Do not forget to set the corresponding indices, or use <see cref="GenerateIndices"/></b>
        /// </summary>
        public Vertex[] Vertices
        {
            get => vertices;

            set
            {
                vertices = value;
                HasChanged = true;
            }
        }

        /// <summary>
        /// Indices to draw vertices by
        /// </summary>
        public uint[] Indices
        {
            get => indices;

            set
            {
                indices = value;
                HasChanged = true;
            }
        }

        /// <summary>
        /// Amount of indices
        /// </summary>
        public int IndexCount => Indices?.Length ?? 0;

        /// <summary>
        /// Amount of vertices
        /// </summary>
        public int VertexCount => Vertices?.Length ?? 0;

        /// <summary>
        /// Force the data to be reuploaded to the GPU
        /// </summary>
        public void ForceUpdate()
        {
            HasChanged = true;
        }

        /// <summary>
        /// Generates indices that simply walk the vertex array from beginning to end
        /// </summary>
        public void GenerateIndices()
        {
            indices = new uint[vertices.Length];
            for (uint i = 0; i < vertices.Length; i++)
                indices[i] = i;
            HasChanged = true;
        }

        ///// <summary>
        ///// Add a new vertex attribute array
        ///// </summary>
        ///// <returns>Location of the attribute (-3)</returns>
        //public int AddAttribute(VertexAttributeArray array)
        //{
        //    ja dit mag dus niet. het mag een keer op het begin maar daarna mag je alleen maar de arrays aanpassen

        //    if (extraAttributes == null)
        //        throw new global::System.InvalidOperationException("This vertex buffer was created without an extra attribute list. You can't add an attribute.");

        //    extraAttributes.Add(array);
        //    return ExtraAttributeCount - 1;
        //}

        /// <summary>
        /// Get a vertex attribute array. Returns null if nothing is found. This is a reference value.
        /// </summary>
        public VertexAttributeArray GetAttribute(int location)
        {
            if (extraAttributes == null)
                return null;

            if (location < 0 || location >= ExtraAttributeCount)
                return null;

            return extraAttributes[location];
        }

        /// <summary>
        /// Returns the amount of extra attributes. The total amount of attributes equals this value + 3
        /// </summary>
        public int ExtraAttributeCount => extraAttributes?.Length ?? 0;
    }
}
