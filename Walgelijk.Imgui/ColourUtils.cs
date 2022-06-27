using System;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public struct ColourUtils
    {
        public static void ColourToHexadecimal(Color color, Span<char> toFill)
        {
            var bytes = color.ToBytes();
            toFill[0] = '#';

            convert(bytes.r, out char a, out char b);
            toFill[1] = a;
            toFill[2] = b;

            convert(bytes.g, out a, out b);
            toFill[3] = a;
            toFill[4] = b;

            convert(bytes.b, out a, out b);
            toFill[5] = a;
            toFill[6] = b;

            void convert(byte x, out char a, out char b)
            {
                var s = DecimalHexadecimalLookup[x];
                a = s.Length == 2 ? s[0] : '0';
                b = s.Length == 2 ? s[1] : s[0];
            }
        }

        public static bool IsValidHexadecimalColour(ReadOnlySpan<char> str)
        {
            if (str.Length != 7)
                return false;
            if (str[0] != '#')
                return false;
            var allowed = DecimalHexadecimalLookup.AsSpan()[0..16];
            for (int i = 1; i < 7; i++)
            {
                bool f = false;
                for (int p = 0; p < allowed.Length; p++)
                    if (allowed[p].Contains(str[i]))
                    {
                        f =  true;
                        break;
                    }

                if (!f)
                    return false;
            }

            return true;
        }

        public static Color HSVtoRGB(float h01, float s01, float v01)
        {
            float h = h01 * 360;

            float c = v01 * s01;
            float x = c * (1 - MathF.Abs(Utilities.Mod(h / 60, 2) - 1));
            float m = v01 - c;

            Vector3 i = Vector3.One;

            if (domain(0, h, 60))
                i = new Vector3(c, x, 0);
            else if (domain(60, h, 120))
                i = new Vector3(x, c, 0);
            else if (domain(120, h, 180))
                i = new Vector3(0, c, x);
            else if (domain(180, h, 240))
                i = new Vector3(0, x, c);
            else if (domain(240, h, 300))
                i = new Vector3(x, 0, c);
            else if (domain(300, h, 360))
                i = new Vector3(c, 0, x);

            return new Color(i.X + m, i.Y + m, i.Z + m);

            static bool domain(float min, float x, float max) => min <= x && x <= max;
        }

        public static (float h01, float s01, float v01) RGBtoHSV(Color color)
        {
            float cMax = MathF.Max(color.R, MathF.Max(color.G, color.B));
            float cMin = MathF.Min(color.R, MathF.Min(color.G, color.B));
            float delta = cMax - cMin;

            float hue = 0;
            if (approx(delta, 0))
                hue = 0;
            else if (approx(color.R, cMax))
                hue = 60 * Utilities.Mod((color.G - color.B) / delta, 6f);
            else if (approx(color.G, cMax))
                hue = 60 * ((color.B - color.R) / delta + 2);
            else if (approx(color.B, cMax))
                hue = 60 * ((color.R - color.G) / delta + 4);
            hue /= 360;

            float saturation;
            if (approx(cMax, 0))
                saturation = 0;
            else
                saturation = delta / cMax;

            float value = cMax;

            return (hue, saturation, value);

            static bool approx(float a, float b) => MathF.Abs(a - b) < 0.001f;
        }

        public static readonly string[] DecimalHexadecimalLookup =
        {
            "0" ,
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "1A",
            "1B",
            "1C",
            "1D",
            "1E",
            "1F",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "2A",
            "2B",
            "2C",
            "2D",
            "2E",
            "2F",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "3A",
            "3B",
            "3C",
            "3D",
            "3E",
            "3F",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "4A",
            "4B",
            "4C",
            "4D",
            "4E",
            "4F",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "5A",
            "5B",
            "5C",
            "5D",
            "5E",
            "5F",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "6A",
            "6B",
            "6C",
            "6D",
            "6E",
            "6F",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "7A",
            "7B",
            "7C",
            "7D",
            "7E",
            "7F",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "8A",
            "8B",
            "8C",
            "8D",
            "8E",
            "8F",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "9A",
            "9B",
            "9C",
            "9D",
            "9E",
            "9F",
            "A0",
            "A1",
            "A2",
            "A3",
            "A4",
            "A5",
            "A6",
            "A7",
            "A8",
            "A9",
            "AA",
            "AB",
            "AC",
            "AD",
            "AE",
            "AF",
            "B0",
            "B1",
            "B2",
            "B3",
            "B4",
            "B5",
            "B6",
            "B7",
            "B8",
            "B9",
            "BA",
            "BB",
            "BC",
            "BD",
            "BE",
            "BF",
            "C0",
            "C1",
            "C2",
            "C3",
            "C4",
            "C5",
            "C6",
            "C7",
            "C8",
            "C9",
            "CA",
            "CB",
            "CC",
            "CD",
            "CE",
            "CF",
            "D0",
            "D1",
            "D2",
            "D3",
            "D4",
            "D5",
            "D6",
            "D7",
            "D8",
            "D9",
            "DA",
            "DB",
            "DC",
            "DD",
            "DE",
            "DF",
            "E0",
            "E1",
            "E2",
            "E3",
            "E4",
            "E5",
            "E6",
            "E7",
            "E8",
            "E9",
            "EA",
            "EB",
            "EC",
            "ED",
            "EE",
            "EF",
            "F0",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "FA",
            "FB",
            "FC",
            "FD",
            "FE",
            "FF",
        };
    }
}
