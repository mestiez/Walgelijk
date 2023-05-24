using System;
using System.IO;
using Walgelijk.BmFont;

namespace Walgelijk;

/// <summary>
/// Loads fonts based on the format that is detected or provided
/// </summary>
public static class FontLoader
{
    public static Font Load(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        switch (ext)
        {
            case ".fnt": return BmFontReader.LoadFromMetadata(path);
            case ".ttf": return null;
            default:
                throw new Exception("Could not detect format for given font file: " + path);
        }
    }
}