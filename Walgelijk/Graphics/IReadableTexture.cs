using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Objects that have data on the CPU side that is eventually uploaded to the GPU side
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGPUObject<T>
    {
        /// <summary>
        /// Remove the copy of the data that is stored on the CPU, usually because it's already been uploaded to the GPU
        /// </summary>
        public void DisposeCPUCopy();

        /// <summary>
        /// Get the data. This can be null if it's been disposed.
        /// </summary>
        public ReadOnlyMemory<T>? GetData();

        /// <summary>
        /// Should the cpu copy of this object be disposed after it's been uploaded? 
        /// </summary>
        public bool DisposeCPUCopyAfterUpload { get; }
    }

    /// <summary>
    /// A texture that can be written to
    /// </summary>
    public interface IReadableTexture : IDisposable, IGPUObject<Color>
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
        public ReadOnlySpan<Color> ReadPixels() => (GetData() ?? ReadOnlyMemory<Color>.Empty).Span;

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
        public virtual Vector2 Size => new(Width, Height);

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
        public bool NeedsUpdate { get; set; }
    }
}
