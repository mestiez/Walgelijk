namespace Walgelijk.AssetManager;

internal static class IEnumeratorExtensions
{
    public static T? NextOrDefault<T>(this IEnumerator<T> iterator)
    {
        if (iterator.MoveNext())
            return iterator.Current;
        return default;
    }
}
