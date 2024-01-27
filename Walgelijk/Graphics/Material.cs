using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Object that holds unique information specific to a shader
/// </summary>
[Serializable]
public sealed class Material : IDisposable
{
    private const string NoGameExceptionText = "There is no main instance of Game. Setting uniforms can only be done once a game is running";
    private bool disposed = false;

    /// <summary>
    /// The shader this material uses
    /// </summary>
    public Shader Shader { get; set; } = Shader.Default;

    /// <summary>
    /// Blend mode to use for this material
    /// </summary>
    public BlendMode BlendMode { get; set; } = BlendMode.AlphaBlend;

    /// <summary>
    /// Optional stencil state to set before drawing this material. If null, it will retain the previous stencil state.
    /// </summary>
    public StencilState? StencilState { get; set; }

    /// <summary>
    /// Depth testing mode. If true, the fragments will undergo depth testing. If false, they will ignore the depth buffer. If the target does not have a depth buffer, this value will have no effect.
    /// </summary>
    public bool DepthTested { get; set; } = false;

    /// <summary>
    /// The back-facing faces will be culled if this is true
    /// </summary>
    public bool BackfaceCulling { get; set; } = false;

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
    public bool TryGetUniform(string name, out IReadableTexture? value) => InternalUniforms.TryGetValue(name, out value);

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
    public bool TryGetUniform(string name, out int[]? value) => InternalUniforms.TryGetValue(name, out value);

    /// <summary>
    /// Try to get the value of a uniform
    /// </summary>
    /// <returns>True if the uniform exists</returns>
    public bool TryGetUniform(string name, out float[]? value) => InternalUniforms.TryGetValue(name, out value);

    /// <summary>
    /// Try to get the value of a uniform
    /// </summary>
    /// <returns>True if the uniform exists</returns>
    public bool TryGetUniform(string name, out double[]? value) => InternalUniforms.TryGetValue(name, out value);

    /// <summary>
    /// Try to get the value of a uniform
    /// </summary>
    /// <returns>True if the uniform exists</returns>
    public bool TryGetUniform(string name, out Vector3[]? value) => InternalUniforms.TryGetValue(name, out value);

    /// <summary>
    /// Try to get the value of a uniform
    /// </summary>
    /// <returns>True if the uniform exists</returns>
    public bool TryGetUniform(string name, out Matrix4x4[]? value) => InternalUniforms.TryGetValue(name, out value);

    /// <summary>
    /// Returns whether a uniform with the given name has been registered in the material
    /// </summary>
    public bool HasUniform(string name)
    {
        return InternalUniforms.ContainsKey(name);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Game.Main?.Window?.Graphics?.Delete(this);
            GC.SuppressFinalize(this);
        }
        disposed = true;
    }

    /// <summary>
    /// The default material with the default shader. This material is shared.
    /// </summary>
    public static Material DefaultTextured => DefaultMaterialInitialiser.GetMaterial();

    /// <summary>
    /// Creates a new material that just renders the single colour
    /// </summary>
    public static Material CreateSingleColour(Color col) => DefaultMaterialInitialiser.GetSingleColour(col);
}
