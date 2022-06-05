using System;

namespace Walgelijk;

/// <summary>
/// Very basic random name generator
/// </summary>
public class NameGenerator
{
    public static readonly NameGenerator Shared = new NameGenerator();

    public static readonly string[] Vowels =
    {
        "a",
        "aa",
        "e",
        "ee",
        "eu",
        "i",
        "o",
        "oe",
        "ou",
        "u",
        "ui",
        "y",
    };

    public static readonly string[] Consonants =
    {
        "b",
        "c",
        "d",
        "f",
        "g",
        "h",
        "j",
        "k",
        "l",
        "m",
        "n",
        "p",
        "q",
        "r",
        "s",
        "t",
        "u",
        "v",
        "w",
        "x",
        "z",
        "th",
        "ch",
        "sh",
        "sch",
    };

    public ReadOnlySpan<char> GenerateName()
    {
        return GenerateName(Random.Shared.Next(3, 7));
    }

    public ReadOnlySpan<char> GenerateName(int length)
    {
        Span<char> name = new char[length];
        int i = 0;

        bool chooseVowels = Utilities.RandomFloat() > 0.5f;

        while (i < length)
        {
            var a = Utilities.PickRandom((chooseVowels ? Vowels : Consonants)).AsSpan();

            a[0..Math.Min(length - i, a.Length)].CopyTo(name[i..]);
            i += a.Length;

            chooseVowels = !chooseVowels;
        }

        name[0] = char.ToUpper(name[0]);

        return name;
    }
}
