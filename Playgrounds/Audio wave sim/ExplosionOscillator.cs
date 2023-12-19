using System.Numerics;
using Walgelijk;

namespace Playgrounds;

public class ExplosionOscillator : IOscillator
{
    public (int x, int y) Position;
    public float Volume = 0.5f;

    public ExplosionOscillator(Vector2 position)
    {
        Position = ((int)position.X, (int)position.Y);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        double t = time % 3;
        const double duration = 0.4d;

        var radius = Utilities.RandomFloat(7, 9) * (t / duration);
        var intensity = Math.Max(0, Math.Min(1, Utilities.MapRange(0, duration, 1, 0, t)));
        intensity *= intensity;

        if (intensity > 0)
            for (int x = 0; x < field.Width; x++)
                for (int y = 0; y < field.Height; y++)
                    if (Vector2.Distance(new Vector2(Position.x, Position.y), new Vector2(x, y)) < radius)
                    {
                        field.Get(x, y).ForceSet(Volume * (float)(-5 + (Utilities.RandomFloat(-3, 3) * (1 - intensity) * intensity)));
                    }
    }
}
