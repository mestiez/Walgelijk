using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    public class VertexBufferCache
    {
        private Dictionary<VertexBuffer, VertexBufferCacheHandles> vertexBuffers = new Dictionary<VertexBuffer, VertexBufferCacheHandles>();

        public VertexBufferCacheHandles Load(VertexBuffer buffer)
        {
            if (vertexBuffers.TryGetValue(buffer, out VertexBufferCacheHandles handles))
            {
                TryUpdateBuffer(buffer, handles);
                return handles;
            }

            return CreateBuffer(buffer);
        }

        private VertexBufferCacheHandles CreateBuffer(VertexBuffer buffer)
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


            int ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(buffer.IndexCount * sizeof(uint)), buffer.Indices, BufferUsageHint.StaticDraw);

            VertexBufferCacheHandles handles = new VertexBufferCacheHandles(vbo, vao, ibo);

            buffer.HasChanged = false;
            vertexBuffers.Add(buffer, handles);
            return handles;
        }

        private void TryUpdateBuffer(VertexBuffer buffer, VertexBufferCacheHandles handles)
        {
            if (!buffer.HasChanged) return;
            //upload vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, handles.VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Vertices.Length * Vertex.Stride), buffer.Vertices, BufferUsageHint.StaticDraw);

            //upload indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(buffer.IndexCount * sizeof(uint)), buffer.Indices, BufferUsageHint.StaticDraw);
            buffer.HasChanged = false;
        }
    }
}
