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
using System.IO;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticFilePackage
{
    /// <summary>
    /// Represents a single file in a <see cref="FFPackage"/>.
    /// </summary>
    public class FFPFile
    {
        /// <summary>
        /// The internal data for this <see cref="FFPFile"/>.
        /// </summary>
        public struct InternalData
        {
            /// <summary>
            /// The index in the backing stream this file starts at.
            /// </summary>
            public long StartPosition;

            /// <summary>
            /// The length in the backing stream.
            /// </summary>
            public long FileLength;

            /// <summary>
            /// The encoding in use.
            /// </summary>
            public FFPEncoding Encoding;
        }

        /// <summary>
        /// The internal data for this <see cref="FFPFile"/>.
        /// </summary>
        public InternalData Internal;

        /// <summary>
        /// The backing file package.
        /// </summary>
        public FFPackage Package;

        /// <summary>
        /// The length of the file.
        /// </summary>
        public long Length;

        /// <summary>
        /// The full name of the file, with any path data.
        /// </summary>
        public string FullName;

        /// <summary>
        /// The simple name of the file (path data removed).
        /// </summary>
        public string SimpleName;

        /// <summary>
        /// Locker used to prevent overlapping file reads across multiple threads.
        /// </summary>
        public LockObject Locker = new LockObject();

        /// <summary>
        /// Returns a byte array of the actual file data.
        /// </summary>
        /// <returns>The actual file data.</returns>
        public byte[] ReadFileData()
        {
            byte[] output = new byte[Internal.FileLength];
            lock (Locker)
            {
                Package.FileStream.Seek(Internal.StartPosition, SeekOrigin.Begin);
                FFPUtilities.ReadBytesGuaranteed(Package.FileStream, output, (int)Internal.FileLength);
            }
            return FFPUtilities.Decode(output, Internal.Encoding);
        }
    }
}
