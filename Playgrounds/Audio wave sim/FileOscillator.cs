using System.Numerics;
using Walgelijk;

namespace TestWorld;

public class PointFileOscillator : IOscillator
{
    public (int x, int y) Position;
    public bool Loops = true;
    public float Volume = 1;

    public readonly byte[] InputData;

    private int i = 0;

    public PointFileOscillator(string path, Vector2 position)
    {
        Position = ((int)position.X, (int)position.Y);
        InputData = File.ReadAllBytes(path);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        //Position.x = (int)Utilities.Lerp(0, field.Width - 1, ((float)time * 2.8f) % 1);
        //Position.y = (int)Utilities.Lerp(0, field.Height - 1, ((float)time * 0.5f) % 1);
        var b = InputData[Loops ? (i++ % InputData.Length) : Utilities.Clamp(i++, 0, InputData.Length - 1)];
        var v = Utilities.MapRange(0, 255, -1, 1, b);
        field.Get(Position.x, Position.y).ForceSet(v * Volume);
    }
}

public class AreaFileOscillator : IOscillator
{
    public (int x, int y) A;
    public (int x, int y) B;
    public bool Loops = true;
    public float Volume = 1;

    public readonly byte[] InputData;

    private int i = 0;

    public AreaFileOscillator(string path, Vector2 a, Vector2 b)
    {
        A = ((int)a.X, (int)a.Y);
        B = ((int)b.X, (int)b.Y);
        InputData = File.ReadAllBytes(path);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        var b = InputData[Loops ? (i++ % InputData.Length) : Utilities.Clamp(i++, 0, InputData.Length - 1)];
        var v = Utilities.MapRange(0, 255, -1, 1, b);
        foreach (var (x,y) in line(A.x, A.y, B.x, B.y))
        {
            field.Get(x, y).ForceSet(v * Volume);
        }
    }

    static IEnumerable<(int x, int y)> line(int x, int y, int x2, int y2)
    {
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = System.Math.Abs(w);
        int shortest = System.Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = System.Math.Abs(h);
            shortest = System.Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            yield return (x, y);
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
    }
}
