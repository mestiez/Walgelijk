using System.Numerics;

namespace Walgelijk.Prism;

public class PrismTransformComponent : Component
{
    public Vector3 Position;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4x4 Transformation = Matrix4x4.Identity;

    public Vector3 Forwards => Vector3.TransformNormal(-Vector3.UnitZ, Transformation);
    public Vector3 Right => Vector3.TransformNormal(Vector3.UnitX, Transformation);
    public Vector3 Up => Vector3.TransformNormal(Vector3.UnitY, Transformation);
    public Vector3 Backwards => -Forwards;
    public Vector3 Left => -Right;
    public Vector3 Down => -Up;

    public void RecalculateMatrix()
    {
        Transformation = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
    }
}
