using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Struct that holds an array and its vertex attribute type as an enum
    /// </summary>
    public abstract class VertexAttributeArray
    {
        /// <summary>
        /// Type of data
        /// </summary>
        public abstract AttributeType AttributeType { get; }

        /// <summary>
        /// Get value at an index
        /// </summary>
        public abstract object GetAt(int index);

        /// <summary>
        /// Get value at an index
        /// </summary>
        public abstract void SetAt(int index, object value);

        /// <summary>
        /// Amount of elements
        /// </summary>
        public int Count => Data.Length;

        /// <summary>
        /// Raw object data
        /// </summary>
        public dynamic Data;
    }

    /// <summary>
    /// Float vertex attribute array
    /// </summary>
    public class FloatAttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Float;

        public FloatAttributeArray(float[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (float)value;
    }

    /// <summary>
    /// Float vertex attribute array
    /// </summary>
    public class IntAttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Integer;

        public IntAttributeArray(int[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (int)value;
    }

    /// <summary>
    /// Vector2 vertex attribute array
    /// </summary>
    public class Vector2AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Vector2;

        public Vector2AttributeArray(Vector2[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Vector2)value;
    }

    /// <summary>
    /// Vector3 vertex attribute array
    /// </summary>
    public class Vector3AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Vector3;

        public Vector3AttributeArray(Vector3[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Vector3)value;
    }

    /// <summary>
    /// Vector4 vertex attribute array
    /// </summary>
    public class Vector4AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Vector4;

        public Vector4AttributeArray(Vector4[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Vector4)value;
    }

    /// <summary>
    /// Matrix4x4 vertex attribute array
    /// </summary>
    public class Matrix4x4AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Matrix4x4;

        public Matrix4x4AttributeArray(Matrix4x4[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Matrix4x4)value;
    }
}
