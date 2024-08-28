using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Walgelijk;

namespace Tests;

[TestClass]
public class CacheTests
{
    [TestMethod]
    public void Concurrent()
    {
        var cache = new FactorialCache();

        var task = Task.WhenAll(
             Task.Run(() => cache.Load(3)),
             Task.Run(() => cache.Load(6)),
             Task.Run(() => cache.Load(40)),
             Task.Run(() => cache.Load(92))
         );

        Assert.AreNotEqual(4, cache.GetAllLoaded().Count()); // the calculations arent done yet!! there shouldnt be anything

        task.Wait();

        Assert.AreEqual(4, cache.GetAllLoaded().Count()); // we have the data
    }

    public class FactorialCache : ConcurrentCache<UInt128, UInt128>
    {
        protected override UInt128 CreateNew(UInt128 raw)
        {
            UInt128 x = 1;
            for (UInt128 i = raw; i > 0; i--)
            {
                Thread.Sleep(100);
                x *= i;
            }
            return x;
        }

        protected override void DisposeOf(UInt128 loaded)
        {
        }
    }
}
