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
    public void Draw<TVertex>(VertexBuffer<TVertex> vertexBuffer, Material? material = null) where TVertex : struct;

    /// <summary>
    /// Draw a instanced vertex buffer to the currently activated target
    /// </summary>
    /// <param name="vertexBuffer">VertexBuffer to draw</param>
    /// <param name="instanceCount">Amount of elements to draw</param>
    /// <param name="material">Material to draw it with</param>
    public void DrawInstanced<TVertex>(VertexBuffer<TVertex> vertexBuffer, int instanceCount, Material? material = null) where TVertex : struct;

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
    /// Delete a VB from the GPU by its CPU representation
    /// </summary>
    public void Delete<TVertex>(VertexBuffer<TVertex> graphicsObject) where TVertex : struct;

    /// <summary>
    /// Forcibly upload a VB to the GPU if supported. Won't do anything if the VB was already there.
    /// </summary>
    public void Upload<TVertex>(VertexBuffer<TVertex> graphicsObject) where TVertex : struct;

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
    public int TryGetId<TVertex>(VertexBuffer<TVertex> graphicsObject, out int vertexBufferId, out int indexBufferId, out int vertexArrayId, ref int[] vertexAttributeIds) where TVertex : struct;

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

    /// <summary>
    /// Get a colour from a texture at a given coordinate in texture space
    /// </summary>
    /// <param name="tex"></param>
    /// <returns></returns>
    public Color SampleTexture(IReadableTexture tex, int x, int y);
}
