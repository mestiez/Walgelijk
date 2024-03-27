using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

/// <summary>
/// A dictionary for scenes used by <see cref="Game"/>
/// </summary>
public class SceneCache
{
    private readonly Dictionary<SceneId, Scene> cache = [];

    public void Add(Scene scene) => cache.Add(scene.Id, scene);
    public bool Has(SceneId id) => cache.ContainsKey(id);
    public Scene Get(SceneId id) => cache[id];
    public bool TryGet(SceneId id, [NotNullWhen(true)] out Scene? scene) => cache.TryGetValue(id, out scene);
    public bool Remove(SceneId id) => Remove(id, out _);
    public bool Remove(SceneId id, [NotNullWhen(true)] out Scene? scene)
    {
        if (cache.Remove(id, out scene))
        {
            if (scene.Disposed)
                scene.Dispose();
            return true;
        }
        return false;
    }

    public Scene this[SceneId i]
    {
        get => cache[i];
        set => cache[i] = value;
    }
}
