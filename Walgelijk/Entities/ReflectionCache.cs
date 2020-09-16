using System;
using System.Collections.Generic;

namespace Walgelijk
{
    internal static class ReflectionCache
    {
        private static readonly Dictionary<Type, object[]> attributes = new Dictionary<Type, object[]>();

        internal static AttributeType[] GetAttributes<AttributeType, T>() where T : class
        {
            var type = typeof(T);

            if (!attributes.TryGetValue(type, out var attr))
            {
                attr = type.GetCustomAttributes(typeof(AttributeType), true);
                attributes.Add(type, attr);
            }

            return attr as AttributeType[];
        }
    }
}
