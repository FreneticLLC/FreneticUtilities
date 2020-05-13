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

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>
    /// Used for C# locking (which requires an arbitrary object reference to map the monitor lock to).
    /// Serves as a better placeholder empty object to use instead of just raw 'object'.
    /// </summary>
    public sealed class LockObject
    {
    }
}
