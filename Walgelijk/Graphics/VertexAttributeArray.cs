using System.Collections;
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
        public abstract int Count { get; }

        /// <summary>
        /// The raw data arraw
        /// </summary>
        public abstract T[] GetData<T>();
    }

    /// <summary>
    /// Float vertex attribute array
    /// </summary>
    public class FloatAttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Float;

        public float[] Data;
        public override int Count => Data.Length;

        public FloatAttributeArray(float[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (float)value;

        public override T[] GetData<T>() => Data as T[];
    }

    /// <summary>
    /// Float vertex attribute array
    /// </summary>
    public class IntAttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Integer;

        public int[] Data;
        public override int Count => Data.Length;

        public IntAttributeArray(int[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (int)value;

        public override T[] GetData<T>() => Data as T[];
    }

    /// <summary>
    /// Vector2 vertex attribute array
    /// </summary>
    public class Vector2AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Vector2;

        public Vector2[] Data;
        public override int Count => Data.Length;

        public Vector2AttributeArray(Vector2[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Vector2)value;

        public override T[] GetData<T>() => Data as T[];
    }

    /// <summary>
    /// Vector3 vertex attribute array
    /// </summary>
    public class Vector3AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Vector3;

        public Vector3[] Data;
        public override int Count => Data.Length;

        public Vector3AttributeArray(Vector3[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Vector3)value;

        public override T[] GetData<T>() => Data as T[];
    }

    /// <summary>
    /// Vector4 vertex attribute array
    /// </summary>
    public class Vector4AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Vector4;

        public Vector4[] Data;
        public override int Count => Data.Length;

        public Vector4AttributeArray(Vector4[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Vector4)value;

        public override T[] GetData<T>() => Data as T[];
    }

    /// <summary>
    /// Matrix4x4 vertex attribute array
    /// </summary>
    public class Matrix4x4AttributeArray : VertexAttributeArray
    {
        public override AttributeType AttributeType => AttributeType.Matrix4x4;

        public Matrix4x4[] Data;
        public override int Count => Data.Length;

        public Matrix4x4AttributeArray(Matrix4x4[] data)
        {
            Data = data;
        }

        public override object GetAt(int index) => Data[index];

        public override void SetAt(int index, object value) => Data[index] = (Matrix4x4)value;

        public override T[] GetData<T>() => Data as T[];
    }
}
