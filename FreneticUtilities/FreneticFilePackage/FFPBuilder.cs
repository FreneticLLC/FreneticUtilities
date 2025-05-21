//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticFilePackage;

/// <summary>A helper class to create new <see cref="FFPackage"/>s.</summary>
public static class FFPBuilder
{
    /// <summary>Represents a single file that will go into a <see cref="FFPackage"/>.</summary>
    public class FFPBuilderFile
    {
        /// <summary>The name (in the final package) of the file.</summary>
        public string Name;

        /// <summary>
        /// The file object.
        /// May be a <see cref="string"/> of a file name on file system,
        /// a <see cref="byte"/>[] of raw file data,
        /// or a <see cref="Stream"/> of file data.
        /// </summary>
        public object FileObject;
    }

    /// <summary>Options for building a <see cref="FFPackage"/>.</summary>
    public class Options
    {
        /// <summary>Whether GZip compression is allowed.</summary>
        public bool MayGZip = true;

        /// <summary>
        /// Minimum length before compression is considered.
        /// The default value is 1024 (1 kilobyte).
        /// </summary>
        public long MinimumCompression = 1024;

        /// <summary>
        /// Maximum length to consider compressing.
        /// The default value is (1024 * 1024 * 32) (32 megabytes).
        /// </summary>
        public long MaximumCompression = 1024 * 1024 * 32;

        /// <summary>
        /// The minimum compression ratio (percentage) to store.
        /// The default value is 70, meaning the compressed size must be 70% or less of the raw size.
        /// </summary>
        public int RequiredCompressionPercentage = 70;

        /// <summary>
        /// File extensions to exclude from compression (because they are already compressed or can't be reasonably compressed), eg "png", "jpg", etc.
        /// </summary>
        public HashSet<string> ExcludeCompressionExtensions = ["png", "jpg", "webp", "webm", "mp4", "mkv"];
    }

    /// <summary>Creates a <see cref="FFPackage"/> from a file system folder and saves it to a new file.</summary>
    /// <param name="folder">The file system folder.</param>
    /// <param name="outputFile">The file to output to.</param>
    /// <param name="options">The building options.</param>
    public static void CreateFromFolder(string folder, string outputFile, Options options)
    {
        FFPBuilderFile[] inputFiles = InternalData.GetFilesIn(folder);
        using FileStream output = File.OpenWrite(outputFile);
        CreateFromFiles(inputFiles, output, options);
        output.Flush(true);
    }
    /// <summary>Creates a <see cref="FFPackage"/> from a file system folder.</summary>
    /// <param name="folder">The file system folder.</param>
    /// <param name="output">The stream to output to.</param>
    /// <param name="options">The building options.</param>
    public static void CreateFromFolder(string folder, Stream output, Options options)
    {
        CreateFromFiles(InternalData.GetFilesIn(folder), output, options);
    }

