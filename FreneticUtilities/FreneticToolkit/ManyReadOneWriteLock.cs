using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>
/// Micro-utility to provide a many-readers, one-writer lock system.
/// Guarantees than a write will never overlap with a read, but reads can overlap with each other.
/// </summary>
/// <param name="maxReaders">Maximum parallel reads (that may never overlap with a write).</param>
public class ManyReadOneWriteLock(int maxReaders = 10)
{
    /// <summary>Read semaphore to limit reading from happening at the same time as writing, but allow parallel reads.</summary>
    public SemaphoreSlim ReadSemaphore = new(maxReaders, maxReaders);

    /// <summary>Write semaphore to limit writes from overlapping.</summary>
    public SemaphoreSlim WriteSemaphore = new(1, 1);

    /// <summary>Maximum number of readers allowed at once.</summary>
    public readonly int MaxReaders = maxReaders;

    /// <summary>Claim a single read lock, allowing you to read the locked data, guaranteeing no current writer but possible other readers.
    /// Use with 'using', eg <code>using ManyReadOneWriteLock.ReadClaim claim = lock.LockRead();</code>
    /// </summary>
    public ReadClaim LockRead()
    {
        ReadSemaphore.Wait();
        return new ReadClaim() { Lock = this };
    }

    /// <summary>Claim a full write lock, allowing you to write the locked data, guaranteeing no other readers.
    /// Use with 'using', eg <code>using ManyReadOneWriteLock.WriteClaim claim = lock.LockWrite();</code>
    /// </summary>
    public WriteClaim LockWrite()
    {
        WriteSemaphore.Wait();
        for (int i = 0; i < MaxReaders; i++)
        {
            ReadSemaphore.Wait();
        }
        return new WriteClaim() { Lock = this };
    }

    /// <summary>Represents one held read claim on a lock. Acquire from <see cref="LockRead"/>.</summary>
    public struct ReadClaim : IDisposable
    {
        /// <summary>The lock that this claim is associated with.</summary>
        public ManyReadOneWriteLock Lock;

        /// <summary>Whether this claim has been disposed.</summary>
        public bool IsDisposed;

        /// <summary>Dispose of the claim, releasing the lock. Implements <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            Lock.ReadSemaphore.Release();
            IsDisposed = true;
        }
    }

    /// <summary>Represents one held write claim on a lock. Acquire from <see cref="LockWrite"/>.</summary>
    public struct WriteClaim : IDisposable
    {
        /// <summary>The lock that this claim is associated with.</summary>
        public ManyReadOneWriteLock Lock;

        /// <summary>Whether this claim has been disposed.</summary>
        public bool IsDisposed;

        /// <summary>Dispose of the claim, releasing the lock. Implements <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            Lock.WriteSemaphore.Release();
            Lock.ReadSemaphore.Release(Lock.MaxReaders);
            IsDisposed = true;
        }
    }
}
