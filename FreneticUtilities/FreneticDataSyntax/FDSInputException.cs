//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>
    /// Represents an exception thrown while inputting data to an FDS section.
    /// </summary>
    [Serializable]
    public class FDSInputException : Exception
    {
        /// <summary>
        /// Construct the FDS exception.
        /// </summary>
        /// <param name="message">The message explaining the error.</param>
        public FDSInputException(string message)
            : base(message)
        {
            // No init needed.
        }
    }
}
