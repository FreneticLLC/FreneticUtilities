using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>
/// Micro-utility to cache a single value in an async-safe way.
/// Guarantees that calc-value will only be called once per key, unless the key changes.
/// This is equivalent to:
/// <code>
/// TKey key;
/// TValue value;
/// if (mynewkey == key) return value;
/// else
/// {
///     key = mynewkey;
///     value = calculateValue();
///     return value;
/// }
/// </code>
/// except with async safety, and being self-contained.
/// <para>Note that due to volatile handling, only Object types are supported.
/// ValueTypes are supportable in theory, but in practice require unique codepaths dependent on bitwidth, and therefore are currently excluded from this implementation.</para>
/// </summary>
/// <param name="calculateValueFunc">Function that defines what value applies for a given key.</param>
/// <param name="maxReaders">Maximum parallel reads (that may never overlap with a write).</param>
public class SingleCacheAsync<TKey, TValue>(Func<TKey, TValue> calculateValueFunc, int maxReaders = 10) where TKey : class where TValue : class
{
    /// <summary>The current cache key (if any).</summary>
    public volatile TKey Key;

    /// <summary>The current cache value (if any).</summary>
    public volatile TValue Value;

    /// <summary>Function that calculates a value for a given key.</summary>
    public Func<TKey, TValue> CalculateValueFunc = calculateValueFunc;

    /// <summary>The backing complex lock to ensure stable access.</summary>
    public ManyReadOneWriteLock Lock = new(maxReaders);

    /// <summary>Get the value for the given key, either from cache or a fresh calculation.</summary>
    public TValue GetValue(TKey key)
    {
        // Try a direct read to get
        using (ManyReadOneWriteLock.ReadClaim claim = Lock.LockRead())
        {
            if (key == Key)
            {
                return Value;
            }
        }
        // But if the direct read is a cache miss, need to potentially write
        using (ManyReadOneWriteLock.WriteClaim claim = Lock.LockWrite())
        {
            // Re-check key inside the lock to avoid unwanted recalculation
            if (key == Key)
            {
                return Value;
            }
            Key = key;
            Value = CalculateValueFunc(key);
            return Value;
        }
    }
}
