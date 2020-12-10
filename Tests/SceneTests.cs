using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Walgelijk;

namespace Tests
{
    [TestClass]
    public class SceneTests
    {
        [TestMethod]
        public void EntityDeleteMemoryTest()
        {
            const int entityCount = 50000;
            const int componentCount = 100;
            const int diffThreshold = 3000;

            Scene scene = new Scene();

            long initialMemory = GC.GetTotalMemory(true);

            for (int i = 0; i < entityCount; i++)
            {
                var e = createEntity(componentCount);
                scene.RemoveEntity(e);
            }

            GC.Collect();

            long afterMemory = GC.GetTotalMemory(true);

            long diff = Math.Abs(afterMemory - initialMemory);

            Assert.IsTrue(diff < diffThreshold, $"Difference is {diff}. Before is {initialMemory}, after is {afterMemory}");

            Entity createEntity(int c)
            {
                var ent = scene.CreateEntity();

                for (int i = 0; i < c; i++)
                {
                  //  scene.AttachComponent(ent, new TransformComponent());
                  //  scene.AttachComponent(ent, new CameraComponent());
                }

                return ent;
            }
        }
    }
}
