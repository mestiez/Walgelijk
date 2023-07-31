namespace Walgelijk.Onion;

public struct IdGen
{
    public static int Create(int a, int b, int c, int d, int e, int f) => global::System.HashCode.Combine(@base, a, b, c, d, e, f);
    public static int Create(int a, int b, int c, int d, int e) => global::System.HashCode.Combine(@base, a, b, c, d, e);
    public static int Create(int a, int b, int c, int d) => global::System.HashCode.Combine(@base, a, b, c, d);
    public static int Create(int a, int b, int c) => global::System.HashCode.Combine(@base, a, b, c);
    public static int Create(int a, int b) => global::System.HashCode.Combine(@base, a, b);
    public static int Create(int a) => global::System.HashCode.Combine(@base, a);

    private static int @base => Onion.Tree.CurrentNode?.Identity ?? 0;
}