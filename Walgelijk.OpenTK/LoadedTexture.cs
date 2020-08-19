using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    public struct LoadedTexture
    {
        public LoadedTexture(byte[] data, int width, int height, TextureUnit textureUnit, int index)
        {
            Data = data;
            Width = width;
            Height = height;
            TextureUnit = textureUnit;
            Index = index;
        }

        public byte[] Data { get; }
        public int Width { get; }
        public int Height { get; }
        public TextureUnit TextureUnit { get; }
        public int Index { get; }

        public void Bind()
        {
            GL.ActiveTexture(TextureUnit);
            GL.BindTexture(TextureTarget.Texture2D, Index);
        }
    }
}
