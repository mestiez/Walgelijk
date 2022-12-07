using System;
using System.Collections.Generic;
using System.Linq;

namespace Walgelijk;

internal static class ReflectionCache
{
    private static readonly Dictionary<Type, object[]> attributes = new();
    private static readonly Dictionary<Type, Type[]> baseTypes = new();

    public static void Clear()
    {
        baseTypes.Clear();
        attributes.Clear();
    }

    internal static IEnumerable<Type> GetAllBaseTypes(Type type)
    {
        if (baseTypes.TryGetValue(type, out var bases))
            return bases;

        bases = GetParent(type).ToArray();
        baseTypes.Add(type, bases);

        return bases;

        static IEnumerable<Type> GetParent(Type t)
        {
            if (t.BaseType != null)
            {
                yield return t.BaseType;
                foreach (var parent in GetParent(t.BaseType))
                {
                    yield return parent;
                }
            }
        }
    }

    internal static AttributeType[] GetAttributes<AttributeType, T>() where T : class
    {
        var type = typeof(T);

        if (!attributes.TryGetValue(type, out var attr))
        {
            attr = type.GetCustomAttributes(typeof(AttributeType), true);
            attributes.Add(type, attr);
        }

        return attr as AttributeType[] ?? throw new Exception("Cached attribute array is invalid");
    }
}
