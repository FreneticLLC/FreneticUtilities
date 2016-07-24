using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticDataSyntax
{
    /// <summary>
    /// Represents a FreneticDataSyntax section or file.
    /// </summary>
    public class FDSSection
    {
        /// <summary>
        /// Constructs the FDS Section from textual data.
        /// </summary>
        /// <param name="contents">The contents of the data file.</param>
        public FDSSection(string contents)
        {
            StartingLine = 1;
            contents = FDSUtility.CleanFileData(contents);
            string[] data = contents.SplitFast('\n');
            for (int i = 0; i < data.Length; i++)
            {

            }
        }

        /// <summary>
        /// Constructs the FDS section from no data, preparing it for usage as a new section.
        /// </summary>
        public FDSSection()
        {

        }

        /// <summary>
        /// The line number this section starts on.
        /// Note that files start at 1.
        /// Only accurate at file-load time.
        /// </summary>
        public int StartingLine = 0;

        /// <summary>
        /// All data contained by this section.
        /// </summary>
        public Dictionary<string, FDSData> Data = new Dictionary<string, FDSData>();

        /// <summary>
        /// Lowercase-stored data for this section.
        /// </summary>
        public Dictionary<string, FDSData> DataLowered = new Dictionary<string, FDSData>();
    }
}
