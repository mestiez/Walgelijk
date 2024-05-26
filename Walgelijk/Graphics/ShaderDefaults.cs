using System;

namespace Walgelijk;

/// <summary>
/// Useful values for shader related business 
/// </summary>
public struct ShaderDefaults
{
    /// <summary>
    /// Default fragment shader code
    /// </summary>
    [Obsolete("Use BuiltInShaders")]
    public static string TexturedFragment => BuiltInShaders.TexturedFragment;
    /// <summary>
    /// Default vertex shader code
    /// </summary>
    [Obsolete("Use BuiltInShaders")]
    public static string WorldSpaceVertex = BuiltInShaders.WorldSpaceVertex;

    /// <summary>
    /// Projection matrix uniform name
    /// </summary>
    public const string ProjectionMatrixUniform = "projection";
    /// <summary>
    /// View matrix uniform name
    /// </summary>
    public const string ViewMatrixUniform = "view";
    /// <summary>
    /// Model matrix uniform name
    /// </summary>
    public const string ModelMatrixUniform = "model";
    /// <summary>
    /// Main texture uniform name
    /// </summary>
    public const string MainTextureUniform = "mainTex";
}
