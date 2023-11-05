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
    }

    private static FFPBuilderFile[] GetFilesIn(string folder)
    {
        folder = Path.GetFullPath(folder);
        List<FFPBuilderFile> filesOutput = new();
        foreach (string file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
        {
            filesOutput.Add(new FFPBuilderFile()
            {
                Name = file[folder.Length..],
                FileObject = file
            });
        }
        return filesOutput.ToArray();
    }

    /// <summary>Creates a <see cref="FFPackage"/> from a file system folder and saves it to a new file.</summary>
    /// <param name="folder">The file system folder.</param>
    /// <param name="outputFile">The file to output to.</param>
    /// <param name="options">The building options.</param>
    public static void CreateFromFolder(string folder, string outputFile, Options options)
    {
        FFPBuilderFile[] inputFiles = GetFilesIn(folder);
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
        CreateFromFiles(GetFilesIn(folder), output, options);
    }

    /// <summary>Creates a <see cref="FFPackage"/> from an array of on-disk files, in-memory files, and stream-backed files.</summary>
    /// <param name="files">List of files.</param>
    /// <param name="output">The stream to output to.</param>
    /// <param name="options">The building options.</param>
    /// <exception cref="InvalidOperationException">If there are duplicate files, or the file cannot be created.</exception>
    public static void CreateFromFiles(FFPBuilderFile[] files, Stream output, Options options)
    {
        HashSet<string> fileSet = new();
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
        output.Write(HEADER, 0, HEADER.Length);
        position += HEADER.Length;
        WriteInt(files.Length, helper, output);
        position += 4;
        FileHeaderInfo[] headers = new FileHeaderInfo[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            headers[i].HeaderPosition = position;
            output.Write(PLACEHOLDER_FILEDATA, 0, PLACEHOLDER_FILEDATA.Length);
            position += PLACEHOLDER_FILEDATA.Length;
            byte[] fileNameBytes = StringConversionHelper.UTF8Encoding.GetBytes(files[i].Name);
            WriteInt(fileNameBytes.Length, helper, output);
            position += 4;
            output.Write(fileNameBytes, 0, fileNameBytes.Length);
            position += fileNameBytes.Length;
        }
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].FileObject is string fileName)
            {
                headers[i].Position = position;
                using FileStream stream = File.OpenRead(fileName);
                headers[i].ActualLength = stream.Length;
                if (options.MayGZip && stream.Length > options.MinimumCompression && stream.Length < options.MaximumCompression)
                {
                    byte[] raw = new byte[stream.Length];
                    FFPUtilities.ReadBytesGuaranteed(stream, raw, (int)stream.Length);
                    byte[] compressed = FFPUtilities.CompressGZip(raw);
                    byte[] toWrite;
                    int ratio = (int)((compressed.Length / (double)raw.Length) * 100);
                    if (ratio < options.RequiredCompressionPercentage)
                    {
                        headers[i].Encoding = (byte)FFPEncoding.GZIP;
                        toWrite = compressed;
                    }
                    else
                    {
                        headers[i].Encoding = (byte)FFPEncoding.RAW;
                        toWrite = raw;
                    }
                    headers[i].FileLength = toWrite.Length;
                    output.Write(toWrite, 0, toWrite.Length);
                    position += toWrite.Length;
                }
                else
                {
                    headers[i].Encoding = (byte)FFPEncoding.RAW;
                    headers[i].FileLength = stream.Length;
                    position += stream.Length;
                    stream.CopyTo(output);
                }
            }
        }
        for (int i = 0; i < files.Length; i++)
        {
            output.Seek(headers[i].HeaderPosition, SeekOrigin.Begin);
            WriteLong(headers[i].Position, helper, output);
            WriteLong(headers[i].FileLength, helper, output);
            output.WriteByte(headers[i].Encoding);
            WriteLong(headers[i].ActualLength, helper, output);
        }
    }

    private struct FileHeaderInfo
    {
        public long HeaderPosition;
        public long Position;
        public long FileLength;
        public byte Encoding;
        public long ActualLength;
    }

    private readonly static byte[] HEADER = new byte[] { (byte)'F', (byte)'F', (byte)'P', (byte)'0', (byte)'0', (byte)'1' };

    private readonly static byte[] PLACEHOLDER_FILEDATA = new byte[8 + 8 + 1 + 8];

    private static void WriteInt(int value, byte[] helper, Stream output)
    {
        PrimitiveConversionHelper.Int32ToBytes(value, helper, 0);
        output.Write(helper, 0, 4);
    }

    private static void WriteLong(long value, byte[] helper, Stream output)
    {
        PrimitiveConversionHelper.Long64ToBytes(value, helper, 0);
        output.Write(helper, 0, 8);
    }
}
