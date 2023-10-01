using OpenTK.Graphics.OpenGL4;

namespace Walgelijk.OpenTK
{
    internal struct AttributeTypeInfoLookup
    {
        public static int GetStride(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Integer:
                    return sizeof(int);
                case AttributeType.Float:
                    return sizeof(float);
                case AttributeType.Double:
                    return sizeof(double);
                case AttributeType.Vector2:
                    return sizeof(float) * 2;
                case AttributeType.Vector3:
                    return sizeof(float) * 3;
                case AttributeType.Vector4:
                    return sizeof(float) * 4;
                case AttributeType.Matrix4x4:
                    return sizeof(float) * 16;
            }

            return 0;
        }

        public static int GetSize(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Integer:
                case AttributeType.Float:
                case AttributeType.Double:
                    return 1;
                case AttributeType.Vector2:
                    return 2;
                case AttributeType.Vector3:
                    return 3;
                case AttributeType.Vector4:
                    return 4;
                case AttributeType.Matrix4x4:
                    return 4;
            }

            return 0;
        }

        public static VertexAttribPointerType GetPointerType(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Integer:
                    return VertexAttribPointerType.Int;
                case AttributeType.Float:
                case AttributeType.Vector2:
                case AttributeType.Vector3:
                case AttributeType.Vector4:
                case AttributeType.Matrix4x4:
                    return VertexAttribPointerType.Float;
                case AttributeType.Double:
                    return VertexAttribPointerType.Double;
            }

            return VertexAttribPointerType.Float;
        }
    }
}
