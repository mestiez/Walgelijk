using System.Numerics;

namespace Walgelijk.Onion.Animations;

public readonly struct ShrinkAnimation : IAnimation
{
    public readonly float TargetSize;

    public ShrinkAnimation()
    {
        TargetSize = 0.9f;
    }

    public ShrinkAnimation(float minSize)
    {
        TargetSize = minSize;
    }

    public void AnimateColour(ref Color color, float t) { }
    public void AnimateRect(ref Rect rect, float t) => rect = rect.Scale(GetScaling(t));
    public Matrix3x2 GetTransform(float t) => Matrix3x2.CreateScale(GetScaling(t));
    public bool ShouldRenderText(float t) => true;

    private float GetScaling(float t) => Utilities.Lerp(IAnimation.GetProgress(t), 1, TargetSize);
}
