namespace Walgelijk.Physics;

public class Chunk
{
    public readonly int X, Y;
    public readonly Entity[] ContainingEntities;
    public int BodyCount;

    public Chunk(int x, int y, int capacity = 64)
    {
        X = x;
        Y = y;
        ContainingEntities = new Entity[capacity];
    }

    public void Add(in Entity e)
    {
        if (BodyCount >= ContainingEntities.Length)
        {
            Logger.Warn("Chunk overflow!");
            return;
        }

        ContainingEntities[BodyCount++] = e;
    }

    public void Clear()
    {
        BodyCount = 0;
    }

    public bool IsEmpty => BodyCount <= 0;
}
