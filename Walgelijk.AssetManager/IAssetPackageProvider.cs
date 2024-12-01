using System.Collections.Immutable;

namespace Walgelijk.AssetManager;

public interface IAssetPackageProvider
{
    AssetPackageMetadata GetMetadata();
    ImmutableDictionary<AssetId, string> GetGuidTable();
    IEnumerator<string> GetHierarchy();
    ImmutableDictionary<string, AssetId[]> GetTaggedCache();
}
