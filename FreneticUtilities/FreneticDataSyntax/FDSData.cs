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
    public class FDSData : IEquatable<FDSData>
    {
        /// <summary>
        /// Constructs an empty <see cref="FDSData"/> instance.
        /// </summary>
        public FDSData()
        {
        }

        /// <summary>
        /// Constructs an <see cref="FDSData"/> instance of the specified object.
        /// </summary>
        /// <param name="_internal">The object.</param>
        public FDSData(object _internal)
        {
            Internal = _internal;
        }

        /// <summary>
        /// Constructs an <see cref="FDSData"/> instance of the specified object and the specified comments.
        /// </summary>
        /// <param name="_internal">The object.</param>
        /// <param name="comment">The comments to apply (newline separated).</param>
        public FDSData(object _internal, string comment)
        {
            Internal = _internal;
            AddComment(comment);
        }

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
                else if (Internal is FDSSection section && section.IsEmpty())
                {
                    return new List<FDSData>();
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
        /// Gets the internal represented data as a C# decimal value. Returns null if not a valid decimal.
        /// </summary>
        public decimal? AsDecimal
        {
            get
            {
                if (Internal is decimal asDecimal)
                {
                    return asDecimal;
                }
                else if (Internal is double asDouble)
                {
                    return (decimal)asDouble;
                }
                else if (Internal is float asFloat)
                {
                    return (decimal)asFloat;
                }
                if (Internal is long asLong)
                {
                    return (decimal)asLong;
                }
                else if (Internal is int asInt)
                {
                    return (decimal)asInt;
                }
                else if (decimal.TryParse(Internal.ToString(), out decimal d))
                {
                    return d;
                }
                return null;
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
                else if (double.TryParse(Internal.ToString(), out double d))
                {
                    return d;
                }
                return null;
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
                else if (long.TryParse(Internal.ToString(), out long l))
                {
                    return l;
                }
                return null;
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
        /// Gets the internal represented data as a 16-bit signed integer. Returns null if not a valid integer.
        /// </summary>
        public short? AsShort
        {
            get
            {
                long? lValue = AsLong;
                if (!lValue.HasValue)
                {
                    return null;
                }
                return (short)lValue.Value;
            }
        }

        /// <summary>
        /// Gets the internal represented data as an 8-bit signed integer (sbyte). Returns null if not a valid integer.
        /// </summary>
        public sbyte? AsSByte
        {
            get
            {
                long? lValue = AsLong;
                if (!lValue.HasValue)
                {
                    return null;
                }
                return (sbyte)lValue.Value;
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
        /// Gets the internal represented data as a 16-bit unsigned integer. Returns null if not a valid integer.
        /// </summary>
        public ushort? AsUShort
        {
            get
            {
                ulong? lValue = AsULong;
                if (!lValue.HasValue)
                {
                    return null;
                }
                return (ushort)lValue.Value;
            }
        }

        /// <summary>
        /// Gets the internal represented data as an 8-bit unsigned integer (byte). Returns null if not a valid integer.
        /// </summary>
        public byte? AsByte
        {
            get
            {
                ulong? lValue = AsULong;
                if (!lValue.HasValue)
                {
                    return null;
                }
                return (byte)lValue.Value;
            }
        }

        /// <summary>
        /// Gets the internal represented data as an array of bytes. Returns null if not a valid array of bytes.
        /// </summary>
        public byte[] AsByteArray
        {
            get
            {
                return Internal as byte[];
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
            else if (Internal is byte[] output)
            {
                return Convert.ToBase64String(output, Base64FormattingOptions.None);
            }
            else if (Internal is bool bValue)
            {
                return bValue ? "true" : "false";
            }
            return FDSUtility.Escape(Internal.ToString());
        }

        /// <summary>Implements <see cref="Object.ToString"/> by redirecting to <see cref="Outputable"/></summary>
        public override string ToString()
        {
            return Outputable();
        }

        /// <summary>Implements <see cref="Object.GetHashCode"/> by redirecting to <see cref="Internal"/></summary>
        public override int GetHashCode()
        {
            return Internal.GetHashCode();
        }

        /// <summary>Implements <see cref="Object.Equals(object)"/> by redirecting to <see cref="Internal"/></summary>
        public override bool Equals(object obj)
        {
            if (!(obj is FDSData data))
            {
                return false;
            }
            return Equals(data);
        }

        /// <summary>Implements <see cref="IEquatable{FDSData}.Equals(FDSData)"/> by redirecting to <see cref="Internal"/></summary>
        public bool Equals(FDSData other)
        {
            return Internal.Equals(other.Internal);
        }
    }
}
