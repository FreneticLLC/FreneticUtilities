using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace FreneticUtilitiesTester;

/// <summary>Helper to apply FGE-Enhancements to a data folder.</summary>
public class FGEEnhancements
{
    /// <summary>A '.png' file encoder that makes dense clean files.</summary>
    public static PngEncoder CleanPngEncoder = new()
    {
        CompressionLevel = PngCompressionLevel.BestCompression,
        ColorType = PngColorType.Palette,
        BitDepth = PngBitDepth.Bit8,
        FilterMethod = PngFilterMethod.Adaptive,
        InterlaceMethod = PngInterlaceMode.None,
        TransparentColorMode = PngTransparentColorMode.Clear,
        SkipMetadata = true
    };

    /// <summary>A '.jpg' file encoder that makes dense small thumbnails.</summary>
    public static JpegEncoder ThumbnailEncoder = new()
    {
        Quality = 70,
        ColorType = JpegEncodingColor.YCbCrRatio444,
        SkipMetadata = true,
        Interleaved = false
    };

    /// <summary>What version of the enhancements engine is currently implemented. Changing this version indicates enhancements must re-run.</summary>
    public static int EnhancementsVersion = 1;

    /// <summary>Configuration for FGE Packages.</summary>
    public class FGEPackageConfig : AutoConfiguration
    {
        /// <summary>The name of this package.</summary>
        [ConfigComment("The name of this package, eg 'My Cool Game Data'. Keep it clear, simple, and unique.")]
        public string Name = "Unknown";

        /// <summary>The author of this package.</summary>
        [ConfigComment("The name of the author of this package, eg 'Frenetic LLC'. Keep it clear, simple, and unique.")]
        public string Author = "Unknown";

        /// <summary>The version of this package.</summary>
        [ConfigComment("The version of this package, eg '1.0.0'. Use simple numeric dotted version format.")]
        public string PackageVersion = "1.0.0";

        /// <summary>The description of this package.</summary>
        [ConfigComment("The description of this package. This is an optional place to put any extra/custom text.")]
        public string Description = "...";

        /// <summary>Configuration for FGE Package enhancements.</summary>
        public class EnhancementsConfig : AutoConfiguration
        {
            /// <summary>Whether to do '.png' fixing.</summary>
            [ConfigComment("If true, run enhancements on '.png' files, including creation of thumbnails, and reduction of file size.")]
            public bool DoPngFix = true;

            /// <summary>Whether to do '.jpg' fixing.</summary>
            [ConfigComment("If true, run enhancements on '.jpg' files, including creation of thumbnails.")]
            public bool DoJpgFix = true;

            /// <summary>Whether to log in console what edits are made while running.</summary>
            [ConfigComment("Whether to log in console what edits are made while running.")]
            public bool LogEdits = true;

            /// <summary>Version of <see cref="EnhancementsVersion"/> last used.</summary>
            [ConfigComment("Internal version tracker, do not edit. You can set this to '0' to force enhancements to fully re-run.")]
            public int EnhancementsVersionRan = 0;
        }

        /// <summary>Configuration for FGE Package enhancements.</summary>
        [ConfigComment("Enhancements configurations.")]
        public EnhancementsConfig Enhancements = new();
    }

    /// <summary>Runs the full sweep of potential enhancements over a given folder.</summary>
    public static void RunNow(string folder)
    {
        string configFileName = $"{folder}/fge_package.fds";
        if (!File.Exists(configFileName))
        {
            Console.WriteLine($"\n[ERROR] Cannot apply FGE enhancements to '{folder}': no 'fge_package.fds' file found.\n");
            return;
        }
        FDSSection configData = FDSUtility.ReadFile(configFileName);
        FGEPackageConfig config = new();
        config.Load(configData);
        bool mustRerun = false;
        if (config.Enhancements.EnhancementsVersionRan < EnhancementsVersion)
        {
            mustRerun = true;
            config.Enhancements.EnhancementsVersionRan = EnhancementsVersion;
        }
        config.Save(true).SaveToFile(configFileName);
        foreach (string file in Directory.EnumerateFiles(folder, "*.thumb.jpg", SearchOption.AllDirectories).ToArray())
        {
            if (mustRerun)
            {
                File.Delete(file);
            }
        }
        if (config.Enhancements.DoPngFix)
        {
            foreach (string file in Directory.EnumerateFiles(folder, "*.png", SearchOption.AllDirectories).ToArray())
            {
                string thumbName = $"{file.BeforeLast('.')}.thumb.jpg";
                if (File.Exists(thumbName))
                {
                    continue;
                }
                if (config.Enhancements.LogEdits)
                {
                    Console.WriteLine($"Will apply PNG enhancements to '{file}'");
                }
                Image image = Image.Load(file);
                image.SaveAsPng(file, CleanPngEncoder);
                if (image.Width >= 128 && image.Height >= 128)
                {
                    image.Mutate(i =>
                    {
                        i.Resize(64, 64);
                    });
                }
                else if (image.Width >= 16 && image.Height >= 16)
                {
                    image.Mutate(i =>
                    {
                        i.Resize(8, 8);
                    });
                }
                image.SaveAsJpeg(thumbName, ThumbnailEncoder);
            }
        }
        if (config.Enhancements.DoJpgFix)
        {
            foreach (string file in Directory.EnumerateFiles(folder, "*.jpg", SearchOption.AllDirectories).ToArray())
            {
                string thumbName = $"{file.BeforeLast('.')}.thumb.jpg";
                if (File.Exists(thumbName) || file.EndsWith(".thumb.jpg"))
                {
                    continue;
                }
                if (config.Enhancements.LogEdits)
                {
                    Console.WriteLine($"Will apply JPG enhancements to '{file}'");
                }
                Image image = Image.Load(file);
                if (image.Width >= 128 && image.Height >= 128)
                {
                    image.Mutate(i =>
                    {
                        i.Resize(64, 64);
                    });
                }
                else if (image.Width >= 16 && image.Height >= 16)
                {
                    image.Mutate(i =>
                    {
                        i.Resize(8, 8);
                    });
                }
                image.SaveAsJpeg(thumbName, ThumbnailEncoder);
            }
        }
    }
}
