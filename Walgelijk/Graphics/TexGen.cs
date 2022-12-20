using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Provides texture generation functions
/// </summary>
public struct TexGen
{
    /// <summary>
    /// Generate a simple checkerboard texture
    /// </summary>
    public static Texture Checkerboard(int width, int height, int boxSize, Color a, Color b)
    {
        var pixels = new Color[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var f = (x % (boxSize * 2)) >= boxSize;
                f ^= (y % (boxSize * 2)) >= boxSize;
                pixels[i++] = Utilities.Lerp(a, b, f ? 1 : 0);
            }

        return new Texture(width, height, pixels, false, false);
    }

    /// <summary>
    /// Generate a simple noise texture
    /// </summary>
    public static Texture Noise(int width, int height, float freq, float z, Color a, Color b)
    {
        var pixels = new Color[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var f = Walgelijk.Noise.GetFractal(x * freq / width, y * freq / height, z, 3) * 0.5f + 0.5f;
                pixels[i++] = Utilities.Lerp(a, b, f);
            }

        return new Texture(width, height, pixels, false, false);
    }

    /// <summary>
    /// Generate a flat colour texture
    /// </summary>
    public static Texture Colour(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        Array.Fill(pixels, color);

        return new Texture(width, height, pixels, false, false);
    }


    /// <summary>
    /// Generate a simple gradient texture
    /// </summary>
    public static Texture Gradient(int width, int height, GradientType type, Color a, Color b)
    {
        var pixels = new Color[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var p = new Vector2(x / (float)width, y / (float)width);
                float f = type switch
                {
                    GradientType.Horizontal => p.X,
                    GradientType.Vertical => p.Y,
                    GradientType.Diagonal => (p.X + p.Y) * 0.5f,
                    GradientType.Radial => Utilities.MapRange(-MathF.PI, MathF.PI, 0, 1, MathF.Atan2((p.Y - 0.5f) * .5f, (p.X - 0.5f) * .5f)),
                    GradientType.Spherical => Vector2.Distance(p, new(0.5f)) * 2f,
                    GradientType.Manhattan => MathF.Max(MathF.Abs(.5f - p.Y), MathF.Abs(.5f - p.X)) * 2f,
                    _ => throw new NotImplementedException(),
                };
                pixels[i++] = Utilities.Lerp(a, b, f);
            }

        return new Texture(width, height, pixels, false, false);
    }


    public enum GradientType
    {
        Horizontal,
        Vertical,
        Diagonal,
        Radial,
        Spherical,
        Manhattan,
    }
}
