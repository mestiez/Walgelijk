using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Dictionary collection for all valid uniform datatypes
    /// </summary>
    public class UniformDictionary
    {
        public readonly Dictionary<string, int> Integers = new();
        public readonly Dictionary<string, float> Floats = new();
        public readonly Dictionary<string, double> Doubles = new();
        public readonly Dictionary<string, Vector2> Vec2s = new();
        public readonly Dictionary<string, Vector3> Vec3s = new();
        public readonly Dictionary<string, Vector4> Vec4s = new();
        public readonly Dictionary<string, Matrix4x4> Mat4s = new();

        public readonly Dictionary<string, IReadableTexture> Textures = new();

        public readonly Dictionary<string, int[]> IntegerArrays = new();
        public readonly Dictionary<string, float[]> FloatArrays = new();
        public readonly Dictionary<string, double[]> DoubleArrays = new();
        public readonly Dictionary<string, Vector3[]> Vec3Arrays = new();
        public readonly Dictionary<string, Matrix4x4[]> Matrix4x4Arrays = new();

        private static void SetValue<T>(Dictionary<string, T> dict, string key, T value)
        {
            if (!dict.TryAdd(key, value))
                dict[key] = value;
        }

        /// <summary>
        /// Set or add a float uniform
        /// </summary>
        public void SetValue(string name, float value) => SetValue(Floats, name, value);
        /// <summary>
        /// Set or add an int uniform
        /// </summary>
        public void SetValue(string name, int value) => SetValue(Integers, name, value);
        /// <summary>
        /// Set or add an int (really. bytes are just converted to ints) uniform
        /// </summary>
        public void SetValue(string name, byte value) => SetValue(Integers, name, value);
        /// <summary>
        /// Set or add a double uniform
        /// </summary>
        public void SetValue(string name, double value) => SetValue(Doubles, name, value);
        /// <summary>
        /// Set or add a Vector2 uniform
        /// </summary>
        public void SetValue(string name, Vector2 value) => SetValue(Vec2s, name, value);
        /// <summary>
        /// Set or add a Vector3 uniform
        /// </summary>
        public void SetValue(string name, Vector3 value) => SetValue(Vec3s, name, value);
        /// <summary>
        /// Set or add a Vector4 uniform
        /// </summary>
        public void SetValue(string name, Vector4 value) => SetValue(Vec4s, name, value);
        /// <summary>
        /// Set or add a Matrix4x4 uniform
        /// </summary>
        public void SetValue(string name, Matrix4x4 value) => SetValue(Mat4s, name, value);
        /// <summary>
        /// Set or add a IReadableTexture uniform
        /// </summary>
        public void SetValue(string name, IReadableTexture value) => SetValue(Textures, name, value);

        /// <summary>
        /// Set or add an int[] uniform
        /// </summary>
        public void SetValue(string name, int[] value) => SetValue(IntegerArrays, name, value);
        /// <summary>
        /// Set or add a float[] uniform
        /// </summary>
        public void SetValue(string name, float[] value) => SetValue(FloatArrays, name, value);
        /// <summary>
        /// Set or add a double[] uniform
        /// </summary>
        public void SetValue(string name, double[] value) => SetValue(DoubleArrays, name, value);
        /// <summary>
        /// Set or add a Vector3[] uniform
        /// </summary>
        public void SetValue(string name, Vector3[] value) => SetValue(Vec3Arrays, name, value);
        /// <summary>
        /// Set or add a Matrix4x4[] uniform
        /// </summary>
        public void SetValue(string name, Matrix4x4[] value) => SetValue(Matrix4x4Arrays, name, value);

        /// <summary>
        /// Try to get a byte uniform value
        /// </summary>
        public bool TryGetValue(string key, out byte v)
        {
            if (Integers.TryGetValue(key, out var b))
            {
                v = (byte)b;
                return true;
            }
            v = 0;
            return false;
        }
        /// <summary>
        /// Try to get an integer uniform value
        /// </summary>
        public bool TryGetValue(string key, out int v) => Integers.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a float uniform value
        /// </summary>
        public bool TryGetValue(string key, out float v) => Floats.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a double uniform value
        /// </summary>
        public bool TryGetValue(string key, out double v) => Doubles.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Vector2 uniform value
        /// </summary>
        public bool TryGetValue(string key, out Vector2 v) => Vec2s.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Vector3 uniform value
        /// </summary>
        public bool TryGetValue(string key, out Vector3 v) => Vec3s.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Vector4 uniform value
        /// </summary>
        public bool TryGetValue(string key, out Vector4 v) => Vec4s.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Matrix4x4 uniform value
        /// </summary>
        public bool TryGetValue(string key, out Matrix4x4 v) => Mat4s.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a IReadableTexture uniform value
        /// </summary>
        public bool TryGetValue(string key, out IReadableTexture? v) => Textures.TryGetValue(key, out v);

        /// <summary>
        /// Try to get an int[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out int[]? v) => IntegerArrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a float[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out float[]? v) => FloatArrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a double[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out double[]? v) => DoubleArrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Vector3[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out Vector3[]? v) => Vec3Arrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Matrix4x4[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out Matrix4x4[]? v) => Matrix4x4Arrays.TryGetValue(key, out v);

        /// <summary>
        /// Does the uniform with the given name exist?
        /// </summary>
        public bool ContainsKey(string name)
        {
            //TODO misschien een aparte hashset met alle uniform namen
            if (Textures.ContainsKey(name))
                return true;
            if (Integers.ContainsKey(name))
                return true;
            if (Floats.ContainsKey(name))
                return true;
            if (Doubles.ContainsKey(name))
                return true;
            if (Vec2s.ContainsKey(name))
                return true;
            if (Vec3s.ContainsKey(name))
                return true;
            if (Vec4s.ContainsKey(name))
                return true;
            if (Mat4s.ContainsKey(name))
                return true;
            if (IntegerArrays.ContainsKey(name))
                return true;
            if (FloatArrays.ContainsKey(name))
                return true;
            if (DoubleArrays.ContainsKey(name))
                return true;
            if (Vec3Arrays.ContainsKey(name))
                return true;
            if (Matrix4x4Arrays.ContainsKey(name))
                return true;
            return false;
        }
    }
}
