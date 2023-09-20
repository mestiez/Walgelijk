using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// A render target of an arbitrary size that can be used as a texture
    /// </summary>
    public class RenderTexture : RenderTarget, IReadableTexture, IDisposable
    {
        private Vector2 size;
        private WrapMode wrapMode;
        private FilterMode filterMode;

        /// <summary>
        /// Construct a <see cref="RenderTexture"/>
        /// </summary>
        [Obsolete("Use the constructor that takes RenderTextureFlags")]
        public RenderTexture(int width, int height, WrapMode wrapMode = WrapMode.Clamp, FilterMode filterMode = FilterMode.Linear, bool generateMipmaps = false, bool hdr = false)
        {
            this.size = new Vector2(width, height);
            this.wrapMode = wrapMode;
            this.filterMode = filterMode;
            if (generateMipmaps)
                Flags |= RenderTextureFlags.Mipmaps;
            if (hdr)
                Flags |= RenderTextureFlags.HDR;
        }      
        
        public RenderTexture(int width, int height, WrapMode wrapMode = WrapMode.Clamp, FilterMode filterMode = FilterMode.Linear, RenderTextureFlags flags = RenderTextureFlags.None)
        {
            this.size = new Vector2(width, height);
            this.wrapMode = wrapMode;
            this.filterMode = filterMode;
            Flags = flags;
        }

        public override Vector2 Size
        {
            get => size;
            set
            {
                if (value != size)
                    NeedsUpdate = true;

                size = value;
            }
        }

        public int Width => (int)Size.X;
        public int Height => (int)Size.Y;
        public readonly RenderTextureFlags Flags;

        public WrapMode WrapMode
        {
            get => wrapMode;
            set
            {
                wrapMode = value;
                if (value != wrapMode)
                    NeedsUpdate = true;
            }
        }

        public FilterMode FilterMode
        {
            get => filterMode;
            set
            {
                filterMode = value;
                if (value != filterMode)
                    NeedsUpdate = true;
            }
        }

        public bool NeedsUpdate { get; set; }

        [Obsolete("Use Flags")]
        public bool GenerateMipmaps => Flags.HasFlag(RenderTextureFlags.Mipmaps);
        [Obsolete("Use Flags")]
        public bool HDR => Flags.HasFlag(RenderTextureFlags.HDR);

        /// <summary>
        /// There is no local copy so this will do nothing.
        /// </summary>
        public bool DisposeLocalCopyAfterUpload => false;

        /// <summary>
        /// You can't get pixels from a RenderTexture
        /// </summary>
        public Color GetPixel(int x, int y) => throw new global::System.Exception("You can't get pixels from a RenderTexture");

        /// <summary>
        /// You can't get pixels from a RenderTexture. This will return an empty span.
        /// </summary>
        public ReadOnlySpan<Color> ReadPixels() => ReadOnlySpan<Color>.Empty;

        /// <summary>
        /// Force an update
        /// </summary>
        public void ForceUpdate()
        {
            NeedsUpdate = true;
        }

        /// <summary>
        /// Delete the texture and framebuffer from the GPU
        /// </summary>
        public void Dispose()
        {
            Game.Main.Window.Graphics.Delete(this);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// There is no local copy so this will do nothing.
        /// </summary>
        public void DisposeLocalCopy() { }

        /// <summary>
        /// You can't get pixels from a RenderTexture. This will return a region of zero length.
        /// </summary>
        public ReadOnlyMemory<Color>? GetData() => ReadOnlyMemory<Color>.Empty;
    }
}
