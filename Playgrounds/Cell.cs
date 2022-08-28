namespace TestWorld;

public class Cell
{
    public float Current;
    public float Previous;
    public float Velocity;
    public float Absorption = 1;
    public float VelocityAbsorption = 1;

    public readonly int X, Y;

    public Cell(float current, int x, int y)
    {
        Previous = Current = current;
        X = x;
        Y = y;
    }

    public void Sync()
    {
        Previous = Current;
    }

    public void ForceSet(float v)
    {
        Previous = Current = v;
        Velocity = 0;
    }
}
