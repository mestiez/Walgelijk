namespace Playgrounds;

public class Cell
{
    public float Current;
    public float Previous;
    public float Velocity;
    public float Absorption = 1;
    public float VelocityAbsorption = 1;
    public float ConductivityAdd = 0;

    public readonly int X, Y;

    public Cell Left;
    public Cell Up;
    public Cell Down;
    public Cell Right;

    public Cell(float current, int x, int y)
    {
        Previous = Current = current;
        X = x;
        Y = y;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void ForceSet(float v)
    {
        Previous = Current = v;
        Velocity = 0;
    }
}
