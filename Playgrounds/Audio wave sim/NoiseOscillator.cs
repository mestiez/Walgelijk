using System.Numerics;
using Walgelijk;

namespace Playgrounds;

public class NoiseOscillator : IOscillator
{
    public (int x, int y) Position;

    public NoiseOscillator(Vector2 position)
    {
        Position = ((int)position.X, (int)position.Y);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        field.Get(Position.x, Position.y).ForceSet(Utilities.RandomFloat(-2, 2));
    }
}
