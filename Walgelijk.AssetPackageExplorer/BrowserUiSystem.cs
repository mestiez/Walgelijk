using Walgelijk.AssetManager;
using Walgelijk.Onion;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.AssetPackageExplorer;

public class BrowserUiSystem : Walgelijk.System
{
    const string title = "Walgelijk Asset Package Explorer";

    public override void Initialise()
    {
        Window.Title = title;
        Window.OnFileDrop += OnFileDrop;
    }

    public override void Update()
    {
        if (!Scene.FindAnyComponent<ExplorerComponent>(out var e))
            return;

        if (Input.IsKeyPressed(Key.F5))
            e.FolderChanged = true;

        if (e.FolderChanged)
        {
            DisposeSelectedAsset(e);
            Audio.StopAll();

            e.SelectedAsset = null;
            e.FolderChanged = false;
            if (e.Package != null)
            {
                e.FolderRefreshCancel.Cancel();
                e.FolderRefreshCancel.Dispose();
                e.FolderRefreshCancel = new(TimeSpan.FromSeconds(4));
                e.FolderRefreshCancel.Token.Register(() => e.IsRebuildingFolderCache = false);

                e.IsRebuildingFolderCache = true;
                Task.Run(() =>
                {
                    e.FolderCache = [
                        ..e.Package.GetFoldersIn(e.CurrentPath).Select(f => new Entry(f + '/', e.CurrentPath.TrimEnd('/') + '/' + f)),
                        ..e.Package.EnumerateFolder(e.CurrentPath).Select(i => new Entry(e.Package.GetMetadata(i)))];
                    e.IsRebuildingFolderCache = false;
                },
                e.FolderRefreshCancel.Token);
            }
        }

        if (e.PackageChanged)
        {
            DisposeSelectedAsset(e);
            Audio.StopAll();

            var s = title;
            if (e.Package != null)
                s += " - " + e.Package.Metadata.Id;

            Window.Title = s;
            e.PackageChanged = false;
            e.SelectedAsset = null;
        }

        if (e.Lock.TryEnterReadLock(200))
        {
            try
            {
                ProcessUi(e, e.Package);
            }
            finally
            {
                e.Lock.ExitReadLock();
            }
        }
    }

