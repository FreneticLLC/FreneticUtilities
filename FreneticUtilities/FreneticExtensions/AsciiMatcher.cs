using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>
    /// Helper class to match ASCII characters efficiently.
    /// </summary>
    public class AsciiMatcher
    {
        /// <summary>
        /// Maximum value considered part of the ASCII range (127).
        /// </summary>
        public static readonly int MAX_ASCII = 127;

        /// <summary>
        /// Minimum value considered outside the ASCII range (128) (<see cref="MAX_ASCII"/> + 1).
        /// </summary>
        public static readonly int MIN_NON_ASCII = MAX_ASCII + 1;

        /// <summary>
        /// Array of booleans, sized as <see cref="MIN_NON_ASCII"/>, such that "Chars[c]" where 'c' is any ASCII character is the validity of that character.
        /// </summary>
        public bool[] Chars = new bool[MIN_NON_ASCII];

        /// <summary>
        /// Construct the matcher from a string of valid symbols.
        /// </summary>
        /// <param name="valid">The valid symbol string.</param>
        public AsciiMatcher(string valid)
        {
            for (int i = 0; i < MIN_NON_ASCII; i++)
            {
                Chars[i] = false;
            }
            for (int i = 0; i < valid.Length; i++)
            {
                Chars[valid[i]] = true;
            }
        }

        /// <summary>
        /// Construct the matcher from a function that evaluates whether any symbol is valid.
        /// </summary>
        /// <param name="isMatch">The validation function.</param>
        public AsciiMatcher(Func<char, bool> isMatch)
        {
            for (int i = 0; i < MIN_NON_ASCII; i++)
            {
                Chars[i] = isMatch((char)i);
            }
        }

        /// <summary>
        /// Returns whether a character is considered valid.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>Whether it is valid.</returns>
        public bool IsMatch(char c)
        {
            return c > MAX_ASCII ? false : Chars[c];
        }

        /// <summary>
        /// Returns whether a string only contains matching symbols.
        /// </summary>
        /// <param name="s">The string to check.</param>
        /// <returns>Whether it is exclusively valid.</returns>
        public bool IsOnlyMatches(string s)
        {
            if (s == null)
            {
                return false;
            }
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsMatch(s[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the string with only matching characters included (and non-matches removed).
        /// </summary>
        /// <param name="s">The original string.</param>
        /// <returns>The trimmed string.</returns>
        public string TrimToMatches(string s)
        {
            StringBuilder newString = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (IsMatch(s[i]))
                {
                    newString.Append(s[i]);
                }
            }
            return newString.ToString();
        }

        /// <summary>
        /// Returns the string with only non-matching characters included (and matches removed).
        /// </summary>
        /// <param name="s">The original string.</param>
        /// <returns>The trimmed string.</returns>
        public string TrimToNonMatches(string s)
        {
            StringBuilder newString = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsMatch(s[i]))
                {
                    newString.Append(s[i]);
                }
            }
            return newString.ToString();
        }
    }
}
