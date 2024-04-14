/* -- Asset manager system --
 * 
 * What I need:
 * - Retrieve asset
 * - Query assets (e.g get all assets of type, in set, with tag, whatever the fuck)
 * - Cross-platform paths
 * - Reference counting / lifetime
 * - Mod support
 * - Stream support
 * - Async
 * - Develop using files, create package on build
 * - Loading packages
 * - Unloading packages
 * - Having multiple packages loaded at once
 * 
 * Proposed API:
 * - static AssetPackage.Load(string path)
 *      - Integrates with Resources.Load<AssetPackage>(string path)
 * - AssetPackage.Dispose()
 * - AssetPackage.Query()
 * - AssetPackage.Id
 * 
 * Resources:
 *  - Game Engine Architecture 3rd Edition, ch 7
 */

using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

/// <summary>
/// Locally unique ID for an asset
/// </summary>
[JsonConverter(typeof(AssetIdConverter))]
public readonly struct AssetId
{
    /// <summary>
    /// Id of the asset within the asset package
    /// </summary>
    public readonly int Internal;

    public AssetId(string path)
    {
        Internal = Hashes.MurmurHash1(path);
    }

    public AssetId(int @internal)
    {
        Internal = @internal;
    }
}

public class AssetIdConverter : JsonConverter<AssetId>
{
    public override AssetId ReadJson(JsonReader reader, Type objectType, AssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string s = reader.Value?.ToString() ?? throw new Exception("Invalid AssetID. Only integers are accepted.");

        return new AssetId(int.Parse(s));
    }

    public override void WriteJson(JsonWriter writer, AssetId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Internal.ToString());
    }
}