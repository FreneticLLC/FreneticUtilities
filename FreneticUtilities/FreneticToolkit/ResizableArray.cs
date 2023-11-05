//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit;

/// <summary>
/// Represents an array that can be resized.
/// In other words, a <see cref="List{T}"/> that gives direct raw access to the underlying array.
/// This is useful primarily for optimization usages.
/// </summary>
public class ResizableArray<T> : IEnumerable<T>
{
    /// <summary>The internal value array. Length may not necessarily match <see cref="Length"/>.</summary>
    public T[] Internal;

    /// <summary>The actual number of elements in this list.</summary>
    public int Length = 0;

    /// <summary>Gets or sets the object at an index in the list.</summary>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }
            return Internal[index];
        }
        set
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }
            Internal[index] = value;
        }
    }

    /// <summary>Constructs an empty list.</summary>
    public ResizableArray()
    {
        Internal = Array.Empty<T>();
    }

    /// <summary>Constructs an empty list with a pre-chosen capacity.</summary>
    /// <param name="capacity">The estimated capacity.</param>
    public ResizableArray(int capacity)
    {
        Internal = new T[capacity];
    }

    /// <summary>Constructs a list from a pre-existing enumerable set.</summary>
    /// <param name="set">The pre-existing values.</param>
    public ResizableArray(IEnumerable<T> set)
    {
        Internal = new T[set.Count()];
        foreach (T x in set)
        {
            Internal[Length++] = x;
        }
    }

    /// <summary>Makes sure the list has enough capacity to hold a given number of elements without resizing.</summary>
    /// <param name="capacity">The capacity ensure.</param>
    public void EnsureCapacity(int capacity)
    {
        if (Internal.Length < capacity)
        {
            T[] old = Internal;
            Internal = new T[capacity];
            for (int i = 0; i < old.Length; i++)
            {
                Internal[i] = old[i];
            }
        }
    }

    /// <summary>Adds an element to the end of the list.</summary>
    /// <param name="x">The value to add.</param>
    public void Add(T x)
    {
        if (Internal.Length < Length + 1)
        {
            EnsureCapacity((Length + 1) * 2);
        }
        Internal[Length++] = x;
    }

    /// <summary>Adds a range of values to the list.</summary>
    /// <param name="set">The set of values to add.</param>
    public void AddRange(IEnumerable<T> set)
    {
        EnsureCapacity(Length + set.Count());
        foreach (T x in set)
        {
            Internal[Length++] = x;
        }
    }

    /// <summary>
    /// Clears the list (sets length to 0, but doesn't alter the array). Be wary that this may cause unexpected memory leaks if not used with care.
    /// Consider using <see cref="FullClear"/> to avoid memory problems if that concern outranks performance.
    /// </summary>
    public void Clear()
    {
        Length = 0;
    }

    /// <summary>Clears the list in a manner that takes longer to compute than <see cref="Clear"/>, but prevents memory leaks.</summary>
    public void FullClear()
    {
        for (int i = 0; i < Length; i++)
        {
            Internal[i] = default;
        }
        Length = 0;
    }

    /// <summary>Gets an enumerator over this list, for loops.</summary>
    public IEnumerator<T> GetEnumerator()
    {
        return new ResArrEnumerator(this);
    }

    /// <summary>Gets an enumerator over this list, for loops.</summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>Helper class for enumerating the <see cref="ResizableArray{T}"/>.</summary>
    public struct ResArrEnumerator : IEnumerator<T>
    {
        /// <summary>The current index in the list.</summary>
        public int Index;

        /// <summary>The relevant original list.</summary>
        public ResizableArray<T> Array;

        /// <summary>Constructs the enumerator.</summary>
        public ResArrEnumerator(ResizableArray<T> arr)
        {
            Array = arr;
            Index = -1;
        }

        /// <summary>Gets the current value.</summary>
        public T Current => Index < Array.Length ? Array[Index] : default;

        /// <summary>Gets the current value.</summary>
        object IEnumerator.Current => Index < Array.Length ? Array[Index] : default;

        /// <summary>Disposes the enumerator (does nothing).</summary>
        public void Dispose()
        {
        }

        /// <summary>Moves to the next element in the array.</summary>
        /// <returns>True if there's another element available, otherwise false.</returns>
        public bool MoveNext()
        {
            Index++;
            return Index < Array.Length;
        }

        /// <summary>Resets the enumerator to start of the list.</summary>
        public void Reset()
        {
            Index = -1;
        }
    }
}
