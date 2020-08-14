using OpenTK.Graphics.OpenGL;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class OpenTKRenderTarget : Walgelijk.RenderTarget
    {
        private Vector2 size;
        private Color clearColour;
        private VertexBufferCache vertexBufferCache = new VertexBufferCache();

        public override Vector2 Size
        {
            get => size;

            set
            {
                size = value;
                GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            }
        }
        public override Color ClearColour
        {
            get => clearColour;

            set
            {
                clearColour = value;
                GL.ClearColor(ClearColour.R, ClearColour.G, ClearColour.B, ClearColour.A);
            }
        }

        public override void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public override void Draw(VertexBuffer vertexBuffer, Material material)
        {
            if (material == null)
            {

            }
            Handles handles = vertexBufferCache.Load(vertexBuffer);

            GL.BindVertexArray(handles.VAO);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, handles.VBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);

            GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}
