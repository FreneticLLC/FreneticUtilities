//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticFilePackage;

namespace FreneticUtilitiesTester
{
    /// <summary>
    /// Entry point for the tester program.
    /// </summary>
    public static class TesterProgram
    {
        /// <summary>
        /// Initializes the program cleanly, fixing the system culture info to prevent glitches.
        /// </summary>
        public static void InitClean()
        {
            string x = ((new Random().Next(5) + 12) / (double)10).ToString();
            if (!x.Contains('.'))
            {
                Console.WriteLine("Warning - system culture is configured to an unsafe culture."
                    + " This means text may be misformatted or otherwise unclear / inconsistent.");
                Console.WriteLine("This tester program will change locally to run with Invariant Culture to avoid issues."
                    + " It is recommended that you fix your PC culture settings to avoid issues outside the tester program.");
            }
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Entry point main method for the tester program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            InitClean();
            string command = args.Length < 1 ? "" : args[0].ToLowerFast();
            switch (command)
            {
                case "pack-folder":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: pack-folder [folder] [output file]");
                        break;
                    }
                    try
                    {
                        FFPBuilder.CreateFromFolder(args[1], args[2], new FFPBuilder.Options());
                        Console.WriteLine("Packing complete.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Packing failed: " + ex.ToString());
                    }
                    break;
                case "pack-show":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: pack-show [package file]");
                        break;
                    }
                    try
                    {
                        using FileStream stream = File.OpenRead(args[1]);
                        FFPackage package = new FFPackage(stream, (warn) => Console.WriteLine("FFPackage Warning: " + warn));
                        Console.WriteLine("Package has " + package.FileCount + " file(s)...");
                        OutputFolder(1, package.RootFolder);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Package showing failed: " + ex.ToString());
                    }
                    break;
                case "pack-dump":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: pack-dump [package file] [output folder]");
                        break;
                    }
                    try
                    {
                        using FileStream stream = File.OpenRead(args[1]);
                        FFPackage package = new FFPackage(stream, (warn) => Console.WriteLine("FFPackage Warning: " + warn));
                        Console.WriteLine("Package has " + package.FileCount + " file(s)...");
                        DumpFolder(1, package.RootFolder, args[2]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Package dumping failed: " + ex.ToString());
                    }
                    break;
                case "fds-resave":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: fds-resave [input file] [output file]");
                        break;
                    }
                    try
                    {
                        FDSSection section = FDSUtility.ReadFile(args[1]);
                        FDSUtility.SaveToFile(section, args[2]);
                        Console.WriteLine("Resave complete.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("FDS resave failed: " + ex.ToString());
                    }
                    break;
                case "version":
                    string programversion = typeof(TesterProgram).Assembly.GetName().Version.ToString();
                    string utilversion = typeof(OtherExtensions).Assembly.GetName().Version.ToString();
                    Console.WriteLine("Frenetic Utilities Tester Program v" + programversion
                        + ", attached to Frenetic Utilities v" + utilversion + ".");
                    break;
                case "help":
                default:
                    Console.WriteLine("Frenetic Utilities Tester Program");
                    Console.WriteLine("Sub-commands available:");
                    Console.WriteLine("  pack-folder [folder] [output file]       | Packages a folder using FFP");
                    Console.WriteLine("  pack-show [input file]                   | Shows the contents of a package");
                    Console.WriteLine("  pack-dump [package file] [output folder] | Dumps the contents of a package");
                    Console.WriteLine("  fds-resave [input file] [output file]    | Loads in and saves back an FDS document");
                    Console.WriteLine("  help                                     | This help output");
                    Console.WriteLine("  version                                  | Displays program version");
                    break;
            }
        }

        /// <summary>
        /// Outputs the contents of a folder to console.
        /// </summary>
        /// <param name="tabs">How much to tab out.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="outputPath">The path to output data to.</param>
        public static void DumpFolder(int tabs, FFPFolder folder, string outputPath)
        {
            Directory.CreateDirectory(outputPath);
            string tabstring = new string(' ', tabs * 2);
            foreach (KeyValuePair<string, object> entry in folder.Contents)
            {
                if (entry.Value is FFPFolder subfolder)
                {
                    Console.WriteLine(tabstring + "| " + entry.Key + ":");
                    DumpFolder(tabs + 1, subfolder, outputPath + "/" + entry.Key);
                }
                else if (entry.Value is FFPFile file)
                {
                    Console.WriteLine(tabstring + "- " + entry.Key + ": " + file.Length + " bytes of data, with " + file.Internal.FileLength + " bytes encoded as " + file.Internal.Encoding);
                    File.WriteAllBytes(outputPath + "/" + entry.Key, file.ReadFileData());
                }
                else
                {
                    Console.WriteLine(tabstring + "* Unknown (broken?) entry at " + entry.Key + ": " + entry.Value);
                }
            }
        }

        /// <summary>
        /// Outputs the contents of a folder to console.
        /// </summary>
        /// <param name="tabs">How much to tab out.</param>
        /// <param name="folder">The folder.</param>
        public static void OutputFolder(int tabs, FFPFolder folder)
        {
            string tabstring = new string(' ', tabs * 2);
            foreach (KeyValuePair<string, object> entry in folder.Contents)
            {
                if (entry.Value is FFPFolder subfolder)
                {
                    Console.WriteLine(tabstring + "| " + entry.Key + ":");
                    OutputFolder(tabs + 1, subfolder);
                }
                else if (entry.Value is FFPFile file)
                {
                    Console.WriteLine(tabstring + "- " + entry.Key + ": " + file.Length + " bytes of data, with "
                        + file.Internal.FileLength + " bytes encoded as " + file.Internal.Encoding + ", starting in-file at " + file.Internal.StartPosition);
                }
                else
                {
                    Console.WriteLine(tabstring + "* Unknown (broken?) entry at " + entry.Key + ": " + entry.Value);
                }
            }
        }
    }
}
