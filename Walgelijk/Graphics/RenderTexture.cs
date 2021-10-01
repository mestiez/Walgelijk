using System;
using System.Collections.Immutable;
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
        public RenderTexture(int width, int height, WrapMode wrapMode = WrapMode.Clamp, FilterMode filterMode = FilterMode.Linear, bool generateMipmaps = false, bool hdr = false)
        {
            this.size = new Vector2(width, height);
            this.wrapMode = wrapMode;
            this.filterMode = filterMode;
            GenerateMipmaps = generateMipmaps;
            HDR = hdr;
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

        public bool GenerateMipmaps { get; }

        public bool HDR { get; }

        public Color GetPixel(int x, int y) => throw new global::System.Exception("You can't get pixels from a RenderTexture");

        public ImmutableArray<Color>? ReadPixels() => null;

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
    }
}
