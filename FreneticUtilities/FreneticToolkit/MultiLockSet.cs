using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>Simple class that tracks a limited size static set of multiple lock objects for a set of values.
/// This operates based on the hash of the lookup object, and does not maintain any table or memory of what objects are used.
/// The size of this set is statically determined when the instance is created.
/// This is useful to prevent async overlap from applying multiple times to the same value, in a way that does not require complex tracking of the values or dynamic memory allocation.
/// <para>Handles nulls as equivalent to hash 0.</para></summary>
/// <example>
/// <code>
///     MultiLockSet&lt;string&gt; set = new(32);
///     string myStr = "some text";
///     lock (set.GetLock(myStr))
///     {
///         // something unique to myStr
///     }
/// </code>
/// </example>
public class MultiLockSet<T>
{
    /// <summary>The internal set of actual <see cref="LockObject"/> used to lock against.</summary>
    public LockObject[] InternalLocks;

    /// <summary>Constructs a new <see cref="MultiLockSet{T}"/> with the specified size.</summary>
    public MultiLockSet(int size)
    {
        InternalLocks = new LockObject[size];
        for (int i = 0; i < size; i++)
        {
            InternalLocks[i] = new();
        }
    }

    /// <summary>Gets the lock object for the given value.</summary>
    public LockObject GetLock(T value)
    {
        return InternalLocks[Math.Abs(value?.GetHashCode() ?? 0) % InternalLocks.Length];
    }
}
