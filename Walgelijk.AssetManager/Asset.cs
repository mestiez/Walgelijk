namespace Walgelijk.AssetManager;

/// <summary>
/// Struct that provides objects to access asset data
/// </summary>
public readonly record struct Asset
{
    public readonly AssetMetadata Metadata;
    public readonly Lazy<Stream> Stream;

    public Asset(AssetMetadata metadata, Lazy<Stream> stream)
    {
        Metadata = metadata;
        Stream = stream;
    }
}

public class ReplacementTable
{

}

public interface ILifetimeOperator
{
    Hook Triggered { get; }
}

public class SceneLifetimeOperator : ILifetimeOperator
{
    public Hook Triggered { get; } = new();

    public SceneLifetimeOperator()
    {
        Game.Main.OnSceneChange.AddListener(OnSceneChange);
    }

    private void OnSceneChange((Scene? Old, Scene? New) tuple)
    {
        Triggered.Dispatch();
        Triggered.ClearListeners();
    }
}