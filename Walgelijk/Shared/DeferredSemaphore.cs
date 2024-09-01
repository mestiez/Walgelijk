using System;
using System.Threading;

namespace Walgelijk;

public struct DeferredSemaphore : IDisposable
{
    private readonly SemaphoreSlim semaphore;

    public DeferredSemaphore(SemaphoreSlim semaphore)
    {
        this.semaphore = semaphore;
        semaphore.Wait();
    }

    public void Dispose()
    {
        semaphore.Release();
    }
}
