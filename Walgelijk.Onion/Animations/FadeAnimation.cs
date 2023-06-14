using System.Numerics;

namespace Walgelijk.Onion.Animations;

public readonly struct FadeAnimation : IAnimation
{
    public void AnimateAlpha(ref float alpha, float t)
    {
        alpha *= IAnimation.GetProgress(t);
    }

    public void AnimateColour(ref Color color, float t)
    {
        AnimateAlpha(ref color.A, t);
    }

    public void AnimateRect(ref Rect rect, float t) { }

    public Matrix3x2 GetTransform(float t) => Matrix3x2.Identity;

    public bool ShouldRenderText(float t) => t > 0.5f;
}
