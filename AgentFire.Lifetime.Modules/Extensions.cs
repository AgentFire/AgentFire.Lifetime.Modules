using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules
{
    internal static class Extensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, Func<TValue> factory)
        {
            return d.TryGetValue(key, out TValue result) ? result : d[key] = factory();
        }
    }
}
