[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(Walgelijk.Onion.HotReloadResponder))]
namespace Walgelijk.Onion;
internal static class HotReloadResponder
{
    public static void UpdateApplication(Type[]? _)
    {
        Onion.ForceClearCache = true;
        Logger.Debug("Hot reload detected, clearing Onion cache.");
    }
}
