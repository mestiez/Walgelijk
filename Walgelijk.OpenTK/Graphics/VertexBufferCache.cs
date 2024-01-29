using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;

namespace Walgelijk.OpenTK;

public class VertexBufferCache<TVertex> : Cache<VertexBuffer<TVertex>, VertexBufferCacheHandles> where TVertex : struct
{
    protected override VertexBufferCacheHandles CreateNew(VertexBuffer<TVertex> buffer)
    {
        var vao = CreateVertexArrayObject();

        var vbo = CreateVertexBufferObject();
        var ibo = CreateIndexBufferObject();
        var extraVbos = CreateExtraVBO(buffer);

        ConfigureAttributes(buffer, vbo, extraVbos);

        var handles = new VertexBufferCacheHandles(vbo, vao, ibo, extraVbos);

        UpdateBuffer(buffer, handles);
        UpdateExtraData(buffer, handles);

        buffer.HasChanged = false;

        return handles;
    }

    private static int[] CreateExtraVBO(VertexBuffer<TVertex> buffer)
    {
        var ids = new int[buffer.ExtraAttributeCount];

        for (int i = 0; i < buffer.ExtraAttributeCount; i++)
        {
            var array = buffer.GetAttribute(i);
            if (array == null) 
                continue;

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
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<int>(), hint);
                break;
            case AttributeType.Float:
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<float>(), hint);
                break;
            case AttributeType.Double:
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<double>(), hint);
                break;
            case AttributeType.Vector2:
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<Vector2>(), hint);
                break;
            case AttributeType.Vector3:
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<Vector3>(), hint);
                break;
            case AttributeType.Vector4:
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<Vector4>(), hint);
                break;
            case AttributeType.Matrix4x4:
                GL.BufferData(BufferTarget.ArrayBuffer, size, array.GetData<Matrix4x4>(), hint);
                break;
        }
    }

    private static VertexAttribPointerType GetPointerType(AttributeType v)
    {
        return v switch
        {
            AttributeType.Integer => VertexAttribPointerType.Int,
            AttributeType.Float => VertexAttribPointerType.Float,
            AttributeType.Double => VertexAttribPointerType.Double,
            AttributeType.Vector2 => VertexAttribPointerType.Float,
            AttributeType.Vector3 => VertexAttribPointerType.Float,
            AttributeType.Vector4 => VertexAttribPointerType.Float,
            AttributeType.Matrix4x4 => VertexAttribPointerType.Float,
            _ => throw new Exception("Invalid attribute type"),
        };
    }

    private static int CreateIndexBufferObject()
    {
        int ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
        return ibo;
    }

    private static int CreateVertexArrayObject()
    {
        int vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        return vao;
    }

    private static void ConfigureAttributes(VertexBuffer<TVertex> buffer, int VBO, int[] extraDataObjects)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

        int location = 0;
        int currentPointerPos = 0;
        var desc = buffer.Descriptor ?? throw new Exception("VertexBuffer Descriptor is null!");

        foreach (var a in desc.GetAttributes())
        {
            GL.VertexAttribPointer(
                location,
                a.ComponentCount,
                GetPointerType(a.Type),
                false,
                desc.GetTotalStride(),
                currentPointerPos);

            GL.EnableVertexAttribArray(location);

            currentPointerPos += a.TotalSize;
            location++;
        }

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

    private static int CreateVertexBufferObject()
    {
        int vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        return vbo;
    }

    public void UpdateBuffer(VertexBuffer<TVertex> buffer, VertexBufferCacheHandles handles)
    {
        var hint = buffer.Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
        var desc = buffer.Descriptor ?? throw new Exception("VertexBuffer Descriptor is null!");

        //upload vertices
        GL.BindBuffer(BufferTarget.ArrayBuffer, handles.VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Vertices.Length * desc.GetTotalStride()), buffer.Vertices, hint);

        //upload indices
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(buffer.IndexCount * sizeof(uint)), buffer.Indices, hint);

        if (buffer.DisposeLocalCopy)
        {
            buffer.Vertices = null;
            buffer.Indices = null;
        }

        buffer.HasChanged = false;
    }

    public void UpdateExtraData(VertexBuffer<TVertex> buffer, VertexBufferCacheHandles handles)
    {
        var hint = buffer.Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

        //upload extra data
        for (int i = 0; i < buffer.ExtraAttributeCount; i++)
        {
            var array = buffer.GetAttribute(i);
            var id = handles.ExtraVBO[i];

            GL.BindBuffer(BufferTarget.ArrayBuffer, id);

            var stride = AttributeTypeInfoLookup.GetStride(array.AttributeType);
            IntPtr size = array.Count * stride;
            UploadCustomDataArray(array, size, hint);
        }

        buffer.ExtraDataHasChanged = false;
    }

    protected override void DisposeOf(VertexBufferCacheHandles loaded)
    {
        GL.DeleteBuffer(loaded.VBO);
        GL.DeleteBuffer(loaded.IBO);
        GL.DeleteBuffers(loaded.ExtraVBO.Length, loaded.ExtraVBO);
        GL.DeleteVertexArray(loaded.VAO);
    }
}
