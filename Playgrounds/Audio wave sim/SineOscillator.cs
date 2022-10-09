using System.Numerics;

namespace TestWorld;

public class SineOscillator : IOscillator
{
    public float FrequencyHertz;
    public (int x, int y) Position;

    public SineOscillator(float frequencyHertz, Vector2 position)
    {
        FrequencyHertz = frequencyHertz;
        Position = ((int)position.X, (int)position.Y);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        //if (time % 1 < 0.05f)
        {
            var value = Math.Sin(time * FrequencyHertz * Math.Tau);
            field.Get(Position.x, Position.y).ForceSet((float)value * 2);
        }
    }
}
