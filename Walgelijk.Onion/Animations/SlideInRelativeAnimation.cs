using System.Numerics;
//🍕

namespace Walgelijk.Onion.Animations;

public readonly struct SlideInRelativeAnimation : IAnimation
{
    public readonly Vector2 OffsetRatio;

    public SlideInRelativeAnimation(Vector2 offsetRatio)
    {
        OffsetRatio = offsetRatio;
    }

    public void AnimateColour(ref Color color, float t) { }

    public void AnimateRect(ref Rect rect, float t)
    {
        rect = Utilities.Lerp(rect.Translate(OffsetRatio * rect.GetSize()), rect, IAnimation.GetProgress(t));
    }

    public Matrix3x2 GetTransform(float t) => Matrix3x2.Identity;

    public bool ShouldRenderText(float t) => true;
}