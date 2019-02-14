//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>
    /// Represents a piece of data within an FDS Section.
    /// </summary>
    public class FDSData
    {
        /// <summary>
        /// The list of comments preceding this data piece.
        /// </summary>
        public List<string> PrecedingComments;

        /// <summary>
        /// Adds a preceding comment to this data piece.
        /// </summary>
        /// <param name="comment">The comment to add.</param>
        public void AddComment(string comment)
        {
            comment = comment.Replace("\r", "");
            PrecedingComments.AddRange(comment.Split('\n').Select(str => str.TrimEnd()));
        }

        /// <summary>
        /// The internal represented data.
        /// </summary>
        public object Internal;

        /// <summary>
        /// Returns the output-able string representation of this data.
        /// </summary>
        /// <returns>The resultant data.</returns>
        public string Outputable()
        {
            if (Internal is List<FDSData> list)
            {
                StringBuilder outputBuilder = new StringBuilder();
                foreach (FDSData dat in list)
                {
                    outputBuilder.Append(dat.Outputable()).Append('|');
                }
                return outputBuilder.ToString();
            }
            if (Internal is byte[])
            {
                return Convert.ToBase64String((byte[])Internal, Base64FormattingOptions.None);
            }
            if (Internal is bool)
            {
                return ((bool)Internal) ? "true" : "false";
            }
            return FDSUtility.Escape(Internal.ToString());
        }
    }
}
