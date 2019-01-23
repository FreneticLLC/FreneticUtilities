//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>
    /// Helper extensions for various types without better extension files.
    /// </summary>
    public static class OtherExtensions
    {
        /// <summary>
        /// Gets a Gaussian random value from a Random object.
        /// </summary>
        /// <param name="input">The random object.</param>
        /// <returns>The Gaussian value.</returns>
        public static double NextGaussian(this Random input)
        {
            double u1 = input.NextDouble();
            double u2 = input.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}
