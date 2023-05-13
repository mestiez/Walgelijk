using System.Numerics;

namespace Walgelijk.Onion.Animations;

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
