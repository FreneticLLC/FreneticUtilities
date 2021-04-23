//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Globalization;
using System.Threading;

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>Special tools that do unusual things.</summary>
    public static class SpecialTools
    {
        /// <summary>Forces the current .NET environment to use the "Invariant Culture". This prevents a large number of bugs caused by Microsoft's broken localization logic.</summary>
        /// <returns>The original localization data.</returns>
        public static CultureInfo Internationalize()
        {
            CultureInfo info = CultureInfo.DefaultThreadCurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            return info;
        }
    }
}
