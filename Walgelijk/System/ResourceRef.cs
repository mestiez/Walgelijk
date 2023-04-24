using System.IO;

namespace Walgelijk;

/// <summary>
/// A resource by key reference. Ensures reimporting is properly handled. Can be returned by the Resources system.
/// </summary>
public readonly struct ResourceRef<T>
{
    /// <summary>
    /// Resource ID
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// File this resource represents
    /// </summary>
    public FileInfo File => Resources.GetFileFromID(Id);

    public ResourceRef(int id)
    {
        Id = id;
    }

    /// <summary>
    /// Get the resource
    /// </summary>
    public T Value => Resources.Load<T>(Id);

    /// <summary>
    /// Unload the resource to force it to be reloaded
    /// </summary>
    public void Reimport()
    {
        Resources.Unload(Value);
    }

    public static implicit operator T(ResourceRef<T> r) => r.Value;
}
