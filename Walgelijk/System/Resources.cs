using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Walgelijk;

/// <summary>
/// Global file based resource storage
/// </summary>
public static class Resources
{
    private static bool initialised;
    private static readonly Stopwatch stopwatch = new();
    private static readonly ConcurrentDictionary<Type, Func<string, object>> loadFunctions = new();
    private static readonly ConcurrentDictionary<int, object> resources = new();
    private static readonly ConcurrentDictionary<Type, string> basePathByType = new();

    private static readonly ConcurrentDictionary<string, int> ids = new();
    private static readonly ConcurrentDictionary<int, FileInfo> fileById = new();

    /// <summary>
    /// Event invoked when a resource has been requested
    /// </summary>
    public static event Action<Type, string>? OnStartLoad;

    /// <summary>
    /// Base path of all resource requests
    /// </summary>
    public static string BasePath { get; set; } = "resources/";

    /// <summary>
    /// Initialise 
    /// </summary>
    internal static void Initialise()
    {
        if (initialised)
            return;
        initialised = true;

        RegisterType(typeof(Texture), (string path) => TextureLoader.FromFile(path));
        RegisterType(typeof(Font), Font.Load);
        RegisterType(typeof(string), File.ReadAllText);
        RegisterType(typeof(string[]), File.ReadAllLines);
        RegisterType(typeof(byte[]), File.ReadAllBytes);
    }

    /// <summary>
    /// Load a resource directly by ID. Use <see cref="GetID(string)"/> to get the ID for a given file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T Load<T>(int id)
    {
        if (resources.TryGetValue(id, out var obj))
        {
            var file = GetFileFromID(id);
            OnStartLoad?.Invoke(typeof(T), file.FullName);
            if (obj is T typed)
                return typed;
            else
                throw new Exception($"The object at \"{id}\" is not of type {typeof(T).Name}. It is {obj.GetType().Name}");
        }
        throw new Exception($"Resource with ID {id} coul not be found");
    }

    /// <summary>
    /// Load a <see cref="ResourceRef{T}"/> at the given path.
    /// The advantage this offers over <see cref="Load{T}(string, bool)"/> is that a <see cref="ResourceRef{T}"/> doesn't actually store the resource, but only the ID and a helper function to retrieve the resource.
    /// This improves support for unloading and hotloading.
    /// </summary>
    /// <typeparam name="T">The type of the object to load</typeparam>
    /// <param name="path">The path of the file</param>
    /// <param name="ignoreBasePaths">Whether or not to ignore any set base paths. Default is false</param>
    public static ResourceRef<T> LoadRef<T>(string path, bool ignoreBasePaths = false)
    {
        if (!ignoreBasePaths)
            path = ParseFullPathForType<T>(path);
        return new ResourceRef<T>(GetID(Path.GetFullPath(path)));
    }

    /// <summary>
    /// Load the resource at the given path. Will throw an exception if there is no resource loader found for the type, or if the file at the path is not of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the object to load</typeparam>
    /// <param name="path">The path of the file</param>
    /// <param name="ignoreBasePaths">Whether or not to ignore any set base paths. Default is false</param>
    public static T Load<T>(string path, bool ignoreBasePaths = false)
    {
        if (!ignoreBasePaths)
            path = ParseFullPathForType<T>(path);

        path = Path.GetFullPath(path);
        var id = GetID(path);
        OnStartLoad?.Invoke(typeof(T), path);

        if (resources.TryGetValue(id, out var obj))
        {
            if (obj is T typed)
                return typed;
            else
                throw new Exception($"The object at \"{path}\" is not of type {typeof(T).Name}. It is {obj.GetType().Name}");
        }

        var newObject = CreateNew(path, typeof(T));
        if (newObject is T result)
        {
            if (!resources.TryAdd(id, result))
                throw new Exception("Failed to add the resource to the resource map");
            return result;
        }

        throw new Exception($"The object at \"{path}\" is not of type {typeof(T).Name}. It is {newObject.GetType().Name}");
    }

    /// <summary>
    /// Return the full path to a resource. Returns null if it could not be found.
    /// </summary>
    public static string? GetPathAssociatedWith(object obj)
    {
        foreach (var item in resources)
            if (item.Value == obj)
                return GetFileFromID(item.Key).FullName;
        return null;
    }

    /// <summary>
    /// Get the full path for a path, considering its type and set base paths
    /// </summary>
    public static string ParseFullPathForType<T>(string path) => CombineBasePath(CombinePathForType<T>(path));

