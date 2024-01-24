using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public static class StateDependentExtensions
{
    public static void SetColor(this ref StateDependent<Appearance> app, StateDependent<Color> color)
    {
        app.Default.Color = color.Default;

        if (color.Active.HasValue)
        {
            if (app.Active.HasValue)
                app.Active = app.Active.Value with { Color = color.Active.Value };
            else
                app.Active = app.Default with { Color = color.Active.Value };
        }

        if (color.Hover.HasValue)
        {
            if (app.Hover.HasValue)
                app.Hover = app.Hover.Value with { Color = color.Hover.Value };
            else
                app.Hover = app.Default with { Color = color.Hover.Value };
        }

        if (color.Triggered.HasValue)
        {
            if (app.Triggered.HasValue)
                app.Triggered = app.Triggered.Value with { Color = color.Triggered.Value };
            else
                app.Triggered = app.Default with { Color = color.Triggered.Value };
        }
    }

    public static void SetTexture(this ref StateDependent<Appearance> app, IReadableTexture texture, ImageMode mode)
    {
        app.Default.Texture = texture;
        app.Default.ImageMode = mode;

        if (app.Active.HasValue)
            app.Active = app.Active.Value with { Texture = texture, ImageMode = mode };
        else
            app.Active = app.Default with { Texture = texture, ImageMode = mode };

        if (app.Hover.HasValue)
            app.Hover = app.Hover.Value with { Texture = texture, ImageMode = mode };
        else
            app.Hover = app.Default with { Texture = texture, ImageMode = mode };

        if (app.Triggered.HasValue)
            app.Triggered = app.Triggered.Value with { Texture = texture, ImageMode = mode };
        else
            app.Triggered = app.Default with { Texture = texture, ImageMode = mode };
    }
}
