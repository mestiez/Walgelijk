using System;

namespace Walgelijk;

[Flags]
public enum FontStyle
{
    Regular = 0,
    Italic = 1,
    Bold = 2,
    ItalicBold = Italic | Bold
}
