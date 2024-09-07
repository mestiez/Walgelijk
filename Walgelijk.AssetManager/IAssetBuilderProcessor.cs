using System.Collections.ObjectModel;

namespace Walgelijk.AssetManager;

public interface IAssetBuilderProcessor
{
    void Process(ref AssetMetadata m, ReadOnlySpan<byte> content);
}
