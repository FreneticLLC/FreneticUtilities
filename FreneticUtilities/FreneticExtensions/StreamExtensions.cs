//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>Helper extensions for <see cref="Stream"/>.</summary>
    public static class StreamExtensions
    {
        /// <summary>Returns all the lines of text within a readable stream.</summary>
        /// <param name="input">The input stream.</param>
        /// <returns>All lines of text, separated.</returns>
        public static IEnumerable<string> AllLinesOfText(this Stream input)
        {
            using StreamReader reader = new(input);
            while (true)
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
