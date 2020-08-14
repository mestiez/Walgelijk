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
            Handles handles = vertexBufferCache.Load(vertexBuffer);

            GL.BindVertexArray(handles.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);

            GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public override void Draw(Vertex[] vertices, Primitive primitive, Material material = null)
        {
            GL.Begin(TypeConverter.Convert(primitive));

            foreach (var item in vertices)
            {
                GL.Color4(item.Color.R, item.Color.G, item.Color.B, item.Color.A);
                GL.TexCoord2(item.TexCoords.X, item.TexCoords.Y);
                GL.Vertex3(item.Position.X, item.Position.Y, item.Position.Z);
            }

            GL.End();
        }
    }
}
