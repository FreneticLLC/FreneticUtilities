//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticFilePackage
{
    /// <summary>
    /// Helper utilities for <see cref="FFPackage"/>.
    /// </summary>
    public static class FFPUtilities
    {
        /// <summary>
        /// An <see cref="AsciiMatcher"/> for file-name-valid symbols.
        /// </summary>
        public static AsciiMatcher FileNameValidator = new AsciiMatcher(AsciiMatcher.LowercaseLetters + AsciiMatcher.Digits + "_. /");

        /// <summary>
        /// Cleans a string to only valid symbols for a file name to contain.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The cleaned file name.</returns>
        public static string CleanFileName(string input)
        {
            return FileNameValidator.TrimToMatches(input.ToLowerFast().Replace('\\', '/')).Trim('/', ' ');
        }

        /// <summary>
        /// Decodes data stored with a specified encoding.
        /// </summary>
        /// <param name="input">The input binary data.</param>
        /// <param name="encoding">The encoding in use.</param>
        /// <returns>The output data.</returns>
        public static byte[] Decode(byte[] input, FFPEncoding encoding)
        {
            switch (encoding)
            {
                case FFPEncoding.RAW:
                    return input;
                case FFPEncoding.GZIP:
                    MemoryStream inputStream = new MemoryStream(input);
                    using (MemoryStream outputStream = new MemoryStream())
                    using (GZipStream GZStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        GZStream.CopyTo(outputStream);
                        GZStream.Flush();
                        return outputStream.ToArray();
                    }
                default:
                    throw new NotSupportedException("Cannot decode from encoding: " + encoding);
            }
        }

        /// <summary>
        /// Compresses the data using the GZip compression algorithm.
        /// </summary>
        /// <param name="input">The input data (uncompressed).</param>
        /// <returns>The output data (compressed).</returns>
        public static byte[] CompressGZip(byte[] input)
        {
            MemoryStream outputStream = new MemoryStream();
            using (GZipStream GZStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                GZStream.Write(input, 0, input.Length);
            }
            return outputStream.ToArray();
        }

        /// <summary>
        /// Encodes data stored with a specified encoding.
        /// </summary>
        /// <param name="input">The input binary data.</param>
        /// <param name="encoding">The encoding in use.</param>
        /// <returns>The output data.</returns>
        public static byte[] Encode(byte[] input, FFPEncoding encoding)
        {
            return encoding switch
            {
                FFPEncoding.RAW => input,
                FFPEncoding.GZIP => CompressGZip(input),
                _ => throw new NotSupportedException("Cannot decode from encoding: " + encoding),
            };
        }
        /// <summary>
        /// Provides a decoding stream for data stored with a specified encoding.
        /// </summary>
        /// <param name="input">The input binary data stream.</param>
        /// <param name="encoding">The encoding in use.</param>
        /// <returns>The output data stream.</returns>
        public static Stream DecodeStream(Stream input, FFPEncoding encoding)
        {
            return encoding switch
            {
                FFPEncoding.RAW => input,
                FFPEncoding.GZIP => new GZipStream(input, CompressionMode.Decompress),
                _ => throw new NotSupportedException("Cannot decode from encoding: " + encoding),
            };
        }

        /// <summary>
        /// Provides an encoding stream for data stored with a specified encoding.
        /// </summary>
        /// <param name="input">The input binary data stream.</param>
        /// <param name="encoding">The encoding in use.</param>
        /// <returns>The output data stream.</returns>
        public static Stream EncodeStream(Stream input, FFPEncoding encoding)
        {
            switch (encoding)
            {
                case FFPEncoding.RAW:
                    return input;
                case FFPEncoding.GZIP:
                    MemoryStream outputStream = new MemoryStream();
                    using (GZipStream gzStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        input.CopyTo(gzStream);
                    }
                    return outputStream;
                default:
                    throw new NotSupportedException("Cannot decode from encoding: " + encoding);
            }
        }

        /// <summary>
        /// Reads a required number of bytes from the input stream. Does not return until all bytes are read.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="outputArray">The output byte array.</param>
        /// <param name="length">The length to read.</param>
        /// <exception cref="InvalidOperationException">When the stream closes without providing the require byte count.</exception>
        public static void ReadBytesGuaranteed(Stream input, byte[] outputArray, int length)
        {
            int totalRead = 0;
            while (totalRead < length)
            {
                int justRead = input.Read(outputArray, totalRead, length - totalRead);
                if (justRead < 0)
                {
                    throw new InvalidOperationException("Stream refused to continue reading, with " + (length - totalRead) + " bytes still required.");
                }
                totalRead += justRead;
            }
        }
    }
}
