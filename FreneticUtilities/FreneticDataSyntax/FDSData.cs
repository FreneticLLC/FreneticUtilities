//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2018 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
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
        /// The internal represented data.
        /// </summary>
        public object Internal;

        /// <summary>
        /// Returns the output-able string representation of this data.
        /// </summary>
        /// <returns>The resultant data.</returns>
        public string Outputable()
        {
            if (Internal is List<FDSData>)
            {
                StringBuilder sb = new StringBuilder();
                foreach (FDSData dat in (List<FDSData>)Internal)
                {
                    sb.Append(dat.Outputable()).Append("|");
                }
                return sb.ToString();
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
