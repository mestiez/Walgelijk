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

public class AnimationCollection : IAnimation
{
    public IAnimation[] All = Array.Empty<IAnimation>();

    public void AnimateAlpha(ref float alpha, float t)
    {
        for (int i = 0; i < All.Length; i++)
            All[i].AnimateAlpha(ref alpha, t);
    }

    public void AnimateColour(ref Color color, float t)
    {
        for (int i = 0; i < All.Length; i++)
            All[i].AnimateColour(ref color, t);
    }

    public void AnimateRect(ref Rect rect, float t)
    {
        for (int i = 0; i < All.Length; i++)
            All[i].AnimateRect(ref rect, t);
    }

    public Matrix3x2 GetTransform(float t)
    {
        //TODO
        throw new NotImplementedException();
    }

    public bool ShouldRenderText(float t)
    {
        bool a = true;
        for (int i = 0; i < All.Length; i++)
            a &= All[i].ShouldRenderText(t);
        return a;
    }
}

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

    public void AnimateAlpha(ref float alpha, float t) { }
    public void AnimateColour(ref Color color, float t) { }
    public void AnimateRect(ref Rect rect, float t) => rect = rect.Scale(GetScaling(t));
    public Matrix3x2 GetTransform(float t) => Matrix3x2.CreateScale(GetScaling(t));
    public bool ShouldRenderText(float t) => true;

    private float GetScaling(float t) => Utilities.Lerp(IAnimation.GetProgress(t), 1, TargetSize);
}

public readonly struct MoveInAnimation : IAnimation
{
    public readonly Vector2 Origin;

    public MoveInAnimation(Vector2 origin)
    {
        Origin = origin;
    }

    public void AnimateAlpha(ref float alpha, float t) { }
    public void AnimateColour(ref Color color, float t) { }

    public void AnimateRect(ref Rect rect, float t)
    {
        var from = new Rect(Origin, rect.GetSize());
        rect = Utilities.Lerp(from, rect, IAnimation.GetProgress(t));
    }

    public Matrix3x2 GetTransform(float t) => Matrix3x2.Identity;

    public bool ShouldRenderText(float t) => true;
}
