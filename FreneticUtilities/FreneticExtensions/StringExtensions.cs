//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>
    /// Helper extensions for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Rapidly converts an ASCII string to a lowercase representation.
        /// <para>Does not work with non-ASCII text (no support for unicode/multi-language/etc).</para>
        /// <para>Operates explicitly on the ASCII 'a-z' and 'A-Z' range.</para>
        /// <para>Can be slow if the string is already lowercase (Consider using <see cref="IsAllLowerFast(string)"/> if that is likely).</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>A lowercase version.</returns>
        public static string ToLowerFast(this string input)
        {
            char[] finalString = input.ToCharArray();
            for (int i = 0; i < finalString.Length; i++)
            {
                if (finalString[i] >= 'A' && finalString[i] <= 'Z')
                {
                    finalString[i] = (char)(finalString[i] - ('A' - 'a'));
                }
            }
            return new string(finalString);
        }

        /// <summary>
        /// Gets the part of a string before a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The prior portion.</returns>
        public static string Before(this string input, string match)
        {
            int index = input.IndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the part of a string before a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The prior portion.</returns>
        public static string Before(this string input, char match)
        {
            int index = input.IndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the part of a string before the last occurence of a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeLast(this string input, string match)
        {
            int index = input.LastIndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the part of a string before the last occurence of a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeLast(this string input, char match)
        {
            int index = input.LastIndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the parts of a string before and after a specified portion.
        /// <para>If no match is found, the full input string will be returned as the 'before', and the after will be an empty string.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <param name="after">The output of the latter portion.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeAndAfter(this string input, string match, out string after)
        {
            int index = input.IndexOf(match);
            if (index < 0)
            {
                after = "";
                return input;
            }
            after = input.Substring(index + match.Length);
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the parts of a string before and after a specified portion.
        /// <para>If no match is found, the full input string will be returned as the 'before', and the after will be an empty string.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <param name="after">The output of the latter portion.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeAndAfter(this string input, char match, out string after)
        {
            int index = input.IndexOf(match);
            if (index < 0)
            {
                after = "";
                return input;
            }
            after = input.Substring(index + 1);
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the parts of a string before and after the last occurence of a specified portion.
        /// <para>If no match is found, the full input string will be returned as the 'before', and the after will be an empty string.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <param name="after">The output of the latter portion.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeAndAfterLast(this string input, char match, out string after)
        {
            int index = input.LastIndexOf(match);
            if (index < 0)
            {
                after = "";
                return input;
            }
            after = input.Substring(index + 1);
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the parts of a string before and after the last occurence of a specified portion.
        /// <para>If no match is found, the full input string will be returned as the 'before', and the after will be an empty string.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <param name="after">The output of the latter portion.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeAndAfterLast(this string input, string match, out string after)
        {
            int index = input.LastIndexOf(match);
            if (index < 0)
            {
                after = "";
                return input;
            }
            after = input.Substring(index + match.Length);
            return input.Substring(0, index);
        }

        /// <summary>
        /// Gets the part of a string after the last occurence of a specified character.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The latter portion.</returns>
        public static string AfterLast(this string input, char match)
        {
            int index = input.LastIndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(index + 1);
        }

        /// <summary>
        /// Gets the part of a string after the last occurence of a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The latter portion.</returns>
        public static string AfterLast(this string input, string match)
        {
            int index = input.LastIndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(index + match.Length);
        }

        /// <summary>
        /// Gets the part of a string after a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The latter portion.</returns>
        public static string After(this string input, char match)
        {
            int index = input.IndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(index + 1);
        }

        /// <summary>
        /// Gets the part of a string after a specified portion.
        /// <para>If no match is found, the full input string will be returned.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The latter portion.</returns>
        public static string After(this string input, string match)
        {
            int index = input.IndexOf(match);
            if (index < 0)
            {
                return input;
            }
            return input.Substring(index + match.Length);
        }

        /// <summary>
        /// Returns whether the string has a specific character at a specific index.
        /// <para>Accepts values out of range (less than zero or greater than length) and returns false for those values.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="index">The 0-based index to check.</param>
        /// <param name="character">The character to check against.</param>
        /// <returns>True if that index equals that character, otherwise false.</returns>
        public static bool IndexEquals(this string input, int index, char character)
        {
            return input.Length > index && index > 0 && input[index] == character;
        }

        /// <summary>
        /// Returns whether the string starts with a null character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>True if the first character of the string is a null, otherwise false.</returns>
        public static bool StartsWithNull(this string input)
        {
            return input.Length > 0 && input[0] == '\0';
        }

        /// <summary>
        /// Returns whether the string starts with the specified character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="firstChar">The character being checked.</param>
        /// <returns>True if the first character of the string is equal to the specified character, otherwise false.</returns>
        public static bool StartsWithFast(this string input, char firstChar)
        {
            return input.Length > 0 && input[0] == firstChar;
        }

        /// <summary>
        /// Returns whether the string ends with a null character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>True if the last character of the string is a null, otherwise false.</returns>
        public static bool EndsWithNull(this string input)
        {
            return input.Length > 0 && input[input.Length - 1] == '\0';
        }

        /// <summary>
        /// Returns whether the string ends with the specified character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="firstChar">The character being checked for.</param>
        /// <returns>True if the last character of the string is equal to the specified character, otherwise false.</returns>
        public static bool EndsWithFast(this string input, char firstChar)
        {
            return input.Length > 0 && input[input.Length - 1] == firstChar;
        }

        /// <summary>
        /// Returns whether the string contains only lowercase ASCII letters (or more specifically: that it does not contain uppercase ASCII letters).
        /// <para>Does not work with non-ASCII text (no support for unicode/multi-language/etc).</para>
        /// <para>Operates explicitly on the ASCII 'a-z' and 'A-Z' range.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>True if there are no uppercase letters, false if there are uppercase letters found.</returns>
        public static bool IsAllLowerFast(this string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] >= 'A' && input[i] <= 'Z')
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Rapidly converts an ASCII string to a uppercase representation.
        /// <para>Does not work with non-ASCII text (no support for unicode/multi-language/etc).</para>
        /// <para>Operates explicitly on the ASCII 'a-z' and 'A-Z' range.</para>
        /// <para>Can be slow if the string is already uppercase (Consider using <see cref="IsAllUpperFast(string)"/> if that is likely).</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>An uppercase version.</returns>
        public static string ToUpperFast(this string input)
        {
            char[] finalString = input.ToCharArray();
            for (int i = 0; i < finalString.Length; i++)
            {
                if (finalString[i] >= 'a' && finalString[i] <= 'z')
                {
                    finalString[i] = (char)(finalString[i] + ('A' - 'a'));
                }
            }
            return new string(finalString);
        }

        /// <summary>
        /// Returns whether the string contains only uppercase ASCII letters (or more specifically: that it does not contain uppercase ASCII letters).
        /// <para>Does not work with non-ASCII text (no support for unicode/multi-language/etc).</para>
        /// <para>Operates explicitly on the ASCII 'a-z' and 'A-Z' range.</para>
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>True if there are no lowercase letters, false if there are lowercase letters found.</returns>
        public static bool IsAllUpperFast(this string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] >= 'a' && input[i] <= 'z')
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Counts instances of a character in a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="charToCount">The character to count.</param>
        /// <returns>The number of times the character is in the string (0 if none).</returns>
        public static int CountCharacter(this string input, char charToCount)
        {
            int finalCount = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == charToCount)
                {
                    finalCount++;
                }
            }
            return finalCount;
        }

        /// <summary>
        /// Quickly split a string around a splitter character, with an optional max split count.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="splitChar">What to split it by.</param>
        /// <returns>The split string pieces.</returns>
        public static string[] SplitFast(this string input, char splitChar)
        {
            int count = CountCharacter(input, splitChar);
            string[] resultArray = new string[count + 1];
            int startIndex = 0;
            int currentResultIndex = 0;
            // 'count' guarantees we won't loop past end of string, so loop until count is reached.
            for (int currentInputIndex = 0; currentResultIndex < count; currentInputIndex++)
            {
                if (input[currentInputIndex] == splitChar)
                {
                    resultArray[currentResultIndex++] = input.Substring(startIndex, currentInputIndex - startIndex);
                    startIndex = currentInputIndex + 1;
                }
            }
            resultArray[currentResultIndex] = input.Substring(startIndex);
            return resultArray;
        }

        /// <summary>
        /// Quickly split a string around a splitter character, with a max split count.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="splitChar">What to split it by.</param>
        /// <param name="maxCount">The maximum number of times to split it.
        /// Note that the result array will have a length 1 greater than this input value.</param>
        /// <returns>The split string pieces.</returns>
        public static string[] SplitFast(this string input, char splitChar, int maxCount)
        {
            int count = CountCharacter(input, splitChar);
            count = ((count > maxCount) ? maxCount : count);
            string[] resultArray = new string[count + 1];
            int startIndex = 0;
            int currentResultIndex = 0;
            // 'count' guarantees we won't loop past end of string, so loop until count is reached.
            for (int currentInputIndex = 0; currentResultIndex < count; currentInputIndex++)
            {
                if (input[currentInputIndex] == splitChar)
                {
                    resultArray[currentResultIndex++] = input.Substring(startIndex, currentInputIndex - startIndex);
                    startIndex = currentInputIndex + 1;
                }
            }
            resultArray[currentResultIndex] = input.Substring(startIndex);
            return resultArray;
        }
    }
}
