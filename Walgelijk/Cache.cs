using System;
using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// A generic cache object that provides a way to load heavy objects based on a lighter key
    /// </summary>
    /// <typeparam name="UnloadedType">The key. This object is usually light and cheap to create</typeparam>
    /// <typeparam name="LoadedType">The loaded object. This object is usually heavy and expensive to create</typeparam>
    public abstract class Cache<UnloadedType, LoadedType>
    {
        private readonly Dictionary<UnloadedType, LoadedType> loaded = new Dictionary<UnloadedType, LoadedType>();

        /// <summary>
        /// Load or create a <see cref="LoadedType"/> from an <see rcef="UnloadedType"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public LoadedType Load(UnloadedType obj)
        {
            if (loaded.TryGetValue(obj, out var v))
                return v;

            v = CreateNew(obj);
            loaded.Add(obj, v);
            return v;
        }

        /// <summary>
        /// Determines what must be done when an entirely new <see cref="LoadedType"/> is created
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected abstract LoadedType CreateNew(UnloadedType raw);

        /// <summary>
        /// Dispose of any resources attached to the loaded type. This is invoked when an entry is unloaded.
        /// </summary>
        /// <param name="loaded"></param>
        protected abstract void DisposeOf(LoadedType loaded);

        /// <summary>
        /// Unload an entry and dispose of all attached resources
        /// </summary>
        public void Unload(UnloadedType obj)
        {
            if (!loaded.TryGetValue(obj, out var loadedObj)) throw new ArgumentException("Attempt to unload an entry that isn't loaded");
            DisposeOf(loadedObj);
            loaded.Remove(obj);
        }

        /// <summary>
        /// Returns if an entry is in the cache
        /// </summary>
        public bool Has(UnloadedType obj) => loaded.ContainsKey(obj);

        /// <summary>
        /// Clear the cache
        /// </summary>
        public void UnloadAll()
        {
            foreach (var entry in loaded)
                DisposeOf(entry.Value);
            loaded.Clear();
        }
    }
}
