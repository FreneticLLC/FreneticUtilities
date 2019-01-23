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
using System.Runtime.CompilerServices;

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>
    /// Helper extensions for <see cref="Stream"/>.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Returns all the lines of text within a readable stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>All lines of text, separated.</returns>
        public static IEnumerable<string> AllLinesOfText(this Stream input)
        {
            using (StreamReader reader = new StreamReader(input))
            {
                while (true) // TODO: Cleaner way to write this loop?
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    yield return line;
                }
            }
        }
    }
}
