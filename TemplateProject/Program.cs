namespace Videogame;

public static class Program
{
    private static void Main(string[] args)
    {
#if DEBUG
        var _ = new Entry();
#else
        try
        {
            var _ = new Entry();
        }
        catch (System.Exception)
        {
            //TODO crash handling
            throw;
        }
#endif
    }
}
