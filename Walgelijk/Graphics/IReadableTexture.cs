using System.Collections.Immutable;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// A texture that can be written to
    /// </summary>
    public interface IReadableTexture
    {
        /// <summary>
        /// Get a pixel
        /// </summary>
        /// <returns></returns>
        public Color GetPixel(int x, int y);

        /// <summary>
        /// Get an immutable array of all pixels
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<Color>? ReadPixels();

        /// <summary>
        /// Width of the texture in pixels
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the texture in pixels
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Size of the image. This returns a <see cref="Vector2"/> with <see cref="Width"/> and <see cref="Height"/>
        /// </summary>
        public virtual Vector2 Size => new Vector2(Width, Height);

        /// <summary>
        /// Wrap mode
        /// </summary>
        public WrapMode WrapMode { get; }

        /// <summary>
        /// Filter mode. Determines how pixels are interpolated between
        /// </summary>
        public FilterMode FilterMode { get; }

        /// <summary>
        /// Whether the texture can store HDR image data
        /// </summary>
        public bool HDR { get; }

        /// <summary>
        /// Whether the texture has generated mipmaps upon load
        /// </summary>
        public bool GenerateMipmaps { get; }

        /// <summary>
        /// Whether or not the renderer needs to send new information to the GPU
        /// </summary>
        public bool NeedsUpdate { get; set;  }
    }
}
