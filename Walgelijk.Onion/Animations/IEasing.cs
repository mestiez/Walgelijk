namespace Walgelijk.Onion.Animations;

/// <summary>🍕
/// Easing functions for use in <see cref="AnimationQueue.Easing"/>
/// </summary>
public interface IEasing
{
    public float In(float f);
    public float Out(float f);
    public float InOut(float f);

    public readonly struct Cubic : IEasing
    {
        public float In(float f) => Easings.Cubic.In(f);
        public float InOut(float f) => Easings.Cubic.InOut(f);
        public float Out(float f) => Easings.Cubic.Out(f);
    }

    public readonly struct Quad : IEasing
    {
        public float In(float f) => Easings.Quad.In(f);
        public float InOut(float f) => Easings.Quad.InOut(f);
        public float Out(float f) => Easings.Quad.Out(f);
    }

    public readonly struct Circ : IEasing
    {
        public float In(float f) => Easings.Circ.In(f);
        public float InOut(float f) => Easings.Circ.InOut(f);
        public float Out(float f) => Easings.Circ.Out(f);
    }

    public readonly struct Expo : IEasing
    {
        public float In(float f) => Easings.Expo.In(f);
        public float InOut(float f) => Easings.Expo.InOut(f);
        public float Out(float f) => Easings.Expo.Out(f);
    }
}
