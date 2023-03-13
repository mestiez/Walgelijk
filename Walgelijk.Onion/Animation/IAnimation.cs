using System.Numerics;

namespace Walgelijk.Onion.Animations;

public interface IAnimation
{
    public void AnimateRect(ref Rect rect, float t);
    public void AnimateColour(ref Color color, float t);
    public void AnimateAlpha(ref float alpha, float t);
    public bool ShouldRenderText(float t);

    public static float GetProgress(float t) => Easings.Cubic.InOut(Utilities.Clamp(t));
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

    public bool ShouldRenderText(float t) => t > 0.5f;
}

public readonly struct ShrinkAnimation : IAnimation
{
    public void AnimateAlpha(ref float alpha, float t) { }
    public void AnimateColour(ref Color color, float t) { }

    public void AnimateRect(ref Rect rect, float t)
    {
        rect = rect.Scale(Utilities.Lerp(IAnimation.GetProgress(t), 1, 0.6f));
    }

    public bool ShouldRenderText(float t) => true;
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

    public bool ShouldRenderText(float t) => true;
}
