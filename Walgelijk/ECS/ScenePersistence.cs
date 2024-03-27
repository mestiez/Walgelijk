namespace Walgelijk;

public enum ScenePersistence
{
    /// <summary>
    /// Dispose the scene and remove it from the scene cache
    /// </summary>
    Dispose,
    /// <summary>
    /// Keep the scene in memory
    /// </summary>
    Persist
}
