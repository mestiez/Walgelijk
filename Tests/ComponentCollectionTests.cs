using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Walgelijk;

namespace Tests;

[TestClass]
public class ComponentCollectionTests
{
    [TestMethod]
    public void AddRemove()
    {
        IComponentCollection coll = new BasicComponentCollection();
        Assert.AreEqual(0, coll.Count);

        Entity ent1 = IdentityGenerator.Generate();
        Entity ent2 = IdentityGenerator.Generate();

        var transform = coll.Attach(ent1, new TransformComponent());

        Assert.AreEqual(0, coll.Count);
        coll.SyncBuffers();
        Assert.AreEqual(1, coll.Count);
        Assert.IsFalse(coll.Contains<CameraComponent>());
        Assert.IsTrue(coll.Contains<TransformComponent>());
        Assert.AreSame(transform, coll.GetAll().First());

        coll.Remove<CameraComponent>(ent1); // non existent component
        coll.SyncBuffers();
        Assert.AreEqual(1, coll.Count);       
        
        coll.Remove<TransformComponent>(ent2);// wrong entity
        coll.SyncBuffers();
        Assert.AreEqual(1, coll.Count);

        coll.Remove<TransformComponent>(ent1);// wrong entity
        coll.SyncBuffers();
        Assert.AreEqual(0, coll.Count);   
        
        coll.Remove<TransformComponent>(ent1);// try to remove the same thing
        coll.SyncBuffers();
        Assert.AreEqual(0, coll.Count);
    }

    [TestMethod]
    public void Querying()
    {
        IComponentCollection coll = new BasicComponentCollection();
        Assert.AreEqual(0, coll.Count);

        Entity ent1 = IdentityGenerator.Generate();
        Entity ent2 = IdentityGenerator.Generate();
        Entity ent3 = IdentityGenerator.Generate();

        var e1blob = coll.Attach(ent1, new BlobComponent());
        var e1hand = coll.Attach(ent1, new HandComponent());
        var e1breadboard = coll.Attach(ent1, new BreadboardComponent());

        var e2blob = coll.Attach(ent2, new BlobComponent());

        var e3board = coll.Attach(ent3, new BoardComponent());
        var e3blob = coll.Attach(ent3, new BlobComponent());

        Assert.AreEqual(0, coll.Count);
        coll.SyncBuffers();

        Assert.AreEqual(6, coll.Count);
        Assert.AreEqual(ent1, e1blob.Entity);
        Assert.AreEqual(ent1, e1hand.Entity);
        Assert.AreEqual(ent1, e1breadboard.Entity);
        Assert.AreEqual(ent2, e2blob.Entity);
        Assert.AreEqual(ent3, e3board.Entity);
        Assert.AreEqual(ent3, e3blob.Entity);

        var allBlobs = coll.GetAllOfType<BlobComponent>();
        Assert.AreEqual(3, allBlobs.Count());
        Assert.IsTrue(allBlobs.Contains(e1blob));
        Assert.IsTrue(allBlobs.Contains(e2blob));
        Assert.IsTrue(allBlobs.Contains(e3blob));

        var everythingOnEntity1 = coll.GetAllFrom(ent1);
        Assert.AreEqual(3, everythingOnEntity1.Count());
        Assert.IsTrue(everythingOnEntity1.Contains(e1blob));
        Assert.IsTrue(everythingOnEntity1.Contains(e1hand));
        Assert.IsTrue(everythingOnEntity1.Contains(e1breadboard));
        
        var inheritance = coll.GetAllOfType<BoardComponent>();
        Assert.IsTrue(inheritance.Contains(e3board));
        Assert.IsTrue(inheritance.Contains(e1breadboard));
        Assert.AreEqual(2, inheritance.Count());

        coll.Remove<BoardComponent>(ent3);

        coll.SyncBuffers();
        inheritance = coll.GetAllOfType<BoardComponent>();
        Assert.IsTrue(inheritance.Contains(e1breadboard));
        Assert.AreEqual(1, inheritance.Count());
    }

    public class BlobComponent : Component { }
    public class HandComponent : Component { }
    public class BoardComponent : Component { }
    public class BreadboardComponent : BoardComponent { }
}
