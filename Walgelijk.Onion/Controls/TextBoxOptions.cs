using System.Text.RegularExpressions;

namespace Walgelijk.Onion.Controls;

public readonly struct TextBoxOptions
{
    public readonly string? Placeholder;
    public readonly int? MaxLength;
    public readonly Regex? Filter;
    public readonly bool Password;

    public static readonly TextBoxOptions PasswordInput = new(password: true);
    public static readonly TextBoxOptions DecimalInput = new(filter: new Regex(@"^\d?(\.\d?)?$"));

    public TextBoxOptions(string? placeholder = null, int? maxLength = null, Regex? filter = null, bool password = false)
    {
        Placeholder = placeholder;
        MaxLength = maxLength;
        Filter = filter;
        Password = password;
    }
}
