using System.Numerics;

namespace Walgelijk;

public class BatchedSpriteComponent : Component
{
    public readonly Material Material;
    public readonly VertexBuffer VertexBuffer;

    public RenderOrder RenderOrder;
    public IReadableTexture Texture = Walgelijk.Texture.ErrorTexture;
    public bool SyncWithTransform = false;
    public bool Visible = true;

    public Matrix3x2 Transform;
    public Color Color = Colors.White;
    public bool HorizontalFlip = false, VerticalFlip = false;

    public BatchedSpriteComponent(Material material, VertexBuffer vertexBuffer, IReadableTexture texture)
    {
        Material = material;
        VertexBuffer = vertexBuffer;
        Texture = texture;
    }

    public BatchedSpriteComponent(IReadableTexture texture, bool centered = false)
    {
        Material = BatchMaterialCreator.DefaultInstancedMaterial;
        VertexBuffer = centered ? PrimitiveMeshes.CenteredQuad : PrimitiveMeshes.Quad;
        Texture = texture;
    }
}
