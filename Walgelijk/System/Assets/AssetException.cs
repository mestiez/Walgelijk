using System;

namespace Walgelijk;

public class AssetException : Exception
{
    public AssetException(in Asset asset, string message) : base(message)
    {
        this.Asset = asset;
    }

    public Asset Asset { get; }
}
