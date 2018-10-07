//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2018 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
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
    /// Helper extensions for <see cref="System.IO.Stream"/>.
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
