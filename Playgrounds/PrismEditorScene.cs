using Walgelijk;

namespace Playgrounds;

public struct PrismEditorScene : ISceneCreator
{
    public Scene Load(Game game) => Walgelijk.Prism.Editor.MapEditorScene.LoadScene(game);
}
