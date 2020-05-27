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

namespace FreneticUtilities.FreneticFilePackage
{
    /// <summary>
    /// A folder within a package.
    /// </summary>
    public class FFPFolder
    {
        /// <summary>
        /// Splits a string into a path.
        /// </summary>
        /// <param name="path">The string of the path, separated by '/'.</param>
        /// <returns>The split path.</returns>
        public static string[] SplitPath(string path)
        {
            return path.Split('/');
        }

        /// <summary>
        /// The contents of the package.
        /// </summary>
        public Dictionary<string, object> Contents = new Dictionary<string, object>();

        /// <summary>
        /// Adds a file to the folder.
        /// </summary>
        /// <param name="path">The full file path, separated by the '/' character.</param>
        /// <param name="file">The actual file.</param>
        /// <param name="overwrite">Whether to overwrite existing files.</param>
        /// <exception cref="InvalidOperationException">If the file cannot be added.</exception>
        public void AddFile(string path, FFPFile file, bool overwrite = false)
        {
            AddFile(SplitPath(path), file, overwrite);
        }

        /// <summary>
        /// Adds a file to the folder.
        /// </summary>
        /// <param name="path">The full file path.</param>
        /// <param name="file">The actual file.</param>
        /// <param name="overwrite">Whether to overwrite existing files.</param>
        /// <exception cref="InvalidOperationException">If the file cannot be added.</exception>
        public void AddFile(string[] path, FFPFile file, bool overwrite = false)
        {
            if (path.Length == 0)
            {
                throw new InvalidOperationException("Path is empty.");
            }
            FFPFolder folder = this;
            for (int i = 0; i < path.Length - 1; i++)
            {
                if (!folder.Contents.TryGetValue(path[i], out object value))
                {
                    FFPFolder createdFolder = new FFPFolder();
                    folder.Contents.Add(path[i], createdFolder);
                    folder = createdFolder;
                    continue;
                }
                if (!(value is FFPFolder newFolder))
                {
                    if (overwrite)
                    {
                        FFPFolder createdFolder = new FFPFolder();
                        folder.Contents[path[i]] = createdFolder;
                        folder = createdFolder;
                        continue;
                    }
                    else
                    {
                        throw new InvalidOperationException("Part of the path is a file, not a folder.");
                    }
                }
                folder = newFolder;
            }
            string finalName = path[^1];
            if (folder.Contents.ContainsKey(finalName))
            {
                if (overwrite)
                {
                    folder.Contents[finalName] = file;
                }
                else
                {
                    throw new InvalidOperationException("File name already exists.");
                }
            }
            else
            {
                folder.Contents.Add(finalName, file);
            }
        }

        /// <summary>
        /// Gets the object at a specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The object.</returns>
        public object GetObjectAt(string[] path)
        {
            FFPFolder folder = this;
            for (int i = 0; i < path.Length; i++)
            {
                if (!folder.Contents.TryGetValue(path[i], out object value))
                {
                    return ((i + 1) == path.Length) ? "File not found." : "Part of the requested path is not present.";
                }
                if ((i + 1) == path.Length)
                {
                    return value;
                }
                if (!(value is FFPFolder newFolder))
                {
                    return "Part of the path is a file, not a folder.";
                }
                folder = newFolder;
            }
            return "Path is empty.";
        }

        /// <summary>
        /// Performs an inline error test on a value, throwing an exception if an error is found.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>The value, unmodified.</returns>
        private object ErrorTest(object value)
        {
            if (value is string errorMessage)
            {
                throw new InvalidOperationException(errorMessage);
            }
            return value;
        }

        /// <summary>
        /// Returns whether a file exists at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Whether the file exists.</returns>
        public bool HasFile(string[] path)
        {
            return GetObjectAt(path) is FFPFile;
        }

        /// <summary>
        /// Returns whether a file exists at the specified path.
        /// </summary>
        /// <param name="path">The path, separated by the '/' symbol.</param>
        /// <returns>Whether the file exists.</returns>
        public bool HasFile(string path)
        {
            return HasFile(SplitPath(path));
        }

        /// <summary>
        /// Returns whether a sub-folder exists at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Whether the sub-folder exists.</returns>
        public bool HasSubFolder(string[] path)
        {
            return GetObjectAt(path) is FFPFolder;
        }

