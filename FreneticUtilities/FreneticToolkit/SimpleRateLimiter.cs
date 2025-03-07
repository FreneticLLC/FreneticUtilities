using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>
/// Simple multi-user rate limiter.
/// Async safe.
/// </summary>
/// <typeparam name="T">The type of the user ID key, eg a string.</typeparam>
/// <param name="maxUsages">The rate limit per-user.</param>
/// <param name="refreshPeriod">How long before the usage counter resets.</param>
/// <param name="lockConcurrency">The number of concurrent locks - raising this improves async concurrency, but costs a bit of memory usage.</param>
public class SimpleRateLimiter<T>(int maxUsages, TimeSpan refreshPeriod, int lockConcurrency = 32) where T : class
{
    /// <summary>How long before the usage counter resets, in milliseconds.</summary>
    public long RefreshPeriod = (long)refreshPeriod.TotalMilliseconds;

    /// <summary>The rate limit per-user.</summary>
    public int MaxUsages = maxUsages;

    /// <summary>Set of locks mapped by user ID key.</summary>
    public MultiLockSet<T> Locks = new(lockConcurrency);

    /// <summary>Data about a user within the <see cref="SimpleRateLimiter{T}"/>.</summary>
    public class User
    {
        /// <summary>Last <see cref="Environment.TickCount64"/> that the user's counter was refreshed at.</summary>
        public long LastRefresh = Environment.TickCount64;

        /// <summary>How many usages have already happened since the last refresh.</summary>
        public int Usages = 0;
    }

    /// <summary>All current tracked rate limit users.</summary>
    public ConcurrentDictionary<T, User> RateLimits = new();

    /// <summary>This is a quick little cheat to keep memory in order: after each successful usage, try to clean a random user from the pool.</summary>
    public void AutoClean()
    {
        int random = Random.Shared.Next(0, RateLimits.Count);
        T picked = RateLimits.Keys.Skip(random).FirstOrDefault();
        if (picked is null)
        {
            return;
        }
        lock (Locks.GetLock(picked))
        {
            if (!RateLimits.TryGetValue(picked, out User user) || Environment.TickCount64 - user.LastRefresh < RefreshPeriod)
            {
                return;
            }
            RateLimits.TryRemove(picked, out _);
        }
        // We cleaned one, so try again.
        AutoClean();
    }

    /// <summary>Try to use one of the rate limit usages for the given ID. Returns true if usage is allowed, or false if rate limit is exceeded.</summary>
    public bool TryUseOne(T id)
    {
        lock (Locks.GetLock(id))
        {
            User user = RateLimits.GetOrAdd(id, _ => new());
            if (Environment.TickCount64 - user.LastRefresh > RefreshPeriod)
            {
                user.Usages = 0;
                user.LastRefresh = Environment.TickCount64;
            }
            if (user.Usages >= MaxUsages)
            {
                return false;
            }
            user.Usages++;
        }
        AutoClean();
        return true;
    }
}
