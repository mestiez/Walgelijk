using System.Collections.Generic;

namespace Walgelijk;

[SingleInstance]
public class BatchRendererStorageComponent : Component
{
    public readonly Dictionary<BatchProfile, Batch> Batches = new();
}
