using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    public struct LoadedTexture
    {
        public LoadedTexture(byte[] data, int width, int height, int handle)
        {
            Data = data;
            Width = width;
            Height = height;
            Index = handle;
        }

        public byte[] Data { get; }
        public int Width { get; }
        public int Height { get; }
        public int Index { get; }

        //public void Bind()
        //{
        //    GL.ActiveTexture(TextureUnit);
        //    GL.BindTexture(TextureTarget.Texture2D, Index);
        //}
    }

    public struct TextureUnitLink
    {
        public LoadedTexture Texture;
        public TextureUnit Unit;

        public TextureUnitLink(LoadedTexture texture, TextureUnit unit)
        {
            Texture = texture;
            Unit = unit;
        }

        public void Bind()
        {
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D, Texture.Index);
        }
    }
}
