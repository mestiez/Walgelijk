using System;
using System.Collections.Generic;

namespace Walgelijk;

public readonly struct BatchProfile : IEquatable<BatchProfile>
{
    public readonly IReadableTexture Texture;
    public readonly Material Material;
    public readonly VertexBuffer VertexBuffer;
    public readonly RenderOrder RenderOrder;

    public BatchProfile(Material material, VertexBuffer vertexBuffer, IReadableTexture texture, RenderOrder renderOrder)
    {
        Material = material;
        VertexBuffer = vertexBuffer;
        Texture = texture;
        RenderOrder = renderOrder;
    }

    public override bool Equals(object? obj)
    {
        return obj is BatchProfile profile && Equals(profile);
    }

    public bool Equals(BatchProfile other)
    {
        return EqualityComparer<IReadableTexture>.Default.Equals(Texture, other.Texture) &&
               EqualityComparer<Material>.Default.Equals(Material, other.Material) &&
               EqualityComparer<VertexBuffer>.Default.Equals(VertexBuffer, other.VertexBuffer) &&
               EqualityComparer<RenderOrder>.Default.Equals(RenderOrder, other.RenderOrder);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Texture, Material, VertexBuffer, RenderOrder);
    }

    public static bool operator ==(BatchProfile left, BatchProfile right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BatchProfile left, BatchProfile right)
    {
        return !(left == right);
    }
}
