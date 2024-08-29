using System.Numerics;

namespace Walgelijk.Onion.Animations;

public readonly struct MoveInAnimation : IAnimation
{
    public readonly Vector2 Origin;

    public MoveInAnimation(Vector2 origin)
    {
        Origin = origin;
    }

    public void AnimateColour(ref Color color, float t) { }

    public void AnimateRect(ref Rect rect, float t)
    {
        var from = new Rect(Origin, rect.GetSize());
        rect = Utilities.Lerp(from, rect, IAnimation.GetProgress(t));
    }

    public Matrix3x2 GetTransform(float t) => Matrix3x2.Identity;

    public bool ShouldRenderText(float t) => true;
}