    private void ProcessUi(ExplorerComponent e, AssetPackage? p)
    {
        const int h = 40;

        Draw.Colour = Colors.Black;
        Draw.ScreenSpace = true;
        Draw.Order = RenderOrder.Zero;
        Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

        Ui.Layout.Width(Window.Width).Height(h);
        Ui.StartGroup(true);
        {
            Ui.Layout.Size(h, h);
            if (Ui.ImageButton(Assets.Load<Texture>("explorer:icon.qoi").Value, ImageContainmentMode.OriginalSize))
            {
                e.CurrentPath = "/";
                e.FolderChanged = true;
            }

            Ui.Layout.Size(h, h).Move(40, 0).Inflate(-8);
            Ui.ImageButton(Assets.Load<Texture>("explorer:left.png").Value, ImageContainmentMode.Contain);

            Ui.Layout.Size(h, h).Move(80, 0).Inflate(-8);
            Ui.ImageButton(Assets.Load<Texture>("explorer:right.png").Value, ImageContainmentMode.Contain);

            Ui.Layout.Size(h, h).Move(120, 0).Inflate(-8);
            if (Ui.ImageButton(Assets.Load<Texture>("explorer:up.png").Value, ImageContainmentMode.Contain))
            {
                if (e.CurrentPath.Contains('/'))
                {
                    e.CurrentPath = e.CurrentPath[..e.CurrentPath.LastIndexOf('/')];
                    e.FolderChanged = true;
                }
            }

            Ui.Layout.Size(Window.Width - h * 4, h).StickRight(false).Inflate(-4, -4);
            Ui.Theme.Padding(5).Once();
            int prev = Hashes.MurmurHash1(e.CurrentPath);
            if (Ui.StringInputBox(ref e.CurrentPath, new("/")))
                e.FolderChanged = prev != Hashes.MurmurHash1(e.CurrentPath);
        }
        Ui.End();

        if (!e.IsRebuildingFolderCache)
        {
            Ui.Layout.Size(Window.Width / 2, Window.Height - h).StickBottom(false);
            Ui.Theme.Padding(5).Once();
            Ui.StartScrollView();
            {
                int i = 0;
                Ui.Theme.SetAll(t =>
                {
                    t.Foreground = new(default, Colors.White.WithAlpha(0.1f), Colors.White.WithAlpha(0.05f));
                    t.Padding = 5;
                    return t;
                });

                Ui.Theme.Push();

                foreach (var entry in e.FolderCache)
                {
                    Ui.Layout.FitWidth().Height(h).CenterHorizontal().Move(0, i * h + 5);
                    bool selected = e.SelectedAsset.HasValue && entry.Asset.HasValue && e.SelectedAsset.Value.Id == entry.Asset.Value.Id;

                    if (selected)
                        Ui.Theme.ForegroundColor(Colors.White.WithAlpha(0.15f)).Once();

                    if (EntryButton.Start(entry.Name, i))
                    {
                        Audio.StopAll();
                        if (entry.Destination != null)
                        {
                            e.CurrentPath = entry.Destination;
                            e.FolderChanged = true;
                        }
                        else if (entry.Asset.HasValue)
                        {
                            DisposeSelectedAsset(e);

                            e.SelectedAsset = entry.Asset;
                            if (e.Package != null)
                            {
                                var metadata = entry.Asset.Value;

                                if (metadata.MimeType.Contains("image"))
                                    e.SelectedLoadedAsset = e.Package.Load<Texture>(entry.Asset.Value.Id);
                                else if (metadata.MimeType.Contains("ogg"))
                                    e.SelectedLoadedAsset = e.Package.Load<StreamAudioData>(entry.Asset.Value.Id);
                                else if (metadata.MimeType.Contains("audio"))
                                    e.SelectedLoadedAsset = e.Package.Load<FixedAudioData>(entry.Asset.Value.Id);
                                else if (metadata.MimeType.Contains("text") || metadata.MimeType.Contains("glsl") || metadata.Path.EndsWith("json"))
                                    e.SelectedLoadedAsset = e.Package.Load<string>(entry.Asset.Value.Id);
                                else
                                    e.SelectedLoadedAsset = null;
                            }
                        }
                    }

                    i++;
                }
                Ui.Theme.Pop();
            }
            Ui.End();

            if (e.SelectedAsset.HasValue && e.Package != null)
            {
                Ui.Layout.Size(Window.Width / 2, Window.Height - h).StickRight(false).StickBottom(false).Inflate(-10);
                Ui.Theme.Padding(5).Once();
                Ui.StartScrollView();
                {
                    switch (e.SelectedLoadedAsset)
                    {
                        case AudioData audio:
                            Ui.Layout.FitContainer(0.5f, null).Height(h).CenterHorizontal().Inflate(-10);
                            if (Ui.Button("Play"))
                            {
                                Audio.Play(new Sound(audio));
                            }
                            break;    
                        case Texture tex:
                            Ui.Layout.FitContainer().Center().Inflate(-10);
                            Ui.Image(tex, ImageContainmentMode.Contain);
                            break;
                        case string value:
                            Ui.Layout.FitWidth().PreferredSize().MinSize(64, 64);
                            Ui.Theme.Text(Colors.Orange).Once();
                            Ui.TextRect(value, HorizontalTextAlign.Left, VerticalTextAlign.Top);
                            break;
                        default:
                            Ui.Layout.FitWidth().Height(h).CenterHorizontal();
                            Ui.TextRect("Asset is binary data", HorizontalTextAlign.Center, VerticalTextAlign.Middle);

                            if (e.SelectedAsset.Value.Size <= 5000)
                            {
                                Ui.Layout.FitContainer(0.5f, null).Height(h).CenterHorizontal().Move(0, h).Inflate(-10);
                                if (Ui.Button("Show as text"))
                                {
                                    e.SelectedLoadedAsset = e.Package.LoadNoCache<string>(e.SelectedAsset.Value.Id);
                                }
                            }
                            break;
                    }
                }
                Ui.End();
            }
        }
    }

    private static void DisposeSelectedAsset(ExplorerComponent e)
    {
        if (e.Package != null && e.SelectedAsset.HasValue)
            e.Package.DisposeOf(e.SelectedAsset.Value.Id);

        if (e.SelectedLoadedAsset is IDisposable d)
            d.Dispose();

        e.SelectedLoadedAsset = null;
    }

    private void OnFileDrop(object? sender, string[] e)
    {
        if (e.Length == 0 || !Scene.FindAnyComponent<ExplorerComponent>(out var ex))
            return;

        Task.Run(() =>
        {
            if (ex.Lock.TryEnterWriteLock(1000))
            {
                try
                {
                    var o = ex.Package;
                    ex.Package = null;
                    o?.Dispose();

                    ex.Package = new AssetPackage(e[0]);
                    ex.PackageChanged = true;
                    ex.FolderChanged = true;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                finally
                {
                    ex.Lock.ExitWriteLock();
                }
            }
        });
    }
}
