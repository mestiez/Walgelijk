﻿namespace Walgelijk;

public class BatchRendererSystem : Walgelijk.System
{
    public override void Render()
    {
        if (!Scene.FindAnyComponent<BatchRendererStorageComponent>(out var storage))
        {
            Logger.Warn($"{nameof(BatchRendererSystem)} needs an entity with {nameof(BatchRendererStorageComponent)}");
            return;
        }

        // TODO remove empty batches

        foreach (var batch in storage.Batches.Values)
            batch.Amount = 0;

        foreach (var item in Scene.GetAllComponentsOfType<BatchedSpriteComponent>())
        {
            if (!item.Visible)
                continue;

            if (item.SyncWithTransform && Scene.TryGetComponentFrom<TransformComponent>(item.Entity, out var tc))
                item.Transform = tc.LocalToWorldMatrix;

            var profile = new BatchProfile(item.Material, item.VertexBuffer, item.Texture, item.RenderOrder);

            if (!storage.Batches.TryGetValue(profile, out var batch))
            {
                batch = new Batch(profile);
                storage.Batches.Add(profile, batch);
            }

            batch.Add(item.Transform, item.Color);
        }

        foreach (var batch in storage.Batches.Values)
        {
            if (batch.Amount == 0)
                continue;

            batch.InstancedVertexBuffer.ExtraDataHasChanged = true;
            RenderQueue.Add(batch, batch.Profile.RenderOrder);
        }
    }
}
