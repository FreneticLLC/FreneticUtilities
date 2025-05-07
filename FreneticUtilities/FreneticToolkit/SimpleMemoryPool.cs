using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>
/// A simple memory pool for objects. Can take and return data. Can dynamically create new items, reset old items to default, and destroy excess old items.
/// Async-safe.
/// </summary>
/// <typeparam name="T">The type of data to track in the pool</typeparam>
/// <param name="createNew">Function that creates and returns a new object of the given type.</param>
/// <param name="resetOld">A function that resets a given object fully to default before return.</param>
/// <param name="maxInPool">The max number of items in the pool before destroying them.</param>
/// <param name="destroyOld">A function to destroy old items that can't fit the pool.</param>
public class SimpleMemoryPool<T>(Func<T> createNew, Action<T> resetOld, int maxInPool, Action<T> destroyOld)
{
    /// <summary>The internal queue of available items.</summary>
    public ConcurrentQueue<T> AvailableQueue = new();

    /// <summary>Pre-fill the pool with the given number of items. Pre-fill can keep memory management cleaner.</summary>
    /// <param name="count">How many items to pre-fill the pool with.</param>
    public void Prefill(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AvailableQueue.Enqueue(createNew());
        }
    }

    /// <summary>Take a new item.</summary>
    public T Take()
    {
        if (AvailableQueue.TryDequeue(out T item))
        {
            return item;
        }
        else
        {
            return createNew();
        }
    }

    /// <summary>Return an item to the pool. The item will be reset to default before being returned.</summary>
    /// <param name="item">The item to return.</param>
    public void Return(T item)
    {
        resetOld(item);
        if (AvailableQueue.Count < maxInPool)
        {
            AvailableQueue.Enqueue(item);
        }
        else
        {
            destroyOld(item);
        }
    }
}
