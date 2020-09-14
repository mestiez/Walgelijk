using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    public struct TypeConverter
    {
        public static PrimitiveType Convert(Primitive primitive)
        {
            return primitive switch
            {
                Primitive.Points => PrimitiveType.Points,
                Primitive.Lines => PrimitiveType.Lines,
                Primitive.LineLoop => PrimitiveType.LineLoop,
                Primitive.LineStrip => PrimitiveType.LineStrip,
                Primitive.Triangles => PrimitiveType.Triangles,
                Primitive.TriangleStrip => PrimitiveType.TriangleStrip,
                Primitive.TriangleFan => PrimitiveType.TriangleFan,
                Primitive.Quads => PrimitiveType.Quads,
                Primitive.QuadStrip => PrimitiveType.QuadStrip,
                Primitive.Polygon => PrimitiveType.Polygon,
                _ => PrimitiveType.Triangles,
            };
        }

        public static Key Convert(global::OpenTK.Input.Key key)
        {
            return (Key)(int)key;
        }

        public static Button Convert(global::OpenTK.Input.MouseButton button)
        {
            return (Button)(int)button;
        }

        public static int Convert(TextureUnit textureUnit)
        {
            return (int)textureUnit - (int)TextureUnit.Texture0;
        }

        public static TextureUnit Convert(int textureUnit)
        {
            return (TextureUnit)(textureUnit + (int)TextureUnit.Texture0);
        }

        public static WrapMode Convert(TextureWrapMode mode)
        {
            switch (mode)
            {
                case TextureWrapMode.Repeat:
                    return WrapMode.Repeat;

                case TextureWrapMode.Clamp:
                case TextureWrapMode.ClampToBorder:
                case TextureWrapMode.ClampToEdge:
                    return WrapMode.Clamp;

                case TextureWrapMode.MirroredRepeat:
                    return WrapMode.Mirror;

                default:
                    return default;
            }
        }

        public static TextureWrapMode Convert(WrapMode mode)
        {
            switch (mode)
            {
                case WrapMode.Clamp:
                    return TextureWrapMode.Clamp;
                case WrapMode.Repeat:
                    return TextureWrapMode.Repeat;
                case WrapMode.Mirror:
                    return TextureWrapMode.MirroredRepeat;
                default:
                    return default;
            }
        }

        public static int Convert(FilterMode filter)
        {
            switch (filter)
            {
                case FilterMode.Nearest:
                    return (int)TextureMinFilter.Nearest;
                case FilterMode.Linear:
                    return (int)TextureMinFilter.Linear;
                default:
                    return default;
            }
        }
    }
}
