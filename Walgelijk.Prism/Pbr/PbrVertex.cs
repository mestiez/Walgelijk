using System.Numerics;

namespace Walgelijk.Prism.Pbr;

public struct PbrVertex
{
    public Vector3 Position;
    public Vector2 TexCoords;
    public Vector2 TexCoords2;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 BiTangent;
    public Vector4 Color;

    public PbrVertex(Vector3 position, Vector2 texCoords, Vector3 normal, Vector3 tangent, Vector3 biTangent, Color color)
    {
        Position = position;
        TexCoords = texCoords;
        TexCoords2 = texCoords;
        Normal = normal;
        Tangent = tangent;
        BiTangent = biTangent;
        Color = color;
    }

    public readonly struct Descriptor : IVertexDescriptor
    {
        public IEnumerable<VertexAttributeDescriptor> GetAttributes()
        {
            yield return new VertexAttributeDescriptor(AttributeType.Vector3); // position
            yield return new VertexAttributeDescriptor(AttributeType.Vector2); // uv
            yield return new VertexAttributeDescriptor(AttributeType.Vector2); // uv 2
            yield return new VertexAttributeDescriptor(AttributeType.Vector3); // normal
            yield return new VertexAttributeDescriptor(AttributeType.Vector3); // tangent
            yield return new VertexAttributeDescriptor(AttributeType.Vector3); // bitangent
            yield return new VertexAttributeDescriptor(AttributeType.Vector4); // color
        }
    }
}
