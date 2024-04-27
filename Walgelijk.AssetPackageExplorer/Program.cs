using System.Numerics;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;
using Walgelijk.CommonAssetDeserialisers;
using Walgelijk.CommonAssetDeserialisers.Audio;
using Walgelijk.CommonAssetDeserialisers.Audio.Qoa;
using Walgelijk.Onion;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;

using Uiw = Walgelijk.Onion.Onion;

namespace Walgelijk.AssetPackageExplorer;

// TODO
// - display all metadata (assets & entire package)
// - extract buttons (per file, per folder, all)
// - async asset loading
// - asset loading error handling

public class Program : IDisposable
{
    private static readonly Game game = new(
        new OpenTKWindow("Asset package explorer", default, new Vector2(800, 600)),
        new OpenALAudioRenderer());

    static void Main(string[] args)
    {
        game.UpdateRate = 200;
        game.FixedUpdateRate = 20;
        game.Profiling.DrawQuickProfiler = false;

        Assets.RegisterPackage("explorer.waa");
        AssetDeserialisers.Register(new FontDeserialiser());
        AssetDeserialisers.Register(new OggFixedAudioDeserialiser());
        AssetDeserialisers.Register(new OggStreamAudioDeserialiser());
        AssetDeserialisers.Register(new WaveFixedAudioDeserialiser());
        AssetDeserialisers.Register(new QoaFixedAudioDeserialiser());

        var mainColour = new Color(0.81f, 0.52f, 1f, 1f);
        ref var theme = ref Ui.Theme.Base;
        theme.Font = Assets.Load<Font>("explorer:cascadia-mono.wf");
        theme.Background = new(mainColour.Brightness(0.6f));
        theme.Foreground = new(mainColour, mainColour.Brightness(1.1f), mainColour.Brightness(0.9f));
        theme.ScrollbarBackground = mainColour.Brightness(0.6f);
        theme.ScrollbarTracker = new(mainColour, mainColour.Brightness(1.2f).Saturation(1.2f), mainColour.Brightness(0.8f));
        theme.FocusBoxWidth = 0;
        theme.OutlineWidth = 0;
        theme.Padding = 0;
        theme.Rounding = 0;
        theme.FontSize = 16;
        Uiw.Configuration.RenderLayer = 10;
        Uiw.Configuration.ScrollSensitivity = 40;
        Uiw.ActiveSound = null;
        Ui.Animation.DefaultDurationSeconds = 0;

        Draw.CacheTextMeshes = -1;

        var s = game.Scene = new Scene("main");
        var e = s.AttachComponent(s.CreateEntity(), new ExplorerComponent());

        s.AddSystem(new OnionSystem());
        s.AddSystem(new BrowserUiSystem());

        if (args.Length > 0)
        {
            Task.Run(() =>
            {
                e.Lock.EnterWriteLock();
                try
                {
                    e.Package = new AssetPackage(args[0]);
                    e.PackageChanged = true;
                    e.FolderChanged = true;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
                finally
                {
                    e.Lock.ExitWriteLock();
                }
            });
        }

        game.Start();
    }

    public void Dispose()
    {
        game.Stop();
    }
}
