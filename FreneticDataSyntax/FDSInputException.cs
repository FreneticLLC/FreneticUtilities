using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticDataSyntax
{
    /// <summary>
    /// Represents an exception throw while inputting data to an FDS section.
    /// </summary>
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
