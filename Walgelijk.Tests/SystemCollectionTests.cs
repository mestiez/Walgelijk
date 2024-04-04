using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Walgelijk;
using Walgelijk.Onion;
using Walgelijk.ParticleSystem;

namespace Tests;

[TestClass]
public class SystemCollectionTests
{
    private ISystemCollection GetNewCollection() => new BasicSystemCollection();

    [TestMethod]
    public void AddRemove()
    {
        using ISystemCollection coll = GetNewCollection();
        Assert.AreEqual(0, coll.Count);
        Assert.IsFalse(coll.Has<TransformSystem>());
        Assert.IsFalse(coll.Has(typeof(TransformSystem)));

        var transformSystem = coll.Add(new TransformSystem());
        Assert.AreEqual(1, coll.Count);
        Assert.IsTrue(coll.Has<TransformSystem>());
        Assert.IsTrue(coll.Has(typeof(TransformSystem)));
        Assert.AreSame(transformSystem, coll.Get<TransformSystem>());

        Assert.ThrowsException<DuplicateSystemException>(() =>
        {
            coll.Add(new TransformSystem());
        });

        Assert.IsFalse(coll.Remove<ShapeRendererSystem>()); // do nothing when non existent system is removed
        Assert.AreEqual(1, coll.Count);
        Assert.IsTrue(coll.Has<TransformSystem>());
        Assert.IsTrue(coll.Has(typeof(TransformSystem)));

        Assert.IsTrue(coll.Remove<TransformSystem>());
        Assert.AreEqual(0, coll.Count);
        Assert.IsFalse(coll.Has<TransformSystem>());
        Assert.IsFalse(coll.Has(typeof(TransformSystem)));
    }

    [TestMethod]
    public void MidLoopManipulation()
    {
        using ISystemCollection coll = GetNewCollection();
        Assert.AreEqual(0, coll.Count);

        coll.Add(new DebugCameraSystem());
        coll.Add(new TransformSystem());
        Assert.AreEqual(2, coll.Count);
        Assert.IsTrue(coll.Has<DebugCameraSystem>());
        Assert.IsTrue(coll.Has<TransformSystem>());

        foreach (var sys in coll)
        {
            if (sys is DebugCameraSystem) //gewoon iets zodat het maar 1 keer gebeurt
            {
                Assert.IsTrue(coll.Remove<DebugCameraSystem>());
                coll.Add(new ParticleSystem());
                coll.Add(new ShapeRendererSystem());
            }
        }

        Assert.AreEqual(3, coll.Count);
        Assert.IsFalse(coll.Has<DebugCameraSystem>());
        Assert.IsTrue(coll.Has<ShapeRendererSystem>());
        Assert.IsTrue(coll.Has<ParticleSystem>());
    }

    [TestMethod]
    public void Sort()
    {
        using ISystemCollection coll = GetNewCollection();
        Assert.AreEqual(0, coll.Count);

        // added in random order to make sure it has actually been sorted
        var third = coll.Add(new TransformSystem() { ExecutionOrder = 35 });
        var first = coll.Add(new DebugCameraSystem() { ExecutionOrder = 5 });
        var second = coll.Add(new ParticleSystem() { ExecutionOrder = 15 });
        coll.Sort();

        Assert.AreEqual(3, coll.Count);
        Assert.AreEqual(first, coll.First());
        Assert.AreEqual(second, coll.GetAll()[1]);
        Assert.AreEqual(third, coll.GetAll()[2]);

        Assert.IsTrue(coll.Remove<ParticleSystem>());// remove middle system
        coll.Sort();

        Assert.AreEqual(2, coll.Count);
        Assert.AreEqual(first, coll.First());
        Assert.AreEqual(third, coll.GetAll()[1]);

        Assert.ThrowsException<IndexOutOfRangeException>(() =>
        {
            Assert.AreEqual(third, coll.GetAll()[5]); // out of range
        });
    }

    [TestMethod]
    public void GetAll()
    {
        using ISystemCollection coll = GetNewCollection();
        Assert.AreEqual(0, coll.Count);
        var third = coll.Add(new TransformSystem() { ExecutionOrder = 35 });
        var first = coll.Add(new DebugCameraSystem() { ExecutionOrder = 5 });
        var second = coll.Add(new ParticleSystem() { ExecutionOrder = 15 });

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
    public void BasicSystemExceptions()
    {
        using ISystemCollection coll = new BasicSystemCollection(4); //small capacity
        Assert.AreEqual(0, coll.Count);
        coll.Add(new TransformSystem());
        coll.Add(new DebugCameraSystem());
        coll.Add(new ParticleSystem());

        Assert.ThrowsException<DuplicateSystemException>(() =>
        {
            coll.Add(new DebugCameraSystem());
        });

        coll.Add(new OnionSystem());

        Assert.ThrowsException<Exception>(() => // exceeded capacity
        {
            coll.Add(new ShapeRendererSystem());
        });
    }
}
