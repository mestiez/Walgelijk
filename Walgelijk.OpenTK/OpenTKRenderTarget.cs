using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            int vao = vertexBufferCache.Load(vertexBuffer);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexBuffer.Vertices.Length);


            //GL.Begin(PrimitiveType.Triangles);
            //for (int i = 0; i < vertexBuffer.Indices.Length; i++)
            //{
            //    var vertex = vertexBuffer.Vertices[vertexBuffer.Indices[i]];
            //    GL.Color4(vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
            //    GL.Vertex3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
            //}
            //GL.End();
        }
    }

    public class VertexBufferCache
    {
        private Dictionary<VertexBuffer, int> vertexBuffers = new Dictionary<VertexBuffer, int>();

        public int Load(VertexBuffer buffer)
        {
            if (vertexBuffers.TryGetValue(buffer, out int handle))
            {
                TryUpdateBuffer(buffer, handle);
                return handle;
            }

            return CreateBuffer(buffer);
        }

        private int CreateBuffer(VertexBuffer buffer)
        {
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Vertices.Length * Vertex.Stride), buffer.Vertices, BufferUsageHint.StaticDraw);

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
             
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.Stride, sizeof(float) * 3);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vertex.Stride, sizeof(float) * 6);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            vertexBuffers.Add(buffer, vao);
            buffer.HasChanged = false;
            return vao;
        }

        private void TryUpdateBuffer(VertexBuffer buffer, int handle)
        {
            if (!buffer.HasChanged) return;
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Vertices.Length * Vertex.Stride), buffer.Vertices, BufferUsageHint.StaticDraw);
        }
    }
}
