//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>
    /// Utilities for the FreneticDataSyntax engine.
    /// </summary>
    public static class FDSUtility
    {
        /// <summary>
        /// The default splitter character for section paths.
        /// To change to or a custom default, use <see cref="DefaultSectionPathSplit"/>.
        /// To change for a specific section, use <see cref="FDSSection.SectionPathSplit"/>.
        /// This is a dot value.
        /// </summary>
        public const char DEFAULT_SECTION_PATH_SPLIT = '.';

        /// <summary>
        /// The default splitter character for section paths.
        /// For the internal unmodified default, use <see cref="DEFAULT_SECTION_PATH_SPLIT"/>.
        /// To change for a specific section, use <see cref="FDSSection.SectionPathSplit"/>.
        /// This is initially a dot value. Altering this may cause issues (in particular with escaping) depending on the chosen value.
        /// </summary>
        public static char DefaultSectionPathSplit = DEFAULT_SECTION_PATH_SPLIT;

        /// <summary>
        /// Reads a file into an <see cref="FDSSection"/>. Throws normal exceptions on any issue.
        /// NOTE: May be removed or switched for journalling logic in the future.
        /// </summary>
        /// <param name="fname">The name of the file to read.</param>
        /// <returns>An <see cref="FDSSection"/> containing the same data as the file (if successfully read).</returns>
        public static FDSSection ReadFile(string fname)
        {
            return new FDSSection(StringConversionHelper.UTF8Encoding.GetString(File.ReadAllBytes(fname)));
        }

        /// <summary>
        /// Saves an <see cref="FDSSection"/> into a file. Throws normal exceptions on any issue.
        /// NOTE: May be removed or switched for journalling logic in the future.
        /// </summary>
        /// <param name="section">The data to save.</param>
        /// <param name="fname">The name of the file to read.</param>
        public static void SaveToFile(this FDSSection section, string fname)
        {
            File.WriteAllBytes(fname, StringConversionHelper.UTF8Encoding.GetBytes(section.SaveToString()));
        }

        private static readonly byte[] EMPTY_BYTES = new byte[0];

        /// <summary>
        /// Converts a Base64 string to a byte array.
        /// </summary>
        /// <param name="inputString">The input string to convert.</param>
        /// <returns>The byte array output.</returns>
        public static byte[] FromBase64(string inputString)
        {
            if (inputString.Length == 0)
            {
                return EMPTY_BYTES;
            }
            return Convert.FromBase64String(inputString);
        }

        /// <summary>
        /// Cleans file line endings, tabs, and any other data that may cause issues.
        /// </summary>
        /// <param name="contents">The original file data.</param>
        /// <returns>The cleaned file data.</returns>
        public static string CleanFileData(string contents)
        {
            if (contents.Contains("\r\n"))
            {
                // Windows to Unix
                contents = contents.Replace("\r", "");
            }
            else
            {
                // Old Mac to Unix (leaves Unix form unaltered)
                contents = contents.Replace('\r', '\n');
            }
            return contents.Replace("\t", "    "); // 4 spaces
        }

        /// <summary>
        /// Escapes a string for output.
        /// <para>Only good for values. For keys, use <see cref="EscapeKey(string)"/>.</para>
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string Escape(string str)
        {
            str = str.Replace("\\", "\\s").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
            if (str.EndWithFast(' '))
            {
                str = str + "\\x";
            }
            if (str.StartsWithFast(' '))
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
        /// <para>Only good for values. For keys, use <see cref="UnEscapeKey(string)"/>.</para>
        /// </summary>
        /// <param name="str">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        public static string UnEscape(string str)
        {
            str = str.Replace("\\t", "\t").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\x", "").Replace("\\s", "\\");
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
            if (long.TryParse(input, out long aslong) && aslong.ToString() == input)
            {
                return aslong;
            }
            if (double.TryParse(input, out double asdouble) && asdouble.ToString() == input)
            {
                return asdouble;
            }
            if (input == "true")
            {
                return true;
            }
            if (input == "false")
            {
                return false;
            }
            return input;
        }

        /// <summary>
        /// Processes an input object to standardize it for FDS.
        /// </summary>
        /// <param name="input">The original input object.</param>
        /// <returns>The cleaned proper FDS object.</returns>
        public static object ProcessObject(object input)
        {
            if (input is List<string> stringList)
            {
                return stringList.Select(s => new FDSData() { Internal = s }).ToList();
            }
            return input;
        }
    }
}
