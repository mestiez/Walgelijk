using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.ParticleSystem;

namespace Tests;

[TestClass]
public class SystemCollectionTests
{
    [TestMethod]
    public void AddRemove()
    {
        ISystemCollection coll = new BasicSystemCollection();
        Assert.AreEqual(0, coll.Count);

        coll.Add(new TransformSystem());
        Assert.AreEqual(0, coll.Count);
        Assert.IsFalse(coll.Has<TransformSystem>());
        Assert.IsFalse(coll.Has(typeof(TransformSystem)));

        coll.SyncBuffers();
        Assert.AreEqual(1, coll.Count);
        Assert.IsTrue(coll.Has<TransformSystem>());
        Assert.IsTrue(coll.Has(typeof(TransformSystem)));

        Assert.ThrowsException<DuplicateSystemException>(() =>
        {
            coll.Add(new TransformSystem());
        });

        Assert.IsFalse(coll.Remove<ShapeRendererSystem>()); // do nothing when non existent system is removed
        coll.SyncBuffers();
        Assert.AreEqual(1, coll.Count);
        Assert.IsTrue(coll.Has<TransformSystem>());
        Assert.IsTrue(coll.Has(typeof(TransformSystem)));

        Assert.IsTrue(coll.Remove<TransformSystem>());
        Assert.AreEqual(1, coll.Count);
        Assert.IsTrue(coll.Has<TransformSystem>());
        Assert.IsTrue(coll.Has(typeof(TransformSystem)));
        coll.SyncBuffers();
        Assert.AreEqual(0, coll.Count);
        Assert.IsFalse(coll.Has<TransformSystem>());
        Assert.IsFalse(coll.Has(typeof(TransformSystem)));

        coll.Dispose();
    }

    [TestMethod]
    public void Sort()
    {
        ISystemCollection coll = new BasicSystemCollection();
        Assert.AreEqual(0, coll.Count);

        // added in random order to make sure it has actually been sorted
        var third = coll.Add(new TransformSystem() { ExecutionOrder = 35 });
        var first = coll.Add(new DebugCameraSystem() { ExecutionOrder = 5 });
        var second = coll.Add(new ParticleSystem() { ExecutionOrder = 15 });
        coll.SyncBuffers();

        Assert.AreEqual(3, coll.Count);
        Assert.AreEqual(first, coll.First());
        Assert.AreEqual(second, coll.GetAll()[1]);
        Assert.AreEqual(third, coll.GetAll()[2]);

        Assert.IsTrue(coll.Remove<ParticleSystem>());// remove middle system
        coll.SyncBuffers();

        Assert.AreEqual(2, coll.Count);
        Assert.AreEqual(first, coll.First());
        Assert.AreEqual(third, coll.GetAll()[1]);

        Assert.ThrowsException<IndexOutOfRangeException>(() =>
        {
            Assert.AreEqual(third, coll.GetAll()[5]); // out of range
        });

        coll.Dispose();
    }

    [TestMethod]
    public void GetAll()
    {
        ISystemCollection coll = new BasicSystemCollection();
        Assert.AreEqual(0, coll.Count);
        var third = coll.Add(new TransformSystem() { ExecutionOrder = 35 });
        var first = coll.Add(new DebugCameraSystem() { ExecutionOrder = 5 });
        var second = coll.Add(new ParticleSystem() { ExecutionOrder = 15 });
        coll.SyncBuffers();

        Assert.AreEqual(3, coll.Count);
        Assert.AreEqual(3, coll.GetAll().Length);
        foreach (var item in coll.GetAll())
            Assert.IsTrue(item == first || item == second || item == third);

        var bonus = new Walgelijk.System[3];
        coll.GetAll(bonus);
        Assert.IsTrue(coll.SequenceEqual(bonus));

        bonus = new Walgelijk.System[3];
        coll.GetAll(bonus, 0, 3);
        Assert.IsTrue(coll.SequenceEqual(bonus));

        bonus = new Walgelijk.System[3];
        coll.GetAll(bonus, 1, 1);
        Assert.AreEqual(coll.First(), bonus[1]);

        bonus = new Walgelijk.System[2]; // insufficient length
        coll.GetAll(bonus);
        Assert.IsTrue(coll.ToArray().AsSpan(0, 2).SequenceEqual(bonus));

        bonus = new Walgelijk.System[16]; // excess length
        coll.GetAll(bonus);
        Assert.IsTrue(coll.GetAll().SequenceEqual(bonus.AsSpan(0, coll.Count)));
        Assert.IsTrue(bonus.AsSpan(coll.Count).ToArray().All(a => a == null));
    }

    [TestMethod]
    public void Exceptions()
    {
        ISystemCollection coll = new BasicSystemCollection(4); //small capacity
        Assert.AreEqual(0, coll.Count);
        coll.Add(new TransformSystem());
        coll.Add(new DebugCameraSystem());
        coll.Add(new ParticleSystem());
        coll.SyncBuffers();

        Assert.ThrowsException<DuplicateSystemException>(() =>
        {
            coll.Add(new DebugCameraSystem());
        });

        coll.Add(new GuiSystem());
        coll.SyncBuffers();

        Assert.ThrowsException<Exception>(() => // exceeded capacity
        {
            coll.Add(new ShapeRendererSystem());
        });
    }
}
