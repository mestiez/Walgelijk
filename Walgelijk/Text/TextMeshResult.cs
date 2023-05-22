namespace Walgelijk;

/// <summary>
/// Contains information about the results of text mesh generation
/// </summary>
public struct TextMeshResult
{
    /// <summary>
    /// Amount of glyphs actually generated
    /// </summary>
    public int GlyphCount;

    /// <summary>
    /// Amount of vertices actually generated
    /// </summary>
    public int VertexCount;

    /// <summary>
    /// Amount of indices actually generated
    /// </summary>
    public int IndexCount;

    /// <summary>
    /// Resulting local bounding box of the text mesh
    /// </summary>
    public Rect LocalBounds;

    /// <summary>
    /// Resulting local bounding box of the text, taking the line height and amount of lines into account
    /// </summary>
    public Rect LocalTextBounds;
}
