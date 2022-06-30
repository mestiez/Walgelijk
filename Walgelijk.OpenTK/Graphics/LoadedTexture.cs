using OpenTK.Graphics.OpenGL;
using System;

namespace Walgelijk.OpenTK
{
    public struct LoadedTexture
    {
        public LoadedTexture(int width, int height, int handle)
        {
            Width = width;
            Height = height;
            Index = handle;
        }

        public int Width { get; }
        public int Height { get; }
        public int Index { get; }

        public override bool Equals(object obj)
        {
            return obj is LoadedTexture texture &&
                   Width == texture.Width &&
                   Height == texture.Height &&
                   Index == texture.Index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height, Index);
        }

        public static bool operator ==(LoadedTexture left, LoadedTexture right)
        {
            return left.Width == right.Width &&
                   left.Height == right.Height &&
                   left.Index == right.Index;
        }

        public static bool operator !=(LoadedTexture left, LoadedTexture right)
        {
            return !(left == right);
        }

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

        public override bool Equals(object obj)
        {
            return obj is TextureUnitLink other &&
                   Texture.Index == other.Texture.Index &&
                   Unit == other.Unit;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Texture, Unit);
        }

        public static bool operator ==(TextureUnitLink left, TextureUnitLink right)
        {
            return left.Texture.Index == right.Texture.Index &&
                   left.Unit == right.Unit;
        }

        public static bool operator !=(TextureUnitLink left, TextureUnitLink right)
        {
            return !(left == right);
        }
    }
}
