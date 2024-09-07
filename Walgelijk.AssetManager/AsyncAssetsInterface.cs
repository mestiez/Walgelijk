using System.Collections.Concurrent;
using Walgelijk.AssetManager.Deserialisers;

/* Je moet detecteren of een asset een andere asset nodig heeft en als dat zo is moet je die eerder laden
 * Dit is nodig omdat er een lock is waardoor je niet assets kan laden terwijl je een asset laadt
 */

namespace Walgelijk.AssetManager;

/// <summary>
/// Provides a way to load assets in the background without having to worry about tasks, async, etc.
/// </summary>
public class AsyncAssetsInterface
{
    private readonly ConcurrentDictionary<GlobalAssetId, IWorker> singleWorkers = [];
    private readonly ConcurrentDictionary<string, IWorker> collectionWorkers = [];

    private SemaphoreSlim workerSetLock = new(1);

    public T? Load<T>(GlobalAssetId id)
    {
        if (Assets.IsCached(id))
            return Assets.Load<T>(id);

        using var l = new DeferredSemaphore(workerSetLock);

        if (singleWorkers.TryGetValue(id, out var worker))
        {
            if (worker.Status == WorkerStatus.Finished)
                singleWorkers.TryRemove(id, out _);

            return worker.Status switch
            {
                WorkerStatus.Finished => Assets.Load<T>(id),
                _ => default,
            };
        }
        else
        {
            worker = new Worker<T>(id);
            if (!singleWorkers.TryAdd(id, worker))
                throw new InvalidOperationException($"Asset \"{id}\" is already being loaded.");

            worker.Start();

            return default;
        }
    }

    /// <summary>
    /// Returns a collection of assets of a given type from the given folder, searching every registered package.
    /// The collection only contains loaded assets.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="folder">Package agnostic folder path</param>
    /// <param name="finished">True if the collection has finished loading</param>
    /// <param name="progress">Value from 0.0 to 1.0 representing the loading progress</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IEnumerable<T> EnumerateFolder<T>(string folder, out bool finished, out float progress)
    {
        using var l = new DeferredSemaphore(workerSetLock);

        finished = false;
        folder = AssetPackageUtils.NormalisePath(folder);
        CollectionWorker<T> coll;

        if (!collectionWorkers.TryGetValue(folder, out var worker))
            worker = new CollectionWorker<T>(folder);

        coll = worker as CollectionWorker<T> ?? throw new Exception($"Found collection worker is not of the correct type. Expected {nameof(CollectionWorker<T>)}, got {worker.GetType()}");

        if (worker.Status == WorkerStatus.Finished)
            collectionWorkers.TryRemove(folder, out _);

        finished = coll.Status == WorkerStatus.Finished;
        progress = coll.Progress;

        return coll.Objects;
    }

    private interface IWorker
    {
        WorkerStatus Status { get; }
        void Start();
    }

    private class CollectionWorker<T>(string path) : IWorker
    {
        public WorkerStatus Status { get; private set; }
        public IReadOnlyList<T> Objects => objects;
        public float Progress => totalObjectCount == 0 ? 0 : currentIndex / (float)totalObjectCount;

        private int currentIndex, totalObjectCount;
        private readonly List<T> objects = [];

        public void Start()
        {
            Status = WorkerStatus.Loading;
            GlobalAssetId[] assets = [.. Assets.EnumerateFolder(path, SearchOption.AllDirectories)];
            currentIndex = 0;
            totalObjectCount = assets.Length;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                foreach (var item in assets)
                {
                    try
                    {
                        var package = Assets.GetPackage(item.External);
                        var asset = package.GetAsset(item.Internal);
                        if (!AssetDeserialisers.HasCandidate<T>(asset))
                        {
                            totalObjectCount--;
                            continue;
                        }
                        var aref = Assets.Load<T>(item);
                        var v = aref.Value;
                        objects.Add(v);
                    }
                    catch (Exception e)
                    {
                        // TODO handle
                        totalObjectCount--;
                    }
                    finally
                    {
                        Interlocked.Increment(ref currentIndex);
                    }
                }

                Status = WorkerStatus.Finished;
            });
        }
    }

    private class Worker<T> : IWorker
    {
        public GlobalAssetId Asset { get; }
        public WorkerStatus Status { get; private set; }

        public Worker(GlobalAssetId asset)
        {
            Asset = asset;
        }

        public void Start()
        {
            if (Status == WorkerStatus.Loading)
                return;

            Status = WorkerStatus.Loading;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var aref = Assets.Load<T>(Asset);
                    _ = aref.Value;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    Status = WorkerStatus.Finished;
                }
            });
        }
    }

    public enum WorkerStatus
    {
        Invalid,
        Loading,
        Finished
    }
}
