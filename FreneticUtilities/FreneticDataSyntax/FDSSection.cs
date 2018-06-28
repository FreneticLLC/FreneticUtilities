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
using FreneticUtilities.FreneticExtensions;

namespace FreneticUtilities.FreneticDataSyntax
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
            Dictionary<int, FDSSection> spacedsections = new Dictionary<int, FDSSection>() { { 0, this } };
            List<string> ccomments = new List<string>();
            List<string> seccomments = new List<string>();
            FDSSection csection = this;
            string[] data = contents.SplitFast('\n');
            int pspaces = 0;
            string secwaiting = null;
            List<FDSData> clist = null;
            for (int i = 0; i < data.Length; i++)
            {
                string line = data[i];
                int spaces;
                for (spaces = 0; spaces < line.Length; spaces++)
                {
                    if (line[spaces] != ' ')
                    {
                        break;
                    }
                }
                if (spaces == line.Length)
                {
                    continue;
                }
                string datum = line.Substring(spaces).TrimEnd(' ');
                if (datum.StartsWith("#"))
                {
                    ccomments.Add(datum.Substring(1));
                    continue;
                }
                if (spaces < pspaces)
                {
                    if (spacedsections.TryGetValue(spaces, out FDSSection temp))
                    {
                        csection = temp;
                        foreach (int test in new List<int>(spacedsections.Keys))
                        {
                            if (test > spaces)
                            {
                                spacedsections.Remove(test);
                            }
                        }
                    }
                    else
                    {
                        Exception(i, line, "Spaced incorrectly. Spacing length is less than previous spacing length,"
                            + "but does not match the spacing value of any known section, valid: "
                            + string.Join(" / ", spacedsections.Keys) + ", found: " + spaces + ", was: " + pspaces);
                    }
                }
                if (datum[0] == '-')
                {
                    string clistline = datum.Substring(1).TrimStart(' ');
                    if (clist == null)
                    {
                        if (spaces >= pspaces && secwaiting != null)
                        {
                            clist = new List<FDSData>();
                            csection.SetRootData(FDSUtility.UnEscapeKey(secwaiting), new FDSData() { PrecedingComments = new List<string>(seccomments), Internal = clist });
                            seccomments.Clear();
                            secwaiting = null;
                        }
                        else
                        {
                            Exception(i, line, "Line purpose unknown, attempted list entry when not building a list");
                        }
                    }
                    clist.Add(new FDSData() { PrecedingComments = new List<string>(ccomments), Internal = FDSUtility.InterpretType(FDSUtility.UnEscape(clistline)) });
                    ccomments.Clear();
                    continue;
                }
                clist = null;
                string startofline = "";
                string endofline = "";
                char type = '\0';
                for (int spot = 0; spot < datum.Length; spot++)
                {
                    if (datum[spot] == ':' || datum[spot] == '=')
                    {
                        type = datum[spot];
                        startofline = datum.Substring(0, spot);
                        endofline = spot == datum.Length - 1 ? "": datum.Substring(spot + 1);
                        break;
                    }
                }
                endofline = endofline.TrimStart(' ');
                if (type == '\0')
                {
                    Exception(i, line, "Line purpose unknown");
                }
                if (startofline.Length == 0)
                {
                    Exception(i, line, "Empty key label!");
                }
                if (spaces > pspaces && secwaiting != null)
                {
                    FDSSection sect = new FDSSection();
                    csection.SetRootData(FDSUtility.UnEscapeKey(secwaiting), new FDSData() { PrecedingComments = new List<string>(seccomments), Internal = sect });
                    seccomments.Clear();
                    csection = sect;
                    spacedsections[spaces] = sect;
                    secwaiting = null;
                }
                if (type == '=')
                {
                    if (endofline.Length == 0)
                    {
                        csection.SetRootData(FDSUtility.UnEscapeKey(startofline), new FDSData() { PrecedingComments = new List<string>(ccomments), Internal = new byte[0] });
                    }
                    else
                    {
                        csection.SetRootData(FDSUtility.UnEscapeKey(startofline), new FDSData() { PrecedingComments = new List<string>(ccomments), Internal = Convert.FromBase64String(endofline) });
                    }
                    ccomments.Clear();
                }
                else if (type == ':')
                {
                    if (endofline.Length == 0)
                    {
                        secwaiting = startofline;
                        seccomments = new List<string>(ccomments);
                        ccomments.Clear();
                    }
                    else
                    {
                        csection.SetRootData(FDSUtility.UnEscapeKey(startofline), new FDSData() { PrecedingComments = new List<string>(ccomments), Internal = FDSUtility.InterpretType(FDSUtility.UnEscape(endofline)) });
                        ccomments.Clear();
                    }
                }
                else
                {
                    Exception(i, line, "Internal issue: unrecognize 'type' value: " + type);
                }
                pspaces = spaces;
            }
        }

        private void Exception(int linenumber, string line, string reason)
        {
            throw new Exception("[FDS Parsing error] Line " + (linenumber + 1) + ": " + reason + ", from line as follows: `" + line + "`");
        }

        /// <summary>
        /// Constructs the FDS section from no data, preparing it for usage as a new section.
        /// </summary>
        public FDSSection()
        {
            // Do nothing, we're init'd enough!
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

        /// <summary>
        /// Gets a string from the section. Can stringify non-string values.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <returns>The data found, or the default.</returns>
        public List<string> GetStringList(string key)
        {
            List<FDSData> dat = GetDataList(key);
            if (dat == null)
            {
                return null;
            }
            List<string> newlist = new List<string>(dat.Count);
            for (int i = 0; i < dat.Count; i++)
            {
                newlist.Add(dat[i].Internal.ToString());
            }
            return newlist;
        }

        /// <summary>
        /// Gets a string from the section. Can stringify non-string values.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <returns>The data found, or the default.</returns>
        public List<FDSData> GetDataList(string key)
        {
            FDSData got = GetData(key);
            if (got == null)
            {
                return null;
            }
            object o = got.Internal;
            if (o is List<FDSData>)
            {
                return (List<FDSData>)o;
            }
            else
            {
                return new List<FDSData>() { got };
            }
        }

        /// <summary>
        /// Gets a bool from the section.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public bool? GetBool(string key, bool? def = null)
        {
            FDSData got = GetData(key);
            if (got == null)
            {
                return def;
            }
            object o = got.Internal;
            if (o is bool)
            {
                return (bool)o;
            }
            else
            {
                return o.ToString().ToLowerFast() == "true";
            }
        }

        /// <summary>
        /// Gets a string from the section. Can stringify non-string values.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public string GetString(string key, string def = null)
        {
            FDSData got = GetData(key);
            if (got == null)
            {
                return def;
            }
            object o = got.Internal;
            if (o is string)
            {
                return (string)o;
            }
            else
            {
                return o.ToString();
            }
        }

        /// <summary>
        /// Gets an optional float from the section.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public float? GetFloat(string key, float? def = null)
        {
            return (float?)GetDouble(key, def);
        }

        /// <summary>
        /// Gets an optional double from the section.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public double? GetDouble(string key, double? def = null)
        {
            FDSData got = GetData(key);
            if (got == null)
            {
                return def;
            }
            object o = got.Internal;
            if (o is double)
            {
                return (double)o;
            }
            else if (o is float)
            {
                return (float)o;
            }
            else
            {
                if (double.TryParse(o.ToString(), out double d))
                {
                    return d;
                }
                return def;
            }
        }

        /// <summary>
        /// Gets an optional int from the section.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public int? GetInt(string key, int? def = null)
        {
            return (int?)GetLong(key, def);
        }

        /// <summary>
        /// Gets an optional long from the section.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public long? GetLong(string key, long? def = null)
        {
            FDSData got = GetData(key);
            if (got == null)
            {
                return def;
            }
            object o = got.Internal;
            if (o is long)
            {
                return (long)o;
            }
            else if (o is int)
            {
                return (int)o;
            }
            else
            {
                if (long.TryParse(o.ToString(), out long l))
                {
                    return l;
                }
                return def;
            }
        }

        /// <summary>
        /// Gets an object from the section.
        /// Returns def if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <param name="def">The default object.</param>
        /// <returns>The data found, or the default.</returns>
        public object GetObject(string key, object def = null)
        {
            FDSData got = GetData(key);
            if (got == null)
            {
                return def;
            }
            return got.Internal;
        }

        /// <summary>
        /// Sets data to the section.
        /// May throw an FDSInputException if Set failed!
        /// </summary>
        /// <param name="key">The key to set data from.</param>
        /// <param name="input">The key to set data to.</param>
        public void Set(string key, object input)
        {
            SetData(key, new FDSData() { Internal = input, PrecedingComments = new List<string>() });
        }

        /// <summary>
        /// Sets data to the section.
        /// May throw an FDSInputException if SetData failed!
        /// </summary>
        /// <param name="key">The key to set data from.</param>
        /// <param name="data">The key to set data to.</param>
        public void SetData(string key, FDSData data)
        {
            int lind = key.LastIndexOf('.');
            if (lind < 0)
            {
                SetRootData(key, data);
                return;
            }
            if (lind == key.Length - 1)
            {
                throw new FDSInputException("Invalid SetData key: Ends in a dot!");
            }

            FDSSection sec = GetSectionInternal(key.Substring(0, lind), false, false);
            sec.SetRootData(key.Substring(lind + 1), data);
        }

        /// <summary>
        /// Defaults data to the section (IE, sets it if not present!)
        /// </summary>
        /// <param name="key">The key to set data from.</param>
        /// <param name="input">The key to set data to.</param>
        public void Default(string key, object input)
        {
            DefaultData(key, new FDSData() { Internal = input, PrecedingComments = new List<string>() });
        }

        /// <summary>
        /// Defaults data to the section (IE, sets it if not present!)
        /// </summary>
        /// <param name="key">The key to set data from.</param>
        /// <param name="data">The key to set data to.</param>
        public void DefaultData(string key, FDSData data)
        {
            int lind = key.LastIndexOf('.');
            if (lind < 0)
            {
                if (GetRootData(key) == null)
                {
                    SetRootData(key, data);
                }
                return;
            }
            if (lind == key.Length - 1)
            {
                throw new FDSInputException("Invalid SetData key: Ends in a dot!");
            }

            FDSSection sec = GetSectionInternal(key.Substring(0, lind), false, false);
            string k = key.Substring(lind + 1);
            if (sec.GetRootData(k) == null)
            {
                sec.SetRootData(k, data);
            }
        }

        /// <summary>
        /// Checks if a key exists in the FDS section.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>Whether the key is present.</returns>
        public bool HasKey(string key)
        {
            return GetData(key) != null;
        }

        /// <summary>
        /// Gets data from the section.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <returns>The data found, or null.</returns>
        public FDSData GetData(string key)
        {
            int lind = key.LastIndexOf('.');
            if (lind < 0)
            {
                return GetRootData(key);
            }
            if (lind == key.Length - 1)
            {
                return null;
            }
            FDSSection sec = GetSection(key.Substring(0, lind));
            if (sec == null)
            {
                return null;
            }
            return sec.GetRootData(key.Substring(lind + 1));
        }

        /// <summary>
        /// Gets data from the section.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <returns>The data found, or null.</returns>
        public FDSData GetDataLowered(string key)
        {
            key = key.ToLowerFast();
            int lind = key.LastIndexOf('.');
            if (lind < 0)
            {
                return GetRootDataLowered(key);
            }
            if (lind == key.Length - 1)
            {
                return null;
            }
            FDSSection sec = GetSectionInternal(key.Substring(0, lind), true, true);
            if (sec == null)
            {
                return null;
            }
            return sec.GetRootDataLowered(key.Substring(lind + 1));
        }

        /// <summary>
        /// Gets a sub-section of this FDS section.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key of the section.</param>
        /// <returns>The subsection.</returns>
        public FDSSection GetSection(string key)
        {
            return GetSectionInternal(key, true, false);
        }

        /// <summary>
        /// Gets a sub-section of this FDS section.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key of the section.</param>
        /// <returns>The subsection.</returns>
        public FDSSection GetSectionLowered(string key)
        {
            return GetSectionInternal(key.ToLowerFast(), true, true);
        }

        /// <summary>
        /// Gets a sub-section of this FDS section.
        /// </summary>
        /// <param name="key">The key of the section.</param>
        /// <param name="allowNull">Whether to allow null returns, otherwise enforce the section's existence. If true, can throw an FDSInputException!</param>
        /// <param name="lowered">Whether to read lowercase section names. If set, expects lowercased input key!</param>
        /// <returns>The subsection.</returns>
        private FDSSection GetSectionInternal(string key, bool allowNull, bool lowered)
        {
            if (key == null || key.Length == 0)
            {
                return this;
            }
            string[] dat = key.SplitFast('.');
            FDSSection current = this;
            for (int i = 0; i < dat.Length; i++)
            {
                FDSData fdat = lowered ? current.GetRootDataLowered(dat[i]) : current.GetRootData(dat[i]);
                if (fdat != null && fdat.Internal is FDSSection)
                {
                    current = (FDSSection)fdat.Internal;
                }
                else
                {
                    if (allowNull)
                    {
                        return null;
                    }
                    if (fdat != null)
                    {
                        throw new FDSInputException("Key contains non-section contents!");
                    }
                    FDSSection temp = new FDSSection();
                    current.SetRootData(dat[i], new FDSData() { Internal = temp, PrecedingComments = new List<string>() });
                    current = temp;
                }
            }
            return current;
        }

        /// <summary>
        /// Sets data direct on the root level.
        /// </summary>
        /// <param name="key">The key to set data to.</param>
        /// <param name="data">The data to read.</param>
        public void SetRootData(string key, FDSData data)
        {
            Data[key] = data;
            DataLowered[key.ToLowerFast()] = data;
        }

        /// <summary>
        /// Gets data direct from the root level.
        /// Returns null if not found.
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <returns>The data found, or null.</returns>
        public FDSData GetRootData(string key)
        {
            if (Data.TryGetValue(key, out FDSData temp))
            {
                return temp;
            }
            return null;
        }

        /// <summary>
        /// Gets data direct from the root level.
        /// Returns null if not found.
        /// Assumes input is already lowercase!
        /// </summary>
        /// <param name="key">The key to get data from.</param>
        /// <returns>The data found, or null.</returns>
        public FDSData GetRootDataLowered(string key)
        {
            if (DataLowered.TryGetValue(key, out FDSData temp))
            {
                return temp;
            }
            return null;
        }

        /// <summary>
        /// Converts this FDSSection to a textual representation of itself.
        /// </summary>
        /// <param name="tabulation">How many tabs to start with. Generally do not set this.</param>
        /// <param name="newline">What string to use as a new line. Generally do not set this.</param>
        /// <returns>The string.</returns>
        public string SaveToString(int tabulation = 0, string newline = null)
        {
            if (newline == null)
            {
                newline = "\n";
            }
            StringBuilder sb = new StringBuilder();
            foreach (string key in Data.Keys)
            {
                FDSData dat = Data[key];
                foreach (string str in dat.PrecedingComments)
                {
                    sb.Append('\t', tabulation);
                    sb.Append("#").Append(str).Append(newline);
                }
                sb.Append('\t', tabulation);
                sb.Append(FDSUtility.EscapeKey(key));
                if (dat.Internal is FDSSection)
                {
                    sb.Append(":").Append(newline).Append(((FDSSection)dat.Internal).SaveToString(tabulation + 1, newline));
                }
                else if (dat.Internal is byte[])
                {
                    sb.Append("= ").Append(dat.Outputable()).Append(newline);
                }
                else if (dat.Internal is List<FDSData> datums)
                {
                    sb.Append(":").Append(newline);
                    foreach (FDSData cdat in datums)
                    {
                        foreach (string com in cdat.PrecedingComments)
                        {
                            sb.Append('\t', tabulation);
                            sb.Append("#").Append(com).Append(newline);
                        }
                        sb.Append('\t', tabulation);
                        sb.Append("- ").Append(FDSUtility.Escape(cdat.Outputable())).Append(newline);
                    }
                }
                else
                {
                    sb.Append(": ").Append(FDSUtility.Escape(dat.Outputable())).Append(newline);
                }
            }
            return sb.ToString();
        }
    }
}
