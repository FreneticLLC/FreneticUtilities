//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticUtilities.FreneticExtensions;

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
        public List<string> PrecedingComments = new List<string>();

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
        /// Gets the internal represented data as a string. Can stringify non-string values.
        /// </summary>
        public string AsString
        {
            get
            {
                if (Internal is string str)
                {
                    return str;
                }
                else
                {
                    return Internal.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the internal represented data as a boolean.
        /// </summary>
        public bool AsBool
        {
            get
            {
                if (Internal is bool asBool)
                {
                    return asBool;
                }
                else
                {
                    return Internal.ToString().ToLowerFast() == "true";
                }
            }
        }

        /// <summary>
        /// Gets the internal represented data as a list of data.
        /// </summary>
        public List<FDSData> AsDataList
        {
            get
            {
                if (Internal is List<FDSData> asList)
                {
                    return asList;
                }
                else
                {
                    return new List<FDSData>() { this };
                }
            }
        }

        /// <summary>
        /// Gets the internal represented data as a list of strings.
        /// </summary>
        public List<string> AsStringList
        {
            get
            {
                List<FDSData> dat = AsDataList;
                List<string> newlist = new List<string>(dat.Count);
                for (int i = 0; i < dat.Count; i++)
                {
                    newlist.Add(dat[i].Internal.ToString());
                }
                return newlist;
            }
        }

        /// <summary>
        /// Gets the internal represented data as a double-precision (64-bit) floating point value. Returns null if not a valid double.
        /// </summary>
        public double? AsDouble
        {
            get
            {
                if (Internal is double asDouble)
                {
                    return asDouble;
                }
                else if (Internal is float asFloat)
                {
                    return asFloat;
                }
                if (Internal is long asLong)
                {
                    return (double)asLong;
                }
                else if (Internal is int asInt)
                {
                    return (double)asInt;
                }
                else
                {
                    if (double.TryParse(Internal.ToString(), out double d))
                    {
                        return d;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the internal represented data as a single-precision (32-bit) floating point value. Returns null if not a valid float.
        /// </summary>
        public float? AsFloat
        {
            get
            {
                double? dValue = AsDouble;
                if (!dValue.HasValue)
                {
                    return null;
                }
                return (float)dValue.Value;
            }
        }

        /// <summary>
        /// Gets the internal represented data as a 64-bit signed integer. Returns null if not a valid integer.
        /// </summary>
        public long? AsLong
        {
            get
            {
                if (Internal is long asLong)
                {
                    return asLong;
                }
                else if (Internal is int asInt)
                {
                    return asInt;
                }
                else
                {
                    if (long.TryParse(Internal.ToString(), out long l))
                    {
                        return l;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the internal represented data as a 32-bit signed integer. Returns null if not a valid integer.
        /// </summary>
        public int? AsInt
        {
            get
            {
                long? lValue = AsLong;
                if (!lValue.HasValue)
                {
                    return null;
                }
                return (int)lValue.Value;
            }
        }

        /// <summary>
        /// Gets the internal represented data as a 64-bit unsigned integer. Returns null if not a valid integer.
        /// </summary>
        public ulong? AsULong
        {
            get
            {
                if (Internal is ulong asLong)
                {
                    return asLong;
                }
                else if (Internal is uint asInt)
                {
                    return asInt;
                }
                else
                {
                    if (ulong.TryParse(Internal.ToString(), out ulong l))
                    {
                        return l;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the internal represented data as a 32-bit unsigned integer. Returns null if not a valid integer.
        /// </summary>
        public uint? AsUInt
        {
            get
            {
                ulong? lValue = AsULong;
                if (!lValue.HasValue)
                {
                    return null;
                }
                return (uint)lValue.Value;
            }
        }

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
