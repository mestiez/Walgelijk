using System;
using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// Global path based resource storage
    /// </summary>
    public static class Resources
    {
        private static bool initialised;

        private static readonly Dictionary<Type, Func<string, object>> loadFunctions = new Dictionary<Type, Func<string, object>>();
        private static readonly Dictionary<string, object> resources = new Dictionary<string, object>();

        /// <summary>
        /// Initialise 
        /// </summary>
        internal static void Initialise()
        {
            if (initialised) return;
            initialised = true;

            RegisterType(typeof(Texture), (string path) => Texture.Load(path));
            RegisterType(typeof(Font), Font.Load);
        }

        /// <summary>
        /// Load the resource at the given path. Will throw an exception if there is no resource loader found for the type, or if the file at the path is not of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the object to load</typeparam>
        /// <param name="path">The path of the file</param>
        /// <returns></returns>
        public static T Load<T>(string path)
        {
            if (resources.TryGetValue(path, out var obj) && obj is T typed)
                return typed;

            var newObject = CreateNew(path, typeof(T));
            if (newObject is T result)
            {
                resources.Add(path, result);
                return result;
            }

            throw new Exception($"The object at \"${path}\" is not of type ${typeof(T).Name}");
        }

        /// <summary>
        /// Returns if the resource manager can load objects of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
                return loadFromFile(path);
            else
                throw new Exception($"Could not load \"${path}\": there is no resource loader for type ${type.Name}");
        }
    }
}
