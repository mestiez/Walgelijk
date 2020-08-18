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
    }
}
