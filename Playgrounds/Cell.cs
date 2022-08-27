namespace TestWorld;

public class Cell
{
    public float Current;
    public float Previous;
    public float Velocity;
    public float Absorption = 1;
    public float VelocityAbsorption = 1;

    public Cell(float current)
    {
        Previous = Current = current;
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
