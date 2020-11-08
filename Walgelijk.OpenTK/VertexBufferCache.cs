using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.OpenTK
{
    public class VertexBufferCache : Cache<VertexBuffer, VertexBufferCacheHandles>
    {
        protected override VertexBufferCacheHandles CreateNew(VertexBuffer buffer)
        {
            int vao = CreateVertexArrayObject(buffer);

            int vbo = CreateVertexBufferObject(buffer);
            int ibo = CreateIndexBufferObject(buffer);
            int[] extraVbos = CreateExtraVBO(buffer);

            ConfigureAttributes(buffer, vbo, extraVbos);

            VertexBufferCacheHandles handles = new VertexBufferCacheHandles(vbo, vao, ibo, extraVbos);

            UpdateBuffer(buffer, handles);
            UpdateExtraData(buffer, handles);

            buffer.HasChanged = false;

            return handles;
        }

        private static int[] CreateExtraVBO(VertexBuffer buffer)
        {
            int[] ids = new int[buffer.ExtraAttributeCount];

            for (int i = 0; i < buffer.ExtraAttributeCount; i++)
            {
                var array = buffer.GetAttribute(i);
                if (array == null) continue;

                int vbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

                ids[i] = vbo;
            }

            return ids;
        }

        private static void UploadCustomDataArray(VertexAttributeArray array, IntPtr size, BufferUsageHint hint)
        {
            switch (array.AttributeType)
            {
                case AttributeType.Integer:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (int[])array.Data, hint);
                    break;
                case AttributeType.Float:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (float[])array.Data, hint);
                    break;
                case AttributeType.Double:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (double[])array.Data, hint);
                    break;
                case AttributeType.Vector2:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (Vector2[])array.Data, hint);
                    break;
                case AttributeType.Vector3:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (Vector3[])array.Data, hint);
                    break;
                case AttributeType.Vector4:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (Vector4[])array.Data, hint);
                    break;
                case AttributeType.Matrix4x4:
                    GL.BufferData(BufferTarget.ArrayBuffer, size, (Matrix4x4[])array.Data, hint);
                    break;
            }
        }

        private static int CreateIndexBufferObject(VertexBuffer buffer)
        {
            int ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            return ibo;
        }

        private static int CreateVertexArrayObject(VertexBuffer buffer)
        {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            return vao;
        }

        private static void ConfigureAttributes(VertexBuffer buffer, int VBO, int[] extraDataObjects)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, sizeof(float) * 0); //position
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.Stride, sizeof(float) * 3); //texcoords
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vertex.Stride, sizeof(float) * 5); //color

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            int location = 3;
            for (int i = 0; i < buffer.ExtraAttributeCount; i++)
            {
                var array = buffer.GetAttribute(i);
                if (array == null)
                {
                    Logger.Error($"Null vertex attribute array at [{i}]");
                    continue;
                }

                var type = AttributeTypeInfoLookup.GetPointerType(array.AttributeType);
                var size = AttributeTypeInfoLookup.GetSize(array.AttributeType);
                var stride = AttributeTypeInfoLookup.GetStride(array.AttributeType);

                GL.BindBuffer(BufferTarget.ArrayBuffer, extraDataObjects[i]);

                switch (array.AttributeType)
                {
                    case AttributeType.Matrix4x4:
                        GL.VertexAttribPointer(location, size, type, false, stride, 0);
                        GL.VertexAttribPointer(location + 1, size, type, false, stride, sizeof(float) * 4);
                        GL.VertexAttribPointer(location + 2, size, type, false, stride, sizeof(float) * 8);
                        GL.VertexAttribPointer(location + 3, size, type, false, stride, sizeof(float) * 12);

                        GL.EnableVertexAttribArray(location);
                        GL.EnableVertexAttribArray(location + 1);
                        GL.EnableVertexAttribArray(location + 2);
                        GL.EnableVertexAttribArray(location + 3);

                        GL.VertexAttribDivisor(location, 1);
                        GL.VertexAttribDivisor(location + 1, 1);
                        GL.VertexAttribDivisor(location + 2, 1);
                        GL.VertexAttribDivisor(location + 3, 1);
                        //TODO eigenlijk moet je met 1 VertexAttributeArray meerdere pointers moeten kunnen definieren zodat dit geen special case is
                        location += 4;
                        break;
                    default:
                        GL.VertexAttribPointer(location, size, type, false, stride, 0);
                        GL.EnableVertexAttribArray(location);
                        GL.VertexAttribDivisor(location, 1);

                        location++;
                        break;
                }

            }
        }

        private static int CreateVertexBufferObject(VertexBuffer buffer)
        {
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            return vbo;
        }

        public void UpdateBuffer(VertexBuffer buffer, VertexBufferCacheHandles handles)
        {
            //upload vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, handles.VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Vertices.Length * Vertex.Stride), buffer.Vertices, BufferUsageHint.StaticDraw);

            //upload indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(buffer.IndexCount * sizeof(uint)), buffer.Indices, BufferUsageHint.StaticDraw);

            buffer.HasChanged = false;
        }

        public void UpdateExtraData(VertexBuffer buffer, VertexBufferCacheHandles handles)
        {
            //upload extra data
            for (int i = 0; i < buffer.ExtraAttributeCount; i++)
            {
                var array = buffer.GetAttribute(i);
                var id = handles.ExtraVBO[i];

                GL.BindBuffer(BufferTarget.ArrayBuffer, id);

                var stride = AttributeTypeInfoLookup.GetStride(array.AttributeType);
                IntPtr size = (IntPtr)(array.Count * stride);
                const BufferUsageHint hint = BufferUsageHint.DynamicDraw;

                UploadCustomDataArray(array, size, hint);
            }

            buffer.ExtraDataHasChanged = false;
        }

        protected override void DisposeOf(VertexBufferCacheHandles loaded)
        {

        }
    }
}
