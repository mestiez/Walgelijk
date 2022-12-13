using System;

namespace Walgelijk;

/// <summary>
/// Objects that have data on the local side that is eventually uploaded somewhere else (usually the GPU)
/// </summary>
public interface IExternal<T>
{
    /// <summary>
    /// Remove the copy of the data that is stored locally, usually because it's already been uploaded
    /// </summary>
    public void DisposeLocalCopy();

    /// <summary>
    /// Get the data. This can be null if it's been disposed.
    /// </summary>
    public ReadOnlyMemory<T>? GetData();

    /// <summary>
    /// Should the local copy of this object be disposed after it's been uploaded? 
    /// </summary>
    public bool DisposeLocalCopyAfterUpload { get; }
}
