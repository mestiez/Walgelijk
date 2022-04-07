using System;

namespace Walgelijk.OpenTK
{
    public struct LoadedShader
    {
        public int VertexShaderHandle;
        public int FragmentShaderHandle;

        public override bool Equals(object obj)
        {
            return obj is LoadedShader shader &&
                   VertexShaderHandle == shader.VertexShaderHandle &&
                   FragmentShaderHandle == shader.FragmentShaderHandle;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VertexShaderHandle, FragmentShaderHandle);
        }

        public static bool operator ==(LoadedShader left, LoadedShader right)
        {
            return left.VertexShaderHandle == right.VertexShaderHandle &&
                   left.FragmentShaderHandle == right.FragmentShaderHandle;
        }

        public static bool operator !=(LoadedShader left, LoadedShader right)
        {
            return !(left == right);
        }
    }
}