    /// <summary>Creates a <see cref="FFPackage"/> from an array of on-disk files, in-memory files, and stream-backed files.</summary>
    /// <param name="files">List of files.</param>
    /// <param name="output">The stream to output to.</param>
    /// <param name="options">The building options.</param>
    /// <exception cref="InvalidOperationException">If there are duplicate files, or the file cannot be created.</exception>
    public static void CreateFromFiles(FFPBuilderFile[] files, Stream output, Options options)
    {
        HashSet<string> fileSet = [];
        for (int i = 0; i < files.Length; i++)
        {
            files[i].Name = FFPUtilities.CleanFileName(files[i].Name);
            if (files[i].Name.Length == 0)
            {
                throw new InvalidOperationException("Cannot have empty file names.");
            }
            if (!fileSet.Add(files[i].Name))
            {
                throw new InvalidOperationException("Cannot form a package with duplicate files. Duplicate file name: " + files[i].Name);
            }
        }
        long position = 0;
        byte[] helper = new byte[8];
        output.Write(InternalData.HEADER, 0, InternalData.HEADER.Length);
        position += InternalData.HEADER.Length;
        InternalData.WriteInt(files.Length, helper, output);
        position += 4;
        InternalData.FileHeaderInfo[] headers = new InternalData.FileHeaderInfo[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            headers[i].HeaderPosition = position;
            output.Write(InternalData.PLACEHOLDER_FILEDATA, 0, InternalData.PLACEHOLDER_FILEDATA.Length);
            position += InternalData.PLACEHOLDER_FILEDATA.Length;
            byte[] fileNameBytes = StringConversionHelper.UTF8Encoding.GetBytes(files[i].Name);
            InternalData.WriteInt(fileNameBytes.Length, helper, output);
            position += 4;
            output.Write(fileNameBytes, 0, fileNameBytes.Length);
            position += fileNameBytes.Length;
        }
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].FileObject is string fileName)
            {
                InternalData.WriteFileDataToStream(headers[i], ref position, output, fileName, options);
            }
        }
        for (int i = 0; i < files.Length; i++)
        {
            output.Seek(headers[i].HeaderPosition, SeekOrigin.Begin);
            InternalData.WriteLong(headers[i].Position, helper, output);
            InternalData.WriteLong(headers[i].FileLength, helper, output);
            output.WriteByte(headers[i].Encoding);
            InternalData.WriteLong(headers[i].ActualLength, helper, output);
        }
    }

    /// <summary>Internal data for the <see cref="FFPackage"/>.</summary>
    public class InternalData
    {
        /// <summary>Internal handler to write data from a file to a stream, automatically handling any required conversions.</summary>
        public static void WriteFileDataToStream(FileHeaderInfo header, ref long position, Stream output, string fileName, Options options)
        {
            header.Position = position;
            string extension = fileName.AfterLast('.');
            using FileStream stream = File.OpenRead(fileName);
            header.ActualLength = stream.Length;
            if (options.MayGZip && stream.Length > options.MinimumCompression && stream.Length < options.MaximumCompression && !options.ExcludeCompressionExtensions.Contains(extension))
            {
                byte[] raw = new byte[stream.Length];
                FFPUtilities.ReadBytesGuaranteed(stream, raw, (int)stream.Length);
                byte[] compressed = FFPUtilities.CompressGZip(raw);
                byte[] toWrite;
                int ratio = (int)((compressed.Length / (double)raw.Length) * 100);
                if (ratio < options.RequiredCompressionPercentage)
                {
                    header.Encoding = (byte)FFPEncoding.GZIP;
                    toWrite = compressed;
                }
                else
                {
                    header.Encoding = (byte)FFPEncoding.RAW;
                    toWrite = raw;
                }
                header.FileLength = toWrite.Length;
                output.Write(toWrite, 0, toWrite.Length);
                position += toWrite.Length;
            }
            else
            {
                header.Encoding = (byte)FFPEncoding.RAW;
                header.FileLength = stream.Length;
                position += stream.Length;
                stream.CopyTo(output);
            }
        }

        /// <summary>Gets <see cref="FFPBuilderFile"/> instances for every file in a given folder and sub-folders.</summary>
        public static FFPBuilderFile[] GetFilesIn(string folder)
        {
            folder = Path.GetFullPath(folder);
            List<FFPBuilderFile> filesOutput = [];
            foreach (string file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                filesOutput.Add(new FFPBuilderFile()
                {
                    Name = file[folder.Length..],
                    FileObject = file
                });
            }
            return [.. filesOutput];
        }

        /// <summary>Info for a file header.</summary>
        public struct FileHeaderInfo
        {
            /// <summary>The position of the header itself in the file.</summary>
            public long HeaderPosition;
            /// <summary>The position of the file data in the file.</summary>
            public long Position;
            /// <summary>The encoded length of the file data in the file (eg after compression).</summary>
            public long FileLength;
            /// <summary>The encoding ID for the file data (raw vs compressed).</summary>
            public byte Encoding;
            /// <summary>The true length of the file data.</summary>
            public long ActualLength;
        }

        /// <summary>The header for a <see cref="FFPackage"/>, "FFP001".</summary>
        public readonly static byte[] HEADER = "FFP001"u8.ToArray();

        /// <summary>The placeholder file data for a header - empty bytes of the same length as <see cref="FileHeaderInfo"/>.</summary>
        public readonly static byte[] PLACEHOLDER_FILEDATA = new byte[8 + 8 + 1 + 8];

        /// <summary>Writes an int32 to a stream.</summary>
        public static void WriteInt(int value, byte[] helper, Stream output)
        {
            PrimitiveConversionHelper.Int32ToBytes(value, helper, 0);
            output.Write(helper, 0, 4);
        }

        /// <summary>Writes an int64 to a stream.</summary>
        public static void WriteLong(long value, byte[] helper, Stream output)
        {
            PrimitiveConversionHelper.Long64ToBytes(value, helper, 0);
            output.Write(helper, 0, 8);
        }
    }
}
