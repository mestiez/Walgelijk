using System;
using System.Numerics;

namespace Walgelijk;

public class Batch : IDisposable, IRenderTask
{
    public readonly BatchProfile Profile;
    public readonly Matrix4x4AttributeArray Transforms;
    public readonly Vector4AttributeArray Colors;
    public readonly VertexBuffer InstancedVertexBuffer;

    public int Amount;

    public Batch(BatchProfile profile)
    {
        Profile = profile;

        Transforms = new Matrix4x4AttributeArray(new Matrix4x4[32]);
        Colors = new Vector4AttributeArray(new Vector4[32]);

        InstancedVertexBuffer = new VertexBuffer(profile.VertexBuffer.Vertices, profile.VertexBuffer.Indices, new VertexAttributeArray[]
        {
            Transforms,
            Colors,
        });
    }

    public void Add(Matrix3x2 transform, Color col)
    {
        int i = Amount;

        if (i >= Transforms.Count)
        {
            int newSize = i + 32;
            Array.Resize(ref Transforms.Data, newSize);
            Array.Resize(ref Colors.Data, newSize);
        }

        Transforms.Data[i] = new Matrix4x4(transform);
        Colors.Data[i] = col;

        Amount++;
    }

    public void Dispose()
    {
        InstancedVertexBuffer.Dispose();
    }

    public void Execute(IGraphics graphics)
    {
        Profile.Material.SetUniform("mainTex", Profile.Texture);
        graphics.CurrentTarget.ModelMatrix = Matrix4x4.Identity;
        graphics.DrawInstanced(InstancedVertexBuffer, Amount, Profile.Material);
    }

}
