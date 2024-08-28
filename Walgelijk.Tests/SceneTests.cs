using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Walgelijk;

namespace Tests;

[TestClass]
 public class SceneTests
{
    //[TestMethod]
    //TODO dit is oud en moet opnieuw
    public void EntityDeleteMemoryTest()
    {
        const int entityCount = 5000;
        const float diffThreshold = 0.1f;

        Scene scene = new Scene();

        long initialMemory = GC.GetTotalMemory(true);

        for (int i = 0; i < entityCount; i++)
        {
            var e = createEntity();
            scene.UpdateSystems();
            scene.RemoveEntity(e);
            scene.UpdateSystems();
        }

        scene.UpdateSystems();
        GC.Collect(100, GCCollectionMode.Forced, true);

        long afterMemory = GC.GetTotalMemory(true);

        long diff = Math.Abs(afterMemory - initialMemory);

        string mess = $"Difference is {diff}. Before is {initialMemory}, after is {afterMemory}";
        Assert.IsTrue(diff / initialMemory < diffThreshold, mess);
        Console.WriteLine(mess);

        Entity createEntity()
        {
            var ent = scene.CreateEntity();

            scene.AttachComponent(ent, new TransformComponent());
            scene.AttachComponent(ent, new CameraComponent());

            return ent;
        }
    }

    //[TestMethod]
    //TODO dit is oud en moet opnieuw
    public void DetachComponentMemoryTest()
    {
        const int entityCount = 5000;
        const float diffThreshold = 0.1f;

        Scene scene = new Scene();

        long initialMemory = GC.GetTotalMemory(true);

        for (int i = 0; i < entityCount; i++)
        {
            var e = createEntity();
            scene.UpdateSystems();
            scene.DetachComponent<TransformComponent>(e);
            scene.DetachComponent<CameraComponent>(e);
            scene.UpdateSystems();
        }

        GC.Collect(100, GCCollectionMode.Forced, true);

        long afterMemory = GC.GetTotalMemory(true);

        long diff = Math.Abs(afterMemory - initialMemory);

        string mess = $"Difference is {diff}. Before is {initialMemory}, after is {afterMemory}";
        Assert.IsTrue(diff / initialMemory < diffThreshold, mess);
        Console.WriteLine(mess);

        Entity createEntity()
        {
            var ent = scene.CreateEntity();

            scene.AttachComponent(ent, new TransformComponent());
            scene.AttachComponent(ent, new CameraComponent());
            return ent;
        }
    }
}
