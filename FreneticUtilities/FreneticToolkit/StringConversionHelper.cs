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
using FreneticUtilities.FreneticExtensions;

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>A special helper for converting <see cref="string"/> input to and from various types.</summary>
    public static class StringConversionHelper
    {
        /// <summary>
        /// A standard UTF-8 encoding helper object instance.
        /// <para>This is not equivalent to <see cref="Encoding.UTF8"/> as that will output BOM when converting a <see cref="string"/> to binary data (which is usually bad).</para>
        /// <para>This instance, by contrast, is guaranteed to not do that.</para>
        /// </summary>
        public static readonly UTF8Encoding UTF8Encoding = new(false);

        /// <summary>
        /// Converts a string value to the unsigned short-integer value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a unsigned short-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a unsigned short-integer value.</returns>
        public static ushort StringToUShort(string input, ushort defaultValue = 0)
        {
            if (ushort.TryParse(input, out ushort output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the short-integer value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a short-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a short-integer value.</returns>
        public static short StringToShort(string input, short defaultValue = 0)
        {
            if (short.TryParse(input, out short output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the long-integer value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a long-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a long-integer value.</returns>
        public static long StringToLong(string input, long defaultValue = 0)
        {
            if (long.TryParse(input, out long output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the integer value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent an integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>an integer value.</returns>
        public static int StringToInt(string input, int defaultValue = 0)
        {
            if (int.TryParse(input, out int output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the double value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a double.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a double value.</returns>
        public static double StringToDouble(string input, double defaultValue = 0.0)
        {
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the float value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a float.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a float value.</returns>
        public static float StringToFloat(string input, float defaultValue = 0f)
        {
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string to a date-time.
        /// <para>Parsing errors will give a null result.</para>
        /// <para>Expected format is "YYYY/MM/DD hh:mm:ss UTC+OO:oo".</para>
        /// <para>YYYY = 4 digit year, MM = 2 digit month, DD = 2 digit day.</para>
        /// <para>hh = 2 digit hour, mm = 2 digit minute, ss = 2 digit second.</para>
        /// <para>OO = 2 digit offset hours, oo = 2 digit offset minutes. Can be prefixed with a '+' or '-'.</para>
        /// <para>Optionally add 4-digit millisecond, like "YYYY/MM/DD hh:mm:ss:tttt UTC+OO:oo".</para>
        /// <para>Inverted by <see cref="DateTimeToString(DateTimeOffset, bool)"/>.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The date-time.</returns>
        public static DateTimeOffset? StringToDateTime(string input)
        {
            string[] inputSplit = input.SplitFast(' ');
            if (inputSplit.Length != 3)
            {
                return null;
            }
            string[] yearmonthday = inputSplit[0].SplitFast('/');
            if (yearmonthday.Length != 3)
            {
                return null;
            }
            if (!int.TryParse(yearmonthday[0], out int year)
                || !int.TryParse(yearmonthday[1], out int month)
                || !int.TryParse(yearmonthday[2], out int day))
            {
                return null;
            }
            string[] hourminutesecond = inputSplit[1].SplitFast(':');
            if (hourminutesecond.Length != 3 && hourminutesecond.Length != 4)
            {
                return null;
            }
            if (!int.TryParse(hourminutesecond[0], out int hour)
                || !int.TryParse(hourminutesecond[1], out int minute)
                || !int.TryParse(hourminutesecond[2], out int second))
            {
                return null;
            }
            int millisecond = 0;
            if (hourminutesecond.Length == 4 && !int.TryParse(hourminutesecond[3], out millisecond))
            {
                return null;
            }
            int offsetHours;
            int offsetMinutes = 0;
            if (inputSplit[2].Contains('-'))
            {
                string[] offsetinfo = inputSplit[2].SplitFast('-');
                string[] subinf = offsetinfo[1].SplitFast(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                if (!int.TryParse(subinf[0], out offsetHours)
                    && !int.TryParse(subinf[1], out offsetMinutes))
                {
                    return null;
                }
                offsetHours = -offsetHours;
                offsetMinutes = -offsetMinutes;
            }
            else
            {
                string[] offsetinfo = inputSplit[2].SplitFast('+');
                if (offsetinfo.Length != 2)
                {
                    return null;
                }
                string[] subinf = offsetinfo[1].SplitFast(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                if (!int.TryParse(subinf[0], out offsetHours)
                    && !int.TryParse(subinf[1], out offsetMinutes))
                {
                    return null;
                }
            }
            TimeSpan offset = new(offsetHours, offsetMinutes, 0);
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
        }

        /// <summary>
        /// Returns a string representation of the specified time.
        /// <para>Format is "YYYY/MM/DD hh:mm:ss UTC+OO:oo".</para>
        /// <para>YYYY = 4 digit year, MM = 2 digit month, DD = 2 digit day.</para>
        /// <para>hh = 2 digit hour, mm = 2 digit minute, ss = 2 digit second.</para>
        /// <para>OO = 2 digit offset hours, oo = 2 digit offset minutes. Can be prefixed with a '+' or '-'.</para>
        /// <para>if 'showMilliseconds' is true, will add 4-digit millisecond, like "YYYY/MM/DD hh:mm:ss:tttt UTC+OO:oo".</para>
        /// <para>Inverted by <see cref="StringToDateTime(string)"/>.</para>
        /// </summary>
        /// <param name="input">The datetime object.</param>
        /// <param name="showMilliseconds">Whether to include milliseconds.</param>
        /// <returns>The time as a string.</returns>
        public static string DateTimeToString(DateTimeOffset input, bool showMilliseconds)
        {
            static string QPad(int input, int length = 2)
            {
                return input.ToString().PadLeft(length, '0');
            }
            TimeSpan offset = input.Offset;
            string utcoffset;
            if (offset.TotalMilliseconds < 0)
            {
                offset *= -1;
                utcoffset = "-";
            }
            else
            {
                utcoffset = "+";
            }
            utcoffset += QPad(((int)Math.Floor(offset.TotalHours))) + ":" + QPad(offset.Minutes);
            return QPad(input.Year, 4) + "/" + QPad(input.Month) + "/" + QPad(input.Day) + " " + QPad(input.Hour) + ":" + QPad(input.Minute) + ":" + QPad(input.Second)
                + (showMilliseconds ? ":" + QPad(input.Millisecond, 4) : "") + " UTC" + utcoffset;
        }

        /// <summary>
        /// Finds the closest string in a list to a searched string, using Levenshtein comparison logic.
        /// See also <see cref="GetLevenshteinDistance(string, string)"/>.
        /// </summary>
        /// <param name="allStrs">The list of all valid strings.</param>
        /// <param name="searchString">The string to search for.</param>
        /// <param name="maxDistance">The maximum Levenshtein distance, if any.</param>
        /// <returns>The found string, or null if none.</returns>
        public static string FindClosestString(IEnumerable<string> allStrs, string searchString, int maxDistance = int.MaxValue)
        {
            return FindClosestString(allStrs, searchString, out _, maxDistance);
        }

        /// <summary>
        /// Finds the closest string in a list to a searched string, using Levenshtein comparison logic.
        /// See also <see cref="GetLevenshteinDistance(string, string)"/>.
        /// </summary>
        /// <param name="allStrs">The list of all valid strings.</param>
        /// <param name="searchString">The string to search for.</param>
        /// <param name="maxDistance">The maximum Levenshtein distance, if any.</param>
        /// <param name="dist">An output parameter for the lowest distance value.</param>
        /// <returns>The found string, or null if none.</returns>
        public static string FindClosestString(IEnumerable<string> allStrs, string searchString, out int dist, int maxDistance = int.MaxValue)
        {
            int lowestDistance = maxDistance;
            string lowestStr = null;
            foreach (string option in allStrs)
            {
                int currentDistance = GetLevenshteinDistance(searchString, option);
                if (currentDistance < lowestDistance)
                {
                    lowestDistance = currentDistance;
                    lowestStr = option;
                }
            }
            dist = lowestDistance;
            return lowestStr;
        }

        /// <summary>
        /// Gets the approximate distance between two strings, based on Levenshtein comparison logic.
        /// Useful for finding "Did you mean ...?" suggestions (see also <see cref="FindClosestString(IEnumerable{string}, string, int)"/>).
        /// </summary>
        /// <param name="firstString">The first string.</param>
        /// <param name="secondString">The second string to compare against the first.</param>
        /// <returns>A numerical value indicating how different the two strings are.</returns>
        public static int GetLevenshteinDistance(string firstString, string secondString)
        {
            int firstLength = firstString.Length;
            int secondLength = secondString.Length;
            if (firstLength == 0)
            {
                return secondLength;
            }
            else if (secondLength == 0)
            {
                return firstLength;
            }
            int[] previousCostArray = new int[firstLength + 1];
            int[] costArray = new int[firstLength + 1];
            int[] swapPlaceholder;
            for (int i = 0; i <= firstLength; i++)
            {
                previousCostArray[i] = i;
            }
            for (int j = 0; j < secondLength; j++)
            {
                char secondAtJ = secondString[j];
                costArray[0] = j + 1;
                for (int i = 0; i < firstLength; i++)
                {
                    int cost = firstString[i] == secondAtJ ? 0 : 1;
                    // minimum of cell to the left+1, to the top+1, diagonally left
                    // and up +cost
                    costArray[i + 1] = Math.Min(Math.Min(costArray[i] + 1, previousCostArray[i + 1] + 1), previousCostArray[i] + cost);
                }
                // copy current distance counts to 'previous row' distance counts
                swapPlaceholder = previousCostArray;
                previousCostArray = costArray;
                costArray = swapPlaceholder;
            }
            // our last action in the above loop was to switch previous and current, so previous now
            // actually has the most recent cost counts
            return previousCostArray[firstLength];
        }

        /// <summary>
        /// Finds the index of the first different character between two strings (case sensitive).
        /// <para>For example, input of "My text" and "My word" returns '3' (character 't' in <paramref name="a"/> vs character 'w' in <paramref name="b"/>).</para>
        /// <para>For input like "My text" and "My text2", returns '7' (the index of character '2').</para>
        /// <para>Returns -1 for exactly equal strings.</para>
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns>The index of first difference, or -1 if no difference.</returns>
        public static int FindFirstDifference(string a, string b)
        {
            int maxLen = Math.Min(a.Length, b.Length);
            for (int i = 0; i < maxLen; i++)
            {
                if (a[i] != b[i])
                {
                    return i;
                }
            }
            if (a.Length != b.Length)
            {
                return maxLen;
            }
            return -1;
        }

        /// <summary>
        /// Quick and simple method to fill the tags in a tagged string. For example:
        /// <code>
        /// QuickSimpleTagFiller("Hello [name], today is [day]!", "[", "]", (tag) => tag switch { "name" => "Bob", "day" => "Wednesday", _ => null })
        /// </code>
        /// Would return "Hello Bob, today is Wednesday!"
        /// <para>
        /// Specifically designed for very quick and simple usages, to be a much better alternative to "str.Replace("[tag]", value)" spam or similar quickhacks.
        /// Also enables dynamic processing based on tag data.
        /// </para>
        /// </summary>
        /// <param name="taggedText">The original raw text.</param>
        /// <param name="prefix">A symbol that indicates the start of a tag.</param>
        /// <param name="suffix">A symbol that indicates the end of a tag.</param>
        /// <param name="tagReader">A function that takes the text of a tag and returns the appropriate value for it, or null if unfillable.</param>
        public static string QuickSimpleTagFiller(string taggedText, string prefix, string suffix, Func<string, string> tagReader)
        {
            int start = taggedText.IndexOf(prefix);
            int end = taggedText.IndexOf(suffix, start + prefix.Length);
            if (start == -1 || end == -1)
            {
                return taggedText;
            }
            StringBuilder output = new(taggedText.Length);
            int lastEnd = 0;
            while (start != -1 && end != -1)
            {
                output.Append(taggedText[lastEnd..start]);
                string part = taggedText[(start + prefix.Length)..end];
                string read = tagReader(part);
                if (read is not null)
                {
                    output.Append(read);
                    lastEnd = end + suffix.Length;
                }
                else
                {
                    lastEnd = start;
                }
                start = taggedText.IndexOf(prefix, end + suffix.Length);
                end = taggedText.IndexOf(suffix, start + prefix.Length);
            }
            output.Append(taggedText[lastEnd..]);
            return output.ToString();
        }
    }
}
