using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

// This class is based on publications from Stephen Toub on MSFT among others
// I cannot explain how or why one of .NET's core engineers published reference implementations of such an obvious foundation class without just implementing directly into .NET.
// There's even a version implemented in VisualStudio sources. Just not in mainline .NET. Why???

/// <summary>Like a <see cref="AutoResetEvent"/> but awaitable. Allows one thread to proceed per signal.</summary>
public class AsyncAutoResetEvent
{
    /// <summary>Internal data, do not touch.</summary>
    public struct InternalData
    {
        /// <summary>List of tasks representing the set of all threads waiting on this event.</summary>
        public LinkedList<TaskCompletionSource<bool>> Waiting;

        /// <summary>If true, the event has been signaled (and there were no current waiters).</summary>
        public bool IsSignaled;

        /// <summary>Access lock.</summary>
        public LockObject Lock;
    }

    /// <summary>Internal data, do not touch.</summary>
    public InternalData Internal;

    /// <summary>Initialize the <see cref="AsyncAutoResetEvent"/>.</summary>
    /// <param name="signaled">If true, is pre-signalled. If false, is not.</param>
    public AsyncAutoResetEvent(bool signaled)
    {
        Internal.Waiting = new();
        Internal.Lock = new();
        Internal.IsSignaled = signaled;
    }

    /// <summary>Returns a task that waits for the next signal.</summary>
    /// <param name="timeout">The max timeout before giving up.</param>
    /// <param name="cancel">Cancellation token to allow stopping early from an arbitrary signal.</param>
    /// <returns>True if signalled, false if timed out or cancelled.</returns>
    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancel = default)
    {
        LinkedListNode<TaskCompletionSource<bool>> node;
        lock (Internal.Lock)
        {
            if (Internal.IsSignaled)
            {
                Internal.IsSignaled = false;
                return true;
            }
            node = Internal.Waiting.AddLast(new TaskCompletionSource<bool>());
        }
        Task result = await Task.WhenAny(node.Value.Task, Task.Delay(timeout, cancel));
        if (result == node.Value.Task)
        {
            return true;
        }
        else
        {
            lock (Internal.Lock)
            {
                Internal.Waiting.Remove(node);
            }
            return false;
        }
    }

    /// <summary>Sets the signal, allowing exactly one thread through.</summary>
    public void Set()
    {
        lock (Internal.Lock)
        {
            if (Internal.Waiting.Count > 0)
            {
                Internal.Waiting.First.Value.SetResult(true);
                Internal.Waiting.RemoveFirst();
            }
            else
            {
                Internal.IsSignaled = true;
            }
        }
    }
}
