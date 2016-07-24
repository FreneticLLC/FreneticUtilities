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
            str = str.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
            if (str.EndsWith(" "))
            {
                str = str + "\\x";
            }
            if (str.StartsWith(" "))
            {
                str = "\\x" + str;
            }
            return str;
        }

        /// <summary>
        /// Escapes a string for usage as a section key.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeKey(string str)
        {
            return Escape(str).Replace(".", "\\d").Replace(":", "\\c").Replace("=", "\\e");
        }

        /// <summary>
        /// UnEscapes a string for output.
        /// </summary>
        /// <param name="str">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        public static string UnEscape(string str)
        {
            str = str.Replace("\\t", "\t").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\x", "").Replace("\\\\", "\\");
            return str;
        }

        /// <summary>
        /// UnEscapes a string for usage as a section key.
        /// </summary>
        /// <param name="str">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        public static string UnEscapeKey(string str)
        {
            return UnEscape(str.Replace("\\d", ".").Replace("\\c", ":").Replace("\\e", "="));
        }

        /// <summary>
        /// Interprets the type of the input text.
        /// </summary>
        /// <param name="input">The input text.</param>
        /// <returns>The correctly typed result.</returns>
        public static object InterpretType(string input)
        {
            long aslong;
            if (long.TryParse(input, out aslong) && aslong.ToString() == input)
            {
                return aslong;
            }
            double asdouble;
            if (double.TryParse(input, out asdouble) && asdouble.ToString() == input)
            {
                return asdouble;
            }
            return input;
        }

        /// <summary>
        /// Appends a number of spaces to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="spaces">The spaces.</param>
        public static void AppendSpaces(StringBuilder sb, int spaces)
        {
            for (int i = 0; i < spaces; i++)
            {
                sb.Append(' ');
            }
        }
    }
}
