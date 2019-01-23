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

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>
    /// Helper extensions for <see cref="IEnumerable{T}"/>, <see cref="IList{T}"/>, and related types.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Generates a new dictionary with keys and values swapped around.
        /// </summary>
        /// <typeparam name="TKey">The original key type.</typeparam>
        /// <typeparam name="TValue">The original value type.</typeparam>
        /// <param name="dictionary">The original dictionary.</param>
        /// <returns>The new dictionary.</returns>
        public static Dictionary<TValue, TKey> SwapKeyValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            Dictionary<TValue, TKey> toReturn = new Dictionary<TValue, TKey>(dictionary.Count);
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                toReturn.Add(pair.Value, pair.Key);
            }
            return toReturn;
        }

        /// <summary>
        /// Creates a dictionary mapping the keys array to the values array, such that keys[i] maps to values[i], for all integer "i" in range.
        /// <para>This will throw an <see cref="ArgumentException"/> if there are duplicate keys, or the two lists do not have the same size.</para>
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="keys">Key list.</param>
        /// <param name="values">Value list.</param>
        /// <returns>Dictionary.</returns>
        public static Dictionary<TKey, TValue> ToDictionaryWithNoDup<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
        {
            int listLength = keys.Count;
            if (listLength != values.Count)
            {
                throw new ArgumentException("Value list does not have same length as key list! (" + keys.Count + ", " + values.Count + ")");
            }
            Dictionary<TKey, TValue> resultDictionary = new Dictionary<TKey, TValue>(listLength * 2);
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
        /// <returns>Dictionary.</returns>
        public static Dictionary<TKey, TValue> ToDictionaryWith<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
        {
            int listLength = keys.Count;
            if (listLength != values.Count)
            {
                throw new ArgumentException("Value list does not have same length as key list! (" + keys.Count + ", " + values.Count + ")");
            }
            Dictionary<TKey, TValue> resultDictionary = new Dictionary<TKey, TValue>(listLength * 2);
            for (int i = 0; i < listLength; i++)
            {
                resultDictionary[keys[i]] = values[i];
            }
            return resultDictionary;
        }

        /// <summary>
        /// Stops an enumerable processing when a function returns true for an item.
        /// </summary>
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

        /// <summary>
        /// Gets a value from a Dictionary, or creates a new value (and adds it to the dictionary).
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key to search for.</param>
        /// <param name="createFunction">A function to create a value.</param>
        /// <returns>The value from the dictionary or the newly created value.</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createFunction)
        {
            if (dictionary.TryGetValue(key, out TValue toReturn))
            {
                return toReturn;
            }
            TValue created = createFunction();
            dictionary.Add(key, created);
            return created;
        }

        /// <summary>
        /// Converts a <see cref="System.Collections.IEnumerator"/> to a generic Enumerable.
        /// </summary>
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

        /// <summary>
        /// Converts an <see cref="IEnumerator{T}"/> to an Enumerable.
        /// </summary>
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

        /// <summary>
        /// Returns an array where additional objects are joined into the array.
        /// </summary>
        /// <typeparam name="T">The array type.</typeparam>
        /// <param name="originalArray">The main array.</param>
        /// <param name="addtionalObjects">The additional objects to append to the end.</param>
        /// <returns>The joined result.</returns>
        public static T[] JoinWith<T>(this T[] originalArray, params T[] addtionalObjects)
        {
            T[] res = new T[originalArray.Length + addtionalObjects.Length];
            originalArray.CopyTo(res, 0);
            addtionalObjects.CopyTo(res, originalArray.Length);
            return res;
        }

        /// <summary>
        /// Returns whether a stream is empty. Invert of "Any()" call.
        /// </summary>
        /// <typeparam name="T">The stream type.</typeparam>
        /// <param name="inp">The input stream.</param>
        /// <returns>Whether the stream is empty.</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> inp)
        {
            return !inp.Any();
        }
    }
}
