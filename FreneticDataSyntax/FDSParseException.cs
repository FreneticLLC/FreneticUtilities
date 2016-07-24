using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticDataSyntax
{
    /// <summary>
    /// Represents an exception throw while parsing FDS contents.
    /// </summary>
    public class FDSParseException : Exception
    {
        /// <summary>
        /// Construct the FDS exception.
        /// </summary>
        /// <param name="message">The message explaining the error.</param>
        public FDSParseException(string message)
            : base(message)
        {
            // No init needed.
        }
    }
}