        /// <summary>
        /// Returns whether a sub-folder exists at the specified path.
        /// </summary>
        /// <param name="path">The path, separated by the '/' symbol.</param>
        /// <returns>Whether the sub-folder exists.</returns>
        public bool HasSubFolder(string path)
        {
            return HasSubFolder(SplitPath(path));
        }

        /// <summary>
        /// Gets a sub-folder at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The folder.</returns>
        /// <exception cref="InvalidOperationException">If the path does not point to a folder.</exception>
        public FFPFolder GetSubFolder(string[] path)
        {
            object gotten = ErrorTest(GetObjectAt(path));
            if (gotten is FFPFolder folder)
            {
                return folder;
            }
            throw new InvalidOperationException("Path is not a folder.");
        }
        
        /// <summary>
        /// Gets a sub-folder at the specified path.
        /// </summary>
        /// <param name="path">The path, separated by the '/' symbol.</param>
        /// <returns>The folder.</returns>
        /// <exception cref="InvalidOperationException">If the path does not point to a folder.</exception>
        public FFPFolder GetSubFolder(string path)
        {
            return GetSubFolder(SplitPath(path));
        }

        /// <summary>
        /// Tries to get a sub-folder at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="folder">The folder, if found.</param>
        /// <returns>Whether a folder was gotten.</returns>
        public bool TryGetSubFolder(string[] path, out FFPFolder folder)
        {
            object gotten = GetObjectAt(path);
            folder = gotten as FFPFolder;
            return folder != null;
        }

        /// <summary>
        /// Tries to get a sub-folder at the specified path.
        /// </summary>
        /// <param name="path">The path, separated by the '/' symbol.</param>
        /// <param name="folder">The folder, if found.</param>
        /// <returns>Whether a folder was gotten.</returns>
        public bool TryGetSubFolder(string path, out FFPFolder folder)
        {
            return TryGetSubFolder(SplitPath(path), out folder);
        }

        /// <summary>
        /// Gets a file at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The file.</returns>
        /// <exception cref="InvalidOperationException">If the path does not point to a file.</exception>
        public FFPFile GetFile(string[] path)
        {
            object gotten = ErrorTest(GetObjectAt(path));
            if (gotten is FFPFile file)
            {
                return file;
            }
            throw new InvalidOperationException("Path is not a file.");
        }

        /// <summary>
        /// Gets a file at the specified path.
        /// </summary>
        /// <param name="path">The path, separated by the '/' symbol.</param>
        /// <returns>The file.</returns>
        /// <exception cref="InvalidOperationException">If the path does not point to a file.</exception>
        public FFPFile GetFile(string path)
        {
            return GetFile(SplitPath(path));
        }

        /// <summary>
        /// Gets a file at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="file">The file, if found.</param>
        /// <returns>Whether a file was gotten.</returns>
        public bool TryGetFile(string[] path, out FFPFile file)
        {
            object gotten = GetObjectAt(path);
            file = gotten as FFPFile;
            return file != null;
        }

        /// <summary>
        /// Gets a file at the specified path.
        /// </summary>
        /// <param name="path">The path, separated by the '/' symbol.</param>
        /// <param name="file">The file, if found.</param>
        /// <returns>Whether a file was gotten.</returns>
        public bool TryGetFile(string path, out FFPFile file)
        {
            return TryGetFile(SplitPath(path), out file);
        }

        /// <summary>
        /// Enumerates all files (not folders) contained in this folder.
        /// </summary>
        /// <returns>The file enumerable.</returns>
        public IEnumerable<string> EnumerateFiles()
        {
            foreach (KeyValuePair<string, object> entry in Contents)
            {
                if (entry.Value is FFPFile)
                {
                    yield return entry.Key;
                }
            }
        }

        /// <summary>
        /// Enumerates all folders (not files) contained in this folder.
        /// </summary>
        /// <returns>The folders enumerable.</returns>
        public IEnumerable<string> EnumerateFolders()
        {
            foreach (KeyValuePair<string, object> entry in Contents)
            {
                if (entry.Value is FFPFolder)
                {
                    yield return entry.Key;
                }
            }
        }
    }
}
