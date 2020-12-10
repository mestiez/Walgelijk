using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Walgelijk
{
    /// <summary>
    /// Collection that is able to quicky get objects by their type
    /// </summary>
    public class CollectionByType : IDisposable
    {
        private Dictionary<Type, IEnumerable> objectCollectionByType = new Dictionary<Type, IEnumerable>();
        private HashSet<object> objects = new HashSet<object>();

        /// <summary>
        /// Try to get a value by type
        /// </summary>
        public bool TryGet<T>(out T value)
        {
            //het bestaat ergens
            if (objectCollectionByType.TryGetValue(typeof(T), out var untypedSet))
            {
                var set = untypedSet as HashSet<T>;
                if (set.Count > 0)
                {
                    value = set.First();
                    return true;
                }
            }

            //het bestaat nergens maar misschien is er een type dat er van derived
            foreach (var pair in objectCollectionByType)
                foreach (var item in pair.Value)
                    if (item is T typed)
                    {
                        TryAdd<T>(typed);
                        value = typed;
                        return true;
                    }

            value = default;
            return false;
        }

        /// <summary>
        /// Try to get values by type
        /// </summary>
        public bool TryGetAll<T>(out IEnumerable<T> value)
        {
            //het bestaat ergens
            if (objectCollectionByType.TryGetValue(typeof(T), out var untypedSet))
            {
                var set = untypedSet as HashSet<T>;
                value = set;
                return true;
            }

            //het bestaat nergens maar misschien is er een type dat er van derived
            HashSet<T> newSet = new HashSet<T>();
            foreach (var pair in objectCollectionByType)
                foreach (var item in pair.Value)
                    if (item is T typed)
                        newSet.Add(typed);

            objectCollectionByType.Add(typeof(T), newSet);
            value = newSet;

            return true;
        }

        /// <summary>
        /// Returns if the collection has an object of the given type
        /// </summary>
        public bool Has<T>()
        {
            return TryGet<T>(out _);
        }

        /// <summary>
        /// Try to add a value to the collection
        /// </summary>
        public bool TryAdd<T>(object value)
        {
            if (objectCollectionByType.TryGetValue(typeof(T), out var set))
                return (set as HashSet<T>).Add((T)value) && objects.Add(value);

            if (objectCollectionByType.TryAdd(typeof(T), new HashSet<T>()))
                return GetObjectCollection<T>().Add((T)value) && objects.Add(value);

            return false;
        }

        /// <summary>
        /// Get all objects regardless of type. Order is undefined.
        /// </summary>
        public IEnumerable<object> GetAll()
        {
            return objects;
        }

        /// <summary>
        /// Remove all objects by type
        /// </summary>
        public bool Remove<T>()
        {
            //TODO en dit... dit is langzaam. er moet een of andere index based bedoeling zijn hier waar de dictionary alleen ints opslaat die refereren naar een index in een lijst met components
            foreach (var item in objectCollectionByType)
            {
                var set = item.Value as HashSet<T>;
                set.RemoveWhere(c => c is T);
                objects.RemoveWhere(c => c is T);
            }
            return true;
        }

        /// <summary>
        /// Clear everything
        /// </summary>
        public void Dispose()
        {
            //TODO dit was toch erg, laten we hopen dat GC er lief mee omgaat :(
            //foreach (var item in objectCollectionByType)
            //{
            //    dynamic set = item.Value;
            //    set.Clear();
            //}

            GC.SuppressFinalize(this);

            objectCollectionByType.Clear();
            objectCollectionByType = null;
            objects.Clear();
            objects = null;
        }

        private HashSet<T> GetObjectCollection<T>()
        {
            return objectCollectionByType[typeof(T)] as HashSet<T>;
        }
    }
}
