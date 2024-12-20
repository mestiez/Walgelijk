﻿using OpenTK.Graphics.OpenGL4;

namespace Walgelijk.OpenTK
{
    public struct TypeConverter
    {
        public static PrimitiveType Convert(Primitive primitive) => primitive switch
        {
            Primitive.Points => PrimitiveType.Points,
            Primitive.Lines => PrimitiveType.Lines,
            Primitive.LineLoop => PrimitiveType.LineLoop,
            Primitive.LineStrip => PrimitiveType.LineStrip,
            Primitive.Triangles => PrimitiveType.Triangles,
            Primitive.TriangleStrip => PrimitiveType.TriangleStrip,
            Primitive.TriangleFan => PrimitiveType.TriangleFan,
            _ => PrimitiveType.Triangles,
        };

        public static Key Convert(global::OpenTK.Windowing.GraphicsLibraryFramework.Keys key)
        {
            return (Key)(int)key;
        }

        public static MouseButton Convert(global::OpenTK.Windowing.GraphicsLibraryFramework.MouseButton button)
        {
            return (MouseButton)(int)button;
        }

        public static int Convert(TextureUnit textureUnit)
        {
            return (int)textureUnit - (int)TextureUnit.Texture0;
        }

        public static TextureUnit Convert(int textureUnit)
        {
            return (TextureUnit)(textureUnit + (int)TextureUnit.Texture0);
        }

        public static WrapMode Convert(TextureWrapMode mode) => mode switch
        {
            TextureWrapMode.Repeat => WrapMode.Repeat,
            TextureWrapMode.ClampToBorder or TextureWrapMode.ClampToEdge => WrapMode.Clamp,
            TextureWrapMode.MirroredRepeat => WrapMode.Mirror,
            _ => default,
        };

        public static TextureWrapMode Convert(WrapMode mode) => mode switch
        {
            WrapMode.Clamp => TextureWrapMode.ClampToBorder,
            WrapMode.Repeat => TextureWrapMode.Repeat,
            WrapMode.Mirror => TextureWrapMode.MirroredRepeat,
            _ => default,
        };

        public static int Convert(FilterMode filter) => filter switch
        {
            FilterMode.Nearest => (int)TextureMinFilter.Nearest,
            FilterMode.Linear => (int)TextureMinFilter.Linear,
            _ => default,
        };
    }
}
