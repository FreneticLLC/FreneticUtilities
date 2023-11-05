//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticExtensions;

/// <summary>Helper extensions for <see cref="IEnumerable{T}"/>, <see cref="IList{T}"/>, and related types.</summary>
public static class EnumerableExtensions
{
    /// <summary>Generates a new dictionary with keys and values swapped around.</summary>
    /// <typeparam name="TKey">The original key type.</typeparam>
    /// <typeparam name="TValue">The original value type.</typeparam>
    /// <param name="dictionary">The original dictionary.</param>
    /// <returns>The new dictionary.</returns>
    public static Dictionary<TValue, TKey> SwapKeyValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        Dictionary<TValue, TKey> toReturn = new(dictionary.Count);
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            toReturn.Add(pair.Value, pair.Key);
        }
        return toReturn;
    }

    /// <summary>
    /// Creates a dictionary mapping the first item in each pair to the second item in the pair.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="pairSet">List of (key, value) pairs.</param>
    /// <param name="throwOnDup">If true, will throw an <see cref="ArgumentException"/> if there are duplicate keys.</param>
    /// <returns>The new dictionary.</returns>
    public static Dictionary<TKey, TValue> PairsToDictionary<TKey, TValue>(this IEnumerable<(TKey, TValue)> pairSet, bool throwOnDup = true)
    {
        Dictionary<TKey, TValue> resultDictionary = new();
        if (throwOnDup)
        {
            foreach ((TKey, TValue) pair in pairSet)
            {
                resultDictionary.Add(pair.Item1, pair.Item2);
            }
        }
        else
        {
            foreach ((TKey, TValue) pair in pairSet)
            {
                resultDictionary[pair.Item1] = pair.Item2;
            }
        }
        return resultDictionary;
    }

    /// <summary>
    /// Creates a dictionary mapping the first item in each pair to the second item in the pair.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="pairSet">List of (key, value) pairs.</param>
    /// <param name="throwOnDup">If true, will throw an <see cref="ArgumentException"/> if there are duplicate keys.</param>
    /// <returns>The new dictionary.</returns>
    public static Dictionary<TKey, TValue> PairsToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairSet, bool throwOnDup = true)
    {
        Dictionary<TKey, TValue> resultDictionary = new();
        if (throwOnDup)
        {
            foreach (KeyValuePair<TKey, TValue> pair in pairSet)
            {
                resultDictionary.Add(pair.Key, pair.Value);
            }
        }
        else
        {
            foreach (KeyValuePair<TKey, TValue> pair in pairSet)
            {
                resultDictionary[pair.Key] = pair.Value;
            }
        }
        return resultDictionary;
    }

    /// <summary>
    /// Creates a dictionary mapping the keys array to the values array, such that keys[i] maps to values[i], for all integer "i" in range.
    /// <para>This will throw an <see cref="ArgumentException"/> if there are duplicate keys, or the two lists do not have the same size.</para>
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="keys">Key list.</param>
    /// <param name="values">Value list.</param>
    /// <returns>The new dictionary.</returns>
    public static Dictionary<TKey, TValue> ToDictionaryWithNoDup<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
    {
        int listLength = keys.Count;
        if (listLength != values.Count)
        {
            throw new ArgumentException("Value list does not have same length as key list! (" + keys.Count + ", " + values.Count + ")");
        }
        Dictionary<TKey, TValue> resultDictionary = new(listLength * 2);
        for (int i = 0; i < listLength; i++)
        {
            resultDictionary.Add(keys[i], values[i]);
        }
        return resultDictionary;
    }

    /// <summary>
    /// Creates a dictionary mapping the keys array to the values array, such that keys[i] maps to values[i], for all integer "i" in range.
    /// <para>This will throw an <see cref="ArgumentException"/> if the two lists do not have the same size.</para>
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="keys">Key list.</param>
    /// <param name="values">Value list.</param>
    /// <returns>The new dictionary.</returns>
    public static Dictionary<TKey, TValue> ToDictionaryWith<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
    {
        int listLength = keys.Count;
        if (listLength != values.Count)
        {
            throw new ArgumentException("Value list does not have same length as key list! (" + keys.Count + ", " + values.Count + ")");
        }
        Dictionary<TKey, TValue> resultDictionary = new(listLength * 2);
        for (int i = 0; i < listLength; i++)
        {
            resultDictionary[keys[i]] = values[i];
        }
        return resultDictionary;
    }

    /// <summary>
    /// Adds all entries from a separate <see cref="Dictionary{TKey, TValue}"/> to this <see cref="Dictionary{TKey, TValue}"/>.
    /// Does not allow duplicates.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="self">This <see cref="Dictionary{TKey, TValue}"/>.</param>
    /// <param name="toAdd">The entries to add.</param>
    public static void AddAll<TKey, TValue>(this Dictionary<TKey, TValue> self, Dictionary<TKey, TValue> toAdd)
    {
        foreach (KeyValuePair<TKey, TValue> entry in toAdd)
        {
            self.Add(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// Adds all entries from a separate <see cref="Dictionary{TKey, TValue}"/> to this <see cref="Dictionary{TKey, TValue}"/>.
    /// For any keys present in both <see cref="Dictionary{TKey, TValue}"/>s, the new values are used (and old values discarded).
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="self">This <see cref="Dictionary{TKey, TValue}"/>.</param>
    /// <param name="toAdd">The entries to add.</param>
    public static void UnionWith<TKey, TValue>(this Dictionary<TKey, TValue> self, Dictionary<TKey, TValue> toAdd)
    {
        foreach (KeyValuePair<TKey, TValue> entry in toAdd)
        {
            self[entry.Key] = entry.Value;
        }
    }

    /// <summary>Stops an enumerable processing when a function returns true for an item.</summary>
    /// <typeparam name="T">List item type.</typeparam>
    /// <param name="list">Item list.</param>
    /// <param name="stopFunction">Function that controls when to stop (true return = stop, false return = continue).</param>
    /// <returns>The list again.</returns>
    public static IEnumerable<T> StopWhen<T>(this IEnumerable<T> list, Func<T, bool> stopFunction)
    {
        foreach (T currentItem in list)
        {
            if (stopFunction(currentItem))
            {
                yield break;
            }
            yield return currentItem;
        }
    }

    /// <summary>Gets a value from a Dictionary, or creates a new value (and adds it to the dictionary).</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key to search for.</param>
    /// <param name="createFunction">A function to create a value.</param>
    /// <returns>The value from the dictionary or the newly created value.</returns>
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createFunction)
    {
        if (dictionary.TryGetValue(key, out TValue toReturn))
        {
            return toReturn;
        }
        TValue created = createFunction();
        dictionary.Add(key, created);
        return created;
    }

    /// <summary>Converts a <see cref="System.Collections.IEnumerator"/> to a generic Enumerable.</summary>
    /// <typeparam name="T">The expected Enumerable type.</typeparam>
    /// <param name="enumerator">The original Enumerator.</param>
    /// <returns>The enumerable.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this System.Collections.IEnumerator enumerator) where T : class
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current as T;
        }
    }

    /// <summary>Converts an <see cref="IEnumerator{T}"/> to an Enumerable.</summary>
    /// <typeparam name="T">The expected Enumerable type.</typeparam>
    /// <param name="enumerator">The original Enumerator.</param>
    /// <returns>The enumerable.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator) where T : class
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    /// <summary>Flattens a list of lists of <typeparamref name="T"/> to a list of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The expected Enumerable type.</typeparam>
    /// <param name="list">The list of lists.</param>
    /// <returns>The flattened list.</returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> list)
    {
        return list.SelectMany(x => x);
    }

    /// <summary>
    /// Returns an array where additional objects are joined into the array.
    /// No checks are done, the two arrays are simply combined into one larger array.
    /// </summary>
    /// <typeparam name="T">The array type.</typeparam>
    /// <param name="originalArray">The main array.</param>
    /// <param name="addtionalObjects">The additional objects to append to the end.</param>
    /// <returns>The joined result.</returns>
    public static T[] JoinWith<T>(this T[] originalArray, params T[] addtionalObjects)
    {
        T[] result = new T[originalArray.Length + addtionalObjects.Length];
        originalArray.CopyTo(result, 0);
        addtionalObjects.CopyTo(result, originalArray.Length);
        return result;
    }

    /// <summary>
    /// Returns a list where additional objects are joined into the list.
    /// No checks are done, the two lists are simply combined into one larger list.
    /// <para>Similar to the LINQ-Provided "Enumerable.Join" extension, but without the equality checking
    /// (the LINQ version does deduplication, this version does not).</para>
    /// </summary>
    /// <typeparam name="T">The list type.</typeparam>
    /// <param name="originalList">The main list.</param>
    /// <param name="addtionalObjects">The additional objects to append to the end.</param>
    /// <returns>The joined result.</returns>
    public static List<T> JoinWith<T>(this IEnumerable<T> originalList, IEnumerable<T> addtionalObjects)
    {
        List<T> result = new(originalList.Count() + addtionalObjects.Count());
        result.AddRange(originalList);
        result.AddRange(addtionalObjects);
        return result;
    }

    /// <summary>Returns whether a stream is empty. Invert of "Any()" call.</summary>
    /// <typeparam name="T">The stream type.</typeparam>
    /// <param name="inp">The input stream.</param>
    /// <returns>Whether the stream is empty.</returns>
    public static bool IsEmpty<T>(this IEnumerable<T> inp)
    {
        return !inp.Any();
    }

    /// <summary>A quick-n-easy tool to join an enumerable input to a string, with a given separator string. Equivalent to "string.Join(separator, input)"</summary>
    /// <typeparam name="T">The list type.</typeparam>
    /// <param name="inp">The list to be joined.</param>
    /// <param name="separator">The separator to go between parts.</param>
    /// <returns>The joined string.</returns>
    public static string JoinString<T>(this IEnumerable<T> inp, string separator)
    {
        return string.Join(separator, inp);
    }

    /// <summary>Returns the 0-based index of the first item that matches the given function, or -1 if no match is found.</summary>
    /// <typeparam name="T">The list type.</typeparam>
    /// <param name="inp">The list to search.</param>
    /// <param name="matcher">A function that returns true if the item matches and the search stop stop and return an index, or false if the item does not match.</param>
    /// <returns>The match index, or -1 if none.</returns>
    public static int FindFirstIndexOf<T>(this IEnumerable<T> inp, Func<T, bool> matcher)
    {
        int i = 0;
        foreach (T item in inp)
        {
            if (matcher(item))
            {
                return i;
            }
            i++;
        }
        return -1;
    }

    /// <summary>Returns the 0-based index of all items that matches the given function, or empty if none.</summary>
    /// <typeparam name="T">The list type.</typeparam>
    /// <param name="inp">The list to search.</param>
    /// <param name="matcher">A function that returns true if the item matches and an index should be given, or false if the item does not match.</param>
    /// <returns>The match indices, or empty if none.</returns>
    public static IEnumerable<int> FindAllIndicesOf<T>(this IEnumerable<T> inp, Func<T, bool> matcher)
    {
        int i = 0;
        foreach (T item in inp)
        {
            if (matcher(item))
            {
                yield return i;
            }
            i++;
        }
    }

    /// <summary>Casts all objects in the enumerable to given type <typeparamref name="T"/> and returns them, skipping any null or uncastable values.</summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <param name="inp">The input set.</param>
    /// <returns>A subset of the input, cast to the given type.</returns>
    public static IEnumerable<T> FilterCast<T>(this System.Collections.IEnumerable inp) where T: class
    {
        foreach (object x in inp)
        {
            if (x is T xT)
            {
                yield return xT;
            }
        }
    }
}
