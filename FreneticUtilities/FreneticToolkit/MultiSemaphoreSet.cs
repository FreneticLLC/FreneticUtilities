using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>Simple class that tracks a limited size static set of multiple Semaphores for a set of values.
/// This operates based on the hash of the lookup object, and does not maintain any table or memory of what objects are used.
/// The size of this set is statically determined when the instance is created.
/// This is useful to prevent async overlap from applying multiple times to the same value, in a way that does not require complex tracking of the values or dynamic memory allocation.
/// <para>Handles nulls as equivalent to hash 0.</para></summary>
/// <example>
/// <code>
///     MultiSemaphoreSet&lt;string&gt; set = new(32, 1);
///     string myStr = "some text";
///     lock (set.GetLock(myStr))
///     {
///         // something unique to myStr
///     }
/// </code>
/// </example>
public class MultiSemaphoreSet<T>
{
    /// <summary>The internal set of actual <see cref="SemaphoreSlim"/> used to lock against.</summary>
    public SemaphoreSlim[] InternalLocks;

    /// <summary>Constructs a new <see cref="MultiLockSet{T}"/> with the specified size.</summary>
    public MultiSemaphoreSet(int size, int semaphoreUsageCount = 1)
    {
        InternalLocks = new SemaphoreSlim[size];
        for (int i = 0; i < size; i++)
        {
            InternalLocks[i] = new(semaphoreUsageCount);
        }
    }

    /// <summary>Gets the lock object for the given value.</summary>
    public SemaphoreSlim GetLock(T value)
    {
        return InternalLocks[Math.Abs(value?.GetHashCode() ?? 0) % InternalLocks.Length];
    }
}
