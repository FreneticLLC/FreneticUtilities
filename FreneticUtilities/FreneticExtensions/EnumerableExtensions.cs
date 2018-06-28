using System;
using System.Collections.Generic;
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
    }
}
