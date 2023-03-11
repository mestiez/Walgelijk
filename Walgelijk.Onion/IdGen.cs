namespace Walgelijk.Onion;

public struct IdGen
{
    public static int Hash(int a, int b, int c, int d, int e, int f) => global::System.HashCode.Combine(a, b, c, d, e, f);
    public static int Hash(int a, int b, int c, int d, int e) => global::System.HashCode.Combine(a, b, c, d, e);
    public static int Hash(int a, int b, int c, int d) => global::System.HashCode.Combine(a, b, c, d);
    public static int Hash(int a, int b, int c) => global::System.HashCode.Combine(a, b, c);
    public static int Hash(int a, int b) => global::System.HashCode.Combine(a, b);
    public static int Hash(int a) => global::System.HashCode.Combine(a);
}