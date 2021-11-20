using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        private static void SetValue<T>(Dictionary<string, T> dict,string key, T value)
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
        public bool TryGetValue(string key, out IReadableTexture v) => Textures.TryGetValue(key, out v);

        /// <summary>
        /// Try to get an int[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out int[] v) => IntegerArrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a float[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out float[] v) => FloatArrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a double[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out double[] v) => DoubleArrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Vector3[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out Vector3[] v) => Vec3Arrays.TryGetValue(key, out v);
        /// <summary>
        /// Try to get a Matrix4x4[] uniform value
        /// </summary>
        public bool TryGetValue(string key, out Matrix4x4[] v) => Matrix4x4Arrays.TryGetValue(key, out v);

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

    /// <summary>
    /// Object that holds unique information specific to a shader
    /// </summary>
    [Serializable]
    public sealed class Material
    {
        private const string NoGameExceptionText = "There is no main instance of Game. Setting uniforms can only be done once a game is running";

        /// <summary>
        /// The shader this material uses
        /// </summary>
        public Shader Shader { get; set; } = Shader.Default;

        /// <summary>
        /// Access the CPU side copy of the uniforms. Only use if you know what you're doing
        /// </summary>
        public readonly UniformDictionary InternalUniforms = new();

        /// <summary>
        /// Create a material with a shader
        /// </summary>
        /// <param name="shader"></param>
        public Material(Shader shader)
        {
            Shader = shader;
        }

        /// <summary>
        /// New instance of the default shader
        /// </summary>
        public Material()
        {
            Shader = Shader.Default;
        }

        /// <summary>
        /// New instance of an existing material. T
        /// </summary>
        public Material(Material toCopy)
        {
            Shader = toCopy.Shader;
            foreach (var item in toCopy.InternalUniforms.Textures) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Integers) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Floats) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Doubles) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Vec2s) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Vec3s) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Vec4s) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Mat4s) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.IntegerArrays) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.FloatArrays) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.DoubleArrays) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Vec3Arrays) SetUniform(item.Key, item.Value);
            foreach (var item in toCopy.InternalUniforms.Matrix4x4Arrays) SetUniform(item.Key, item.Value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, IReadableTexture value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, int value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, float value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, double value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, Vector2 value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, Vector3 value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, Vector4 value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, Matrix4x4 value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, int[] value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, float[] value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, double[] value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, Vector3[] value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        public void SetUniform(string name, Matrix4x4[] value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException(NoGameExceptionText);
            InternalUniforms.SetValue(name, value);
            Game.Main.Window.Graphics.SetUniform(this, name, value);
        }

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out IReadableTexture value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out int value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out float value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out double value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out Vector2 value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out Vector3 value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out Vector4 value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out Matrix4x4 value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out int[] value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out float[] value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out double[] value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out Vector3[] value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out Matrix4x4[] value) => InternalUniforms.TryGetValue(name, out value);

        /// <summary>
        /// Returns whether a uniform with the given name has been registered in the material
        /// </summary>
        public bool HasUniform(string name)
        {
            return InternalUniforms.ContainsKey(name);
        }

        /// <summary>
        /// The default material with the default shader. This material is shared.
        /// </summary>
        public static Material DefaultTextured => DefaultMaterialInitialiser.GetMaterial();
    }
}