    private static string CombinePathForType<T>(string path)
    {
        if (basePathByType.TryGetValue(typeof(T), out var typeSpecificBasePath))
            path = Path.Combine(typeSpecificBasePath, path);
        return path;
    }

    private static string CombineBasePath(string path)
    {
        path = Path.Combine(BasePath, path);
        return path;
    }

    /// <summary>
    /// Returns all IDs without duplicates
    /// </summary>
    public static IEnumerable<int> GetAllIDs() => fileById.Keys;

    /// <summary>
    /// Returns the ID for the object at the given path. 
    /// This function does not take any of the base paths into account. 
    /// You can provide absolute paths or paths relative to the game executable.
    /// </summary>
    public static int GetID(string path)
    {
        if (ids.TryGetValue(path, out var id))
            return id;

        var file = new FileInfo(path);
        id = Hashes.MurmurHash1(file.FullName);

        if (!fileById.ContainsKey(id))
            if (!fileById.TryAdd(id, file))
                throw new Exception("Failed to add FileInfo for resource to the id file cache");

        if (!ids.TryAdd(path, id))
            throw new Exception("Failed to add resource path ID to id cache");
        return id;
    }

    /// <summary>
    /// Returns the <see cref="FileInfo"/> associated with the resource id
    /// </summary>
    public static FileInfo GetFileFromID(int id)
    {
        if (fileById.TryGetValue(id, out var file))
            return file;

        throw new Exception($"No resource with ID {id} found");
    }

    /// <summary>
    /// Gets the base path for a specific type <b>without the BasePath</b>
    /// </summary>
    public static bool TryGetBasePathForType(Type type, [NotNullWhen(true)] out string? path)
    {
        return basePathByType.TryGetValue(type, out path);
    }

    /// <summary>
    /// Sets the base path for a specific type. This will be combined with the <see cref="BasePath"/> and the input path to create the full path
    /// </summary>
    public static void SetBasePathForType(Type type, string basePath)
    {
        if (!basePathByType.TryAdd(type, basePath))
            basePathByType[type] = basePath;
    }

    /// <summary>
    /// Sets the base path for a specific type. This will be combined with the <see cref="BasePath"/> and the input path to create the full path. This method is the generic version of <see cref="SetBasePathForType(Type, string)"/>
    /// </summary>
    public static void SetBasePathForType<T>(string basePath)
    {
        var type = typeof(T);

        if (!basePathByType.TryAdd(type, basePath))
            basePathByType[type] = basePath;
    }

    /// <summary>
    /// Returns if the resource manager can load objects of the given type
    /// </summary>
    public static bool CanLoad(Type type)
    {
        return loadFunctions.ContainsKey(type);
    }

    /// <summary>
    /// Register a resource type with its loader
    /// </summary>
    /// <param name="type">Type of the resource</param>
    /// <param name="loadFunction">The function that returns the object given a path</param>
    /// <returns>Whether the registration succeeded</returns>
    public static bool RegisterType(Type type, Func<string, object> loadFunction)
    {
        return loadFunctions.TryAdd(type, loadFunction);
    }

    /// <summary>
    /// Unloads an asset. Removes it from the cache and also disposes of it if it implements <see cref="IDisposable"/>
    /// </summary>
    public static void Unload(int id)
    {
        if (resources.TryGetValue(id, out var obj))
        {
            if (obj is IDisposable disp)
                disp.Dispose();

            resources.Remove(id, out _);
        }
    }

    /// <summary>
    /// Unloads an asset. Removes it from the cache and also disposes of it if it implements <see cref="IDisposable"/>
    /// </summary>
    public static void Unload(string key)
    {
        Unload(GetID(key));
    }

    /// <summary>
    /// Unloads an asset. Removes it from the cache and also disposes of it if it implements <see cref="IDisposable"/>
    /// </summary>
    public static void Unload<T>(T resource)
    {
        foreach (var item in resources)
        {
            if (item.Value is T typed && typed.Equals(resource))
            {
                Unload(item.Key);
                return;
            }
        }
    }

    private static object CreateNew(string path, Type type)
    {
        if (loadFunctions.TryGetValue(type, out var loadFromFile))
        {
            var norm = Path.GetFileName(path);
            stopwatch.Restart();
            var result = loadFromFile(path);
            stopwatch.Stop();
            Logger.Log($"{type.Name} resource loaded at \"{norm}\" ({Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2)}ms)", nameof(Resources));
            return result;
        }
        else
            throw new Exception($"Could not load \"{path}\": there is no resource loader for type {type.Name}");
    }
}
