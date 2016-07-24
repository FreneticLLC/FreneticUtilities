using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticDataSyntax
{
    /// <summary>
    /// Utilities for the FreneticDataSyntax engine.
    /// </summary>
    public static class FDSUtility
    {
        /// <summary>
        /// Cleans file line endings, tabs, and any other data that may cause issues.
        /// </summary>
        /// <param name="contents">The original file data.</param>
        /// <returns>The cleaned file data.</returns>
        public static string CleanFileData(string contents)
        {
            if (contents.Contains("\r\n"))
            {
                contents = contents.Replace("\r", "");
            }
            else
            {
                contents = contents.Replace('\r', '\n');
            }
            return contents.Replace("\t", "    ");
        }

        /// <summary>
        /// Rapidly lowercases an ASCII string.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <returns>The lowercased variant.</returns>
        public static string ToLowerFast(string str)
        {
            return str.ToLowerFast();
        }

        /// <summary>
        /// Escapes a string for output.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string Escape(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        /// <summary>
        /// Escapes a string for usage as a section key.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeKey(string str)
        {
            return Escape(str).Replace(".", "\\d");
        }
    }
}
