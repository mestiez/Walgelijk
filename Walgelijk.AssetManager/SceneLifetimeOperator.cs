
namespace Walgelijk.AssetManager;

public class SceneLifetimeOperator : ILifetimeOperator
{
    public static readonly SceneLifetimeOperator Shared = new();

    public Hook Triggered { get; } = new();

    public SceneLifetimeOperator()
    {
        Game.Main.OnSceneChange.AddListener(OnSceneChange);
    }

    private void OnSceneChange((Scene? Old, Scene? New) tuple)
    {
        Triggered.Dispatch();
        //Triggered.ClearListeners();
    }
}
