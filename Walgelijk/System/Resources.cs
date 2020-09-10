using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Walgelijk
{
    /// <summary>
    /// Global path based resource storage
    /// </summary>
    public static class Resources
    {
        private static bool initialised;
        private static readonly Stopwatch stopwatch = new Stopwatch();

        private static readonly Dictionary<Type, Func<string, object>> loadFunctions = new Dictionary<Type, Func<string, object>>();
        private static readonly Dictionary<string, object> resources = new Dictionary<string, object>();

        private static readonly Dictionary<Type, string> basePathByType = new Dictionary<Type, string>();

        /// <summary>
        /// Event invoked when 
        /// </summary>
        public static event Action<Type, string> OnStartLoad;

        /// <summary>
        /// Base path of all resource requests
        /// </summary>
        public static string BasePath { get; set; } = ".\\resources\\";

        /// <summary>
        /// Initialise 
        /// </summary>
        internal static void Initialise()
        {
            if (initialised) return;
            initialised = true;

            RegisterType(typeof(Texture), (string path) => Texture.Load(path));
            RegisterType(typeof(IReadableTexture), (string path) => Texture.Load(path));
            RegisterType(typeof(IWritableTexture), (string path) => Texture.Load(path));
            RegisterType(typeof(Font), Font.Load);
            RegisterType(typeof(string), File.ReadAllText);
            RegisterType(typeof(string[]), File.ReadAllLines);
            RegisterType(typeof(byte[]), File.ReadAllBytes);
            RegisterType(typeof(Prefab), (string path) => PrefabLoader.Load(path));
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
            {
                path = CombineTypeSpecificPath<T>(path);
                path = CombineBasePath(path);
            }

            if (resources.TryGetValue(path, out var obj) && obj is T typed)
                return typed;

            var newObject = CreateNew(path, typeof(T));
            if (newObject is T result)
            {
                resources.Add(path, result);
                return result;
            }

            throw new Exception($"The object at \"{path}\" is not of type {typeof(T).Name}");
        }

        private static string CombineTypeSpecificPath<T>(string path)
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

        private static object CreateNew(string path, Type type)
        {
            if (loadFunctions.TryGetValue(type, out var loadFromFile))
            {
                stopwatch.Restart();
                var result = loadFromFile(path);
                stopwatch.Stop();
                Logger.Log($"{type.Name} resource loaded at \"{path}\" ({Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2)}ms)", nameof(Resources));
                return result;
            }
            else
                throw new Exception($"Could not load \"{path}\": there is no resource loader for type {type.Name}");
        }
    }
}
