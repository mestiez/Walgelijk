using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Globalization;

namespace Walgelijk.AssetManager;

public class GlobalAssetIdConverter : JsonConverter<GlobalAssetId>
{
    public override GlobalAssetId ReadJson(JsonReader reader, Type objectType, GlobalAssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string? s = reader.Value?.ToString();

        if (string.IsNullOrWhiteSpace(s))
            return GlobalAssetId.None;

        if (s.Contains(':'))
        {
            var index = s.IndexOf(':');

            var external = s[..index];
            var @internal = s[(index + 1)..];

            if (!int.TryParse(external, out var externalId) && !int.TryParse(external, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out externalId))
                externalId = Hashes.MurmurHash1(external);

            if (!AssetId.TryParse(@internal, out var internalId))
                internalId = new AssetId(@internal);

            return new GlobalAssetId(externalId, internalId);
        }
        else
            throw new Exception("Invalid GlobalAssetId: no delimiter. A global asset ID is formatted like so: \"external:internal\"");
    }

    public override void WriteJson(JsonWriter writer, GlobalAssetId value, JsonSerializer serializer)
    {
        if (value == GlobalAssetId.None)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToString());
    }
}
