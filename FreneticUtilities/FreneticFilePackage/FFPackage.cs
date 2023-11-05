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

/// <summary>The centerpoint class for the Frenetic File Package system. Handles a single file package.</summary>
public class FFPackage
{
    /// <summary>Construct a <see cref="FFPackage"/> from a data stream. Generally a <see cref="FileStream"/> is the stream type to use.</summary>
    /// <param name="dataStream">The stream.</param>
    /// <param name="warning">An action to give warnings. Can be null if warnings should be ignored.</param>
    public FFPackage(Stream dataStream, Action<string> warning)
    {
        FileStream = dataStream;
        ReadHeadersIn(warning);
        Internal.AccessLock = new LockObject();
    }

    private void ReadHeadersIn(Action<string> warning)
    {
        FileStream.Seek(0, SeekOrigin.Begin);
        byte header_1 = GetByte();
        byte header_2 = GetByte();
        byte header_3 = GetByte();
        if (header_1 != 'F' || header_2 != 'F' || header_3 != 'P')
        {
            throw new InvalidOperationException("File is not an FFP file (header failure).");
        }
        byte version_1 = GetByte();
        byte version_2 = GetByte();
        byte version_3 = GetByte();
        if (!(version_1 >= '0' && version_1 <= '9')
            || !(version_2 >= '0' && version_2 <= '9')
            || !(version_3 >= '0' && version_3 <= '9'))
        {
            throw new InvalidOperationException("File is not an FFP file (version failure).");
        }
        int fileVersion = (version_1 - '0') * 100 + (version_2 - '0') * 10 + (version_3 - '0');
        if (fileVersion < 1)
        {
            throw new InvalidOperationException("File is not an FFP file (version 0 unsupported).");
        }
        // Prior version update code goes here if/when needed.
        if (fileVersion > 1)
        {
            warning($"File version '{fileVersion}' is newer than this code supports. Read error may occur.");
        }
        int fileCount = GetInt();
        Files = new Dictionary<string, FFPFile>(fileCount * 2);
        RootFolder = new FFPFolder();
        for (int i = 0; i < fileCount; i++)
        {
            FFPFile file = new()
            {
                Package = this
            };
            file.Internal.StartPosition = GetLong();
            file.Internal.FileLength = GetLong();
            file.Internal.Encoding = (FFPEncoding)GetByte();
            file.Length = GetLong();
            int nameLength = GetInt();
            if (nameLength < 0 || nameLength >= helperBytes.Length)
            {
                throw new InvalidOperationException($"Invalid (too long or too short) filename of size {nameLength}, must be within range 0-{helperBytes.Length}");
            }
            FFPUtilities.ReadBytesGuaranteed(FileStream, helperBytes, nameLength);
            file.FullName = FFPUtilities.CleanFileName(StringConversionHelper.UTF8Encoding.GetString(helperBytes, 0, nameLength));
            file.SimpleName = file.FullName.AfterLast('/');
            if (Files.ContainsKey(file.FullName))
            {
                throw new InvalidOperationException($"Cannot form a package with duplicate file names. Duplicate file name: {file.FullName}");
            }
            Files.Add(file.FullName, file);
            RootFolder.AddFile(file.FullName, file);
        }
        Internal.FileDataStart = FileStream.Position;
    }

    private readonly byte[] helperBytes = new byte[512];

    private byte GetByte()
    {
        int value = FileStream.ReadByte();
        if (value == -1)
        {
            throw new InvalidOperationException("Stream closed early.");
        }
        return (byte)value;
    }

    private int GetInt()
    {
        FFPUtilities.ReadBytesGuaranteed(FileStream, helperBytes, 4);
        int value = PrimitiveConversionHelper.BytesToInt32(helperBytes);
        if (value < 0)
        {
            throw new InvalidOperationException("Value is negative or file package is misformatted.");
        }
        return value;
    }

    private long GetLong()
    {
        FFPUtilities.ReadBytesGuaranteed(FileStream, helperBytes, 8);
        long value = PrimitiveConversionHelper.BytesToLong64(helperBytes);
        if (value < 0)
        {
            throw new InvalidOperationException("Value is negative or file package is misformatted.");
        }
        return value;
    }

    /// <summary>The internal data for this <see cref="FFPackage"/>.</summary>
    public struct InternalData
    {
        /// <summary>Where file data starts at in the backing stream.</summary>
        public long FileDataStart;

        /// <summary>The multi-threaded safety access lock object.</summary>
        public LockObject AccessLock;
    }

    /// <summary>The internal data for this <see cref="FFPackage"/>.</summary>
    public InternalData Internal;

    /// <summary>The backing data stream.</summary>
    public Stream FileStream;

    /// <summary>A mapping of all files contained within the <see cref="FFPackage"/>.</summary>
    public Dictionary<string, FFPFile> Files;

    /// <summary>The root <see cref="FFPFolder"/> of this <see cref="FFPackage"/>.</summary>
    public FFPFolder RootFolder;

    /// <summary>
    /// Gets the data of a file at the specified path.
    /// Locks for safe multithreaded access.
    /// </summary>
    /// <param name="fileName">The name of the file, with path separated by '/'.</param>
    /// <returns>The file data.</returns>
    /// <exception cref="FileNotFoundException">If the file is not present.</exception>
    /// <exception cref="InvalidOperationException">If there is a file reading error.</exception>
    public byte[] GetFileData(string fileName)
    {
        lock (Internal.AccessLock)
        {
            fileName = FFPUtilities.CleanFileName(fileName);
            if (!Files.TryGetValue(fileName, out FFPFile file))
            {
                throw new FileNotFoundException("File is not present in the package.", fileName);
            }
            return file.ReadFileData();
        }
    }

    /// <summary>
    /// Tries to get the data of a file at the specified path.
    /// Locks for safe multithreaded access.
    /// </summary>
    /// <param name="fileName">The name of the file, with path separated by '/'.</param>
    /// <param name="data">The file data, if found.</param>
    /// <returns>Whether the file was found.</returns>
    /// <exception cref="InvalidOperationException">If there is a file reading error.</exception>
    public bool TryGetFileData(string fileName, out byte[] data)
    {
        lock (Internal.AccessLock)
        {
            fileName = FFPUtilities.CleanFileName(fileName);
            if (!Files.TryGetValue(fileName, out FFPFile file))
            {
                data = null;
                return false;
            }
            data = file.ReadFileData();
            return true;
        }
    }

    /// <summary>Gets the number of files in this <see cref="FFPackage"/>.</summary>
    public int FileCount
    {
        get
        {
            return Files.Count;
        }
    }
}
