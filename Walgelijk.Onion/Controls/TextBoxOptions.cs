using System.Text.RegularExpressions;

namespace Walgelijk.Onion.Controls;

public readonly struct TextBoxOptions
{
    public readonly string? Placeholder;
    public readonly int? MaxLength;
    public readonly Regex? Filter;
    public readonly bool Password;
    public readonly bool ScrollIsValueChange;

    public static readonly TextBoxOptions TextInput = new();
    public static readonly TextBoxOptions PasswordInput = new(password: true);
    public static readonly TextBoxOptions DecimalInput = new(filter: new Regex(@"^\d?(\.\d?)?$"), scrollIsValueChange: true);

    public TextBoxOptions(string? placeholder = null, int? maxLength = null, Regex? filter = null, bool password = false, bool scrollIsValueChange = false)
    {
        Placeholder = placeholder;
        MaxLength = maxLength;
        Filter = filter;
        Password = password;
        ScrollIsValueChange = scrollIsValueChange;
    }
}
