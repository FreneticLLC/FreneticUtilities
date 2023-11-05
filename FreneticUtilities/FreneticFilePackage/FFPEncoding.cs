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

namespace FreneticUtilities.FreneticFilePackage;

/// <summary>The valid options for how to encode a file in a <see cref="FFPackage"/>.</summary>
public enum FFPEncoding : byte
{
    /// <summary>The binary data in the file is the actual data.</summary>
    RAW = 0,
    /// <summary>The binary data in the file is encoded by the GZip compression algorithm.</summary>
    GZIP = 1
}
