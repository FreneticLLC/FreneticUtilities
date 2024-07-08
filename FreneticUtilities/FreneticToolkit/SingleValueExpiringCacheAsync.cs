using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>
/// Micro-utility to cache a single value in an async-safe way.
/// Guarantees that calc-value will only be called once per expiration.
/// This is equivalent to:
/// <code>
/// long time;
/// TValue value;
/// if (now() - time &lt; expireTime) return value;
/// else
/// {
///     time = now();
///     value = calculateValue();
///     return value;
/// }
/// </code>
/// except with async safety, and being self-contained.
/// <para>Note that due to volatile handling, only Object types are supported.
/// ValueTypes are supportable in theory, but in practice require unique codepaths dependent on bitwidth, and therefore are currently excluded from this implementation.</para>
/// <para>Note the expiration duration is calculated based on <see cref="Environment.TickCount64"/>, a stable system timer unaffected by real date/time.</para>
/// </summary>
/// <param name="calculateValueFunc">Function that defines the current value.</param>
/// <param name="expireTime">After how much time delay should the value be considered expired.</param>
/// <param name="maxReaders">Maximum parallel reads (that may never overlap with a write).</param>
public class SingleValueExpiringCacheAsync<TValue>(Func<TValue> calculateValueFunc, TimeSpan expireTime, int maxReaders = 10) where TValue : class
{
    /// <summary>The time the value was last updated (<see cref="Environment.TickCount64"/>).</summary>
    public long TimeValueUpdated = 0;

    /// <summary>The current cache value (if any).</summary>
    public volatile TValue Value;

    /// <summary>After how much time delay should the value be considered expired.</summary>
    public TimeSpan ExpireTime = expireTime;

    /// <summary>Function that calculates the new value.</summary>
    public Func<TValue> CalculateValueFunc = calculateValueFunc;

    /// <summary>Maximum number of readers allowed at once.</summary>
    public int MaxReaders = maxReaders;

    /// <summary>Read semaphore to limit reading from happening at the same time as writing, but allow parallel reads.</summary>
    public SemaphoreSlim ReadSemaphore = new(maxReaders, maxReaders);

    /// <summary>Write semaphore to limit writes from overlapping.</summary>
    public SemaphoreSlim WriteSemaphore = new(1, 1);

    /// <summary>Forces the value to immediately be considered expired.</summary>
    public void ForceExpire()
    {
        Interlocked.Exchange(ref TimeValueUpdated, 0);
    }

    /// <summary>Get the current value, either from cache or a fresh calculation.</summary>
    public TValue GetValue()
    {
        ReadSemaphore.Wait();
        try
        {
            long read = Interlocked.Read(ref TimeValueUpdated);
            if (read != 0 && Environment.TickCount64 - read < ExpireTime.TotalMilliseconds)
            {
                return Value;
            }
            ReadSemaphore.Release();
            WriteSemaphore.Wait();
            for (int i = 0; i < MaxReaders; i++) // We need to write, so block all reads (excluding self)
            {
                ReadSemaphore.Wait();
            }
            try
            {
                // Double-check to avoid multi-call of the calc func
                read = Interlocked.Read(ref TimeValueUpdated);
                if (read != 0 && Environment.TickCount64 - read < ExpireTime.TotalMilliseconds)
                {
                    return Value;
                }
                Interlocked.Exchange(ref TimeValueUpdated, Environment.TickCount64);
                Value = CalculateValueFunc();
                return Value;
            }
            finally
            {
                WriteSemaphore.Release();
                for (int i = 0; i < MaxReaders - 1; i++)
                {
                    ReadSemaphore.Release();
                }
            }
        }
        finally
        {
            ReadSemaphore.Release();
        }
    }
}
