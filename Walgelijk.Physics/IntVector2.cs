using System;

namespace Walgelijk.Physics;

public struct IntVector2 : IEquatable<IntVector2>
{
    public int X, Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj) =>
        obj is IntVector2 vector &&
               X == vector.X &&
               Y == vector.Y;

    public bool Equals(IntVector2 other) => other.X == X && other.Y == Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(IntVector2 left, IntVector2 right) => left.Equals(right);

    public static bool operator !=(IntVector2 left, IntVector2 right) => !(left == right);
}
