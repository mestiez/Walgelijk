using System;
using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// Collection that does not allow duplicates and is able to quicky get objects by their type
    /// </summary>
    public class CollectionByType : IDisposable
    {
        private Dictionary<Type, object> table = new Dictionary<Type, object>();
        private HashSet<object> objects = new HashSet<object>();

        /// <summary>
        /// Try to get a value by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T value)
        {
            //check if it just exists in the table
            if (table.TryGetValue(typeof(T), out var untyped))
            {
                value = (T)untyped;
                return true;
            }

            //it doesnt exist in the table, check if the table has any objects that derive from the given type
            foreach (var pair in table)
            {
                if (pair.Value is T typed)
                {
                    TryAdd<T>(typed);
                    value = typed;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Returns if the collection has an object of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>()
        {
            return TryGet<T>(out _);
        }

        /// <summary>
        /// Try to add a value to the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd<T>(object value)
        {
            if (table.TryAdd(typeof(T), value))
            {
                objects.Add(value);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Get all objects regardless of type. Order is undefined.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetAll()
        {
            return objects;
        }

        public void Dispose()
        {
            table.Clear();
            objects.Clear();
            table = null;
            objects = null;
        }
    }
}
