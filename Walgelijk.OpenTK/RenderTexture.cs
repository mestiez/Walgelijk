using System.Numerics;

namespace Walgelijk.OpenTK
{
    public class RenderTexture : RenderTarget
    {
        private Vector2 size;

        public override Vector2 Size
        {
            get => size;
            set
            {
                if (value != size)
                    HasChangedSize = true;

                size = value;
            }
        }

        internal Texture Texture { get; private set; }
        public bool HDR { get; }

        public RenderTexture(Vector2 size, bool hdr = false, FilterMode filterMode = FilterMode.Linear, WrapMode wrapMode = WrapMode.Clamp)
        {
            this.size = size;

            Texture = new Texture((int)size.X, (int)size.Y, false);

            HDR = hdr;
            Texture.FilterMode = filterMode;
            Texture.WrapMode = wrapMode;
        }

        //TODO moet aangeven dat er een update moet worden gedaan
        internal bool HasChangedSize = false;
    }
}
