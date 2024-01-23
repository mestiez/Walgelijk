using System;

namespace Walgelijk;

/// <summary>
/// Graphics utility interface meant to be implemented by the rendering implementation
/// </summary>
public interface IGraphics
{
    /// <summary>
    /// Clear current target
    /// </summary>
    public void Clear(Color color);

    /// <summary>
    /// Draw a vertex buffer to the currently activated target
    /// </summary>
    /// <param name="vertexBuffer">VertexBuffer to draw</param>
    /// <param name="material">Material to draw it with</param>
    public void Draw(VertexBuffer vertexBuffer, Material? material = null);

    /// <summary>
    /// Draw a instanced vertex buffer to the currently activated target
    /// </summary>
    /// <param name="vertexBuffer">VertexBuffer to draw</param>
    /// <param name="instanceCount">Amount of elements to draw</param>
    /// <param name="material">Material to draw it with</param>
    public void DrawInstanced(VertexBuffer vertexBuffer, int instanceCount, Material? material = null);

    /// <summary>
    /// Set a shader program uniform
    /// </summary>
    public void SetUniform<T>(Material material, string uniformName, T data);

    /// <summary>
    /// Drawing bounds settings 
    /// </summary>
    public DrawBounds DrawBounds { get; set; }

    /// <summary>
    /// Set or get the currently active target
    /// </summary>
    public RenderTarget CurrentTarget { get; set; }

    /// <summary>
    /// Delete an object from the GPU by its CPU representation
    /// </summary>
    public void Delete(object obj);

    /// <summary>
    /// Forcibly upload an object to the GPU if supported. Won't do anything if the object was already there.
    /// </summary>
    public void Upload(object obj);

    /// <summary>
    /// Save a texture to disk as a PNG
    /// </summary>
    public void SaveTexture(global::System.IO.FileStream output, IReadableTexture texture);

    /// <summary>
    /// Try to get the ID of the given graphics object. 
    /// Returns false if the given object could not be found, true otherwise.
    /// </summary>
    public bool TryGetId(RenderTexture graphicsObject, out int frameBufferId, out int[] textureId);

    /// <summary>
    /// Try to get the ID of the given graphics object. 
    /// Returns false if the given object could not be found, true otherwise.
    /// </summary>
    public bool TryGetId(IReadableTexture graphicsObject, out int textureId);

    /// <summary>
    /// Try to get the ID of the given graphics object. 
    /// <paramref name="vertexAttributeIds"/> expects an array of integers. It will be filled with the extra attribute IDs if they exist.
    /// Returns -1 if the given object could not be found, otherwise it returns the amount of extra vertex attribute IDs (clamped to the lenghth of the given array if applicable).
    /// </summary>
    public int TryGetId(VertexBuffer graphicsObject, out int vertexBufferId, out int indexBufferId, out int vertexArrayId, ref int[] vertexAttributeIds);

    /// <summary>
    /// Try to get the ID of the given graphics object. 
    /// Returns false if the given object could not be found, true otherwise.
    /// </summary>
    public bool TryGetId(Material graphicsObject, out int id);

    /// <summary>
    /// Blit a <see cref="RenderTexture"/> onto another
    /// </summary>
    public void Blit(RenderTexture source, RenderTexture destination);

    /// <summary>
    /// Access the stencil buffer state
    /// </summary>
    public StencilState Stencil { get; set; }
}

public struct StencilState : IEquatable<StencilState>
{
    /// <summary>
    /// Is stencil writing/reading enabled at all? If false, the rest of the fields won't do anything.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// If true, the stencil buffer will be cleared to all zeroes
    /// </summary>
    public bool ShouldClear;

    /// <summary>
    /// The current stencil access mode, see <see cref="StencilAccessMode"/>
    /// </summary>
    public StencilAccessMode AccessMode;
    public StencilTestMode TestMode;

    /// <summary>
    /// Disable stencil testing and writing
    /// </summary>
    public static StencilState Disabled => new() { Enabled = false };

    /// <summary>
    /// Clear the stencil buffer to all zeroes
    /// </summary>
    public static StencilState Clear => new() { Enabled = true, ShouldClear = true };

    /// <summary>
    /// Any geometry drawn will result in 1s on the stencil buffer, i.e determines the mask
    /// </summary>
    public static StencilState WriteMask => new() { Enabled = true, AccessMode = StencilAccessMode.Write };

    /// <summary>
    /// Fragments will only be drawn if the stencil buffer at that point is 1, i.e inside the mask
    /// </summary>
    public static StencilState InsideMask => new() { Enabled = true, AccessMode = StencilAccessMode.NoWrite, TestMode = StencilTestMode.Inside };

    /// <summary>
    /// Fragments will only be drawn if the stencil buffer at that point is 0, i.e outside the mask
    /// </summary>
    public static StencilState OutsideMask => new() { Enabled = true, AccessMode = StencilAccessMode.NoWrite, TestMode = StencilTestMode.Outside };

    public override bool Equals(object? obj)
    {
        return obj is StencilState state && Equals(state);
    }

    public bool Equals(StencilState other)
    {
        return Enabled == other.Enabled &&
               ShouldClear == other.ShouldClear &&
               AccessMode == other.AccessMode &&
               TestMode == other.TestMode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, ShouldClear, AccessMode, TestMode);
    }

    public static bool operator ==(StencilState left, StencilState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StencilState left, StencilState right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Stencil access mode. Defines ways for drawn geometry to interact with the stencil buffer
/// </summary>
public enum StencilAccessMode
{
    // glStencilMask(0x00);
    /// <summary>
    /// Only read from the stencil buffer
    /// </summary>
    NoWrite,
    // glStencilMask(0xFF);
    /// <summary>
    /// Write 1 to the stencil buffer
    /// </summary>
    Write,
}

public enum StencilTestMode
{
    // glStencilFunc(GL_EQUAL, 1, 0xFF)
    /// <summary>
    /// Only draw inside the mask, i.e where the stencil buffer is set to 1
    /// </summary>
    Inside,
    // glStencilFunc(GL_NOTEQUAL, 1, 0xFF)
    /// <summary>
    /// Only draw outside the mask, i.e where the stencil buffer is set to 0
    /// </summary>
    Outside
}