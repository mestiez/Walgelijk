namespace Walgelijk;

public readonly struct SceneId
{
    public readonly string Id;

    public SceneId(string id)
    {
        Id = id;
    }

    public static implicit operator string(SceneId id) => id.Id;
    public static implicit operator SceneId(string id) => new(id);
}
