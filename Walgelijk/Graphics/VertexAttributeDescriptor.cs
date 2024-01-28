using System;

namespace Walgelijk;

/// <summary>
/// Describes a single vertex attribute array. Used to describe individual fields inside a vertex struct
/// </summary>
public readonly struct VertexAttributeDescriptor
{
    /// <summary>
    /// The data type of the attribute in the vertex struct and the vertex shader
    /// </summary>
    public readonly AttributeType Type;
    /// <summary>
    /// The amount of components of this attribute. E.g a <see cref="Vector3"/> would have 3 components
    /// </summary>
    public readonly int ComponentCount;
    /// <summary>
    /// The size in bytes of each individual component. E.g a <see cref="Vector2"/> has two <see cref="float"/> components, which are <c>sizeof(float)</c> bytes long. That means this value would be set to <c>sizeof(float)</c>.
    /// </summary>
    public readonly int SizePerComponent;

    /// <summary>
    /// Identical to <code>SizePerComponent * ComponentCount</code>
    /// </summary>
    public readonly int TotalSize => SizePerComponent * ComponentCount;

    /// <summary>
    /// Manually construct a vertex attribute descriptor. Please just use the <see cref="VertexAttributeDescriptor(AttributeType)"/> constructor
    /// </summary>
    public VertexAttributeDescriptor(AttributeType type, int componentCount, int sizeInBytes)
    {
        Type = type;
        ComponentCount = componentCount;
        SizePerComponent = sizeInBytes;
    }

    /// <summary>
    /// Create a vertex attribute descriptor based on the given attribute type
    /// </summary>
    public VertexAttributeDescriptor(AttributeType type)
    {
        Type = type;

        switch (type)
        {
            case AttributeType.Integer:
                ComponentCount = 1;
                SizePerComponent = sizeof(int);
                break;
            case AttributeType.Float:
                ComponentCount = 1;
                SizePerComponent = sizeof(float);
                break;
            case AttributeType.Double:
                ComponentCount = 1;
                SizePerComponent = sizeof(double);
                break;
            case AttributeType.Vector2:
                ComponentCount = 2;
                SizePerComponent = sizeof(float);
                break;
            case AttributeType.Vector3:
                ComponentCount = 3;
                SizePerComponent = sizeof(float);
                break;
            case AttributeType.Vector4:
                ComponentCount = 4;
                SizePerComponent = sizeof(float);
                break;
            case AttributeType.Matrix4x4:
                ComponentCount = 4 * 4;
                SizePerComponent = sizeof(float);
                break;
            default:
                throw new Exception("invalid vertex attribute type");
        }
    }
}
