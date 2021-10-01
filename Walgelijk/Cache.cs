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
        protected readonly Dictionary<UnloadedType, LoadedType> loaded = new Dictionary<UnloadedType, LoadedType>();

        /// <summary>
        /// Load or create a <typeparamref name="LoadedType"/> from an <typeparamref name="UnloadedType"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual LoadedType Load(UnloadedType obj)
        {
            if (loaded.TryGetValue(obj, out var v))
                return v;

            v = CreateNew(obj);
            loaded.Add(obj, v);
            return v;
        }

        /// <summary>
        /// Determines what must be done when an entirely new <typeparamref name="LoadedType"/> is created
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
            if (!loaded.TryGetValue(obj, out var loadedObj))
            {
                Logger.Error($"Attempt to unload a(n) {typeof(UnloadedType).Name} that isn't loaded");
                return;
            }

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

        /// <summary>
        /// Returns every loaded item in the cache
        /// </summary>
        public IEnumerable<LoadedType> GetAllLoaded()
        {
            foreach (var item in loaded)
                yield return item.Value;
        }

        /// <summary>
        /// Returns every unloaded item ever loaded
        /// </summary>
        public IEnumerable<UnloadedType> GetAllUnloaded()
        {
            foreach (var item in loaded)
                yield return item.Key;
        }
    }
}
