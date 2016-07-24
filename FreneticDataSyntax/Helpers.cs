using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticDataSyntax
{
    /// <summary>
    /// Helpers for the FreneticDataSyntax engine.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Rapidly converts an ASCII string to a lowercase representation.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>A lowercase version.</returns>
        public static string ToLowerFast(this string input)
        {
            char[] dt = input.ToCharArray();
            for (int i = 0; i < dt.Length; i++)
            {
                if (dt[i] >= 'A' && dt[i] <= 'Z')
                {
                    dt[i] = (char)(dt[i] - ('A' - 'a'));
                }
            }
            return new string(dt);
        }

        /// <summary>
        /// Quickly split a string.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="splitter">What to split it by.</param>
        /// <param name="count">The maximum number of times to split it.</param>
        /// <returns>The split string pieces.</returns>
        public static string[] SplitFast(this string input, char splitter, int count = int.MaxValue)
        {
            int len = input.Length;
            int c = 0;
            for (int i = 0; i < len; i++)
            {
                if (input[i] == splitter)
                {
                    c++;
                }
            }
            c = ((c > count) ? count : c);
            string[] res = new string[c + 1];
            int start = 0;
            int x = 0;
            for (int i = 0; i < len && x < c; i++)
            {
                if (input[i] == splitter)
                {
                    res[x++] = input.Substring(start, i - start);
                    start = i + 1;
                }
            }
            res[x] = input.Substring(start);
            return res;
        }
    }
}
