using System.Numerics;

namespace Walgelijk.Onion.Animations;

public interface IAnimation
{
    public void AnimateRect(ref Rect rect, float t);
    public void AnimateColour(ref Color color, float t);
    public void AnimateAlpha(ref float alpha, float t);
    public Matrix3x2 GetTransform(float t);
    public bool ShouldRenderText(float t);

    public static float GetProgress(float t) => Easings.Cubic.InOut(Utilities.Clamp(Utilities.NanFallback(t, 1)));
}
