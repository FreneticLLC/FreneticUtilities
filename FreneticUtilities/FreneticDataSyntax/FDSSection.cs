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

namespace FreneticUtilities.FreneticDataSyntax;

/// <summary>Represents a FreneticDataSyntax section or file.</summary>
public class FDSSection
{
    /// <summary>Constructs the FDS Section from textual data.</summary>
    /// <param name="contents">The contents of the data file.</param>
    /// <exception cref="FDSInputException">If parsing fails.</exception>
    public FDSSection(string contents)
    {
        FDSParser.Parse(contents, this);
    }

    /// <summary>Constructs the FDS section from no data, preparing it for usage as a new section.</summary>
    public FDSSection()
    {
        // Do nothing, we're init'd enough!
    }

    /// <summary>All data contained by this section.</summary>
    public Dictionary<string, FDSData> Data = new();

    /// <summary>
    /// Lowercase-stored data for this section.
    /// For lookup assistance only.
    /// </summary>
    public Dictionary<string, FDSData> DataLowered = new();

    /// <summary>Comments at the end of the section (usually only on the file root section).</summary>
    public List<string> PostComments = new();

    /// <summary>
    /// The section path splitter for this section.
    /// Will initially hold a value obtained from <see cref="FDSUtility.DefaultSectionPathSplit"/> at instance construction time.
    /// That field is initially a dot value. Altering that default or this value may cause issues (in particular with escaping) depending on the chosen value.
    /// </summary>
    public char SectionPathSplit = FDSUtility.DefaultSectionPathSplit;

    /// <summary>Returns a boolean indicating whether the section is empty.</summary>
    public bool IsEmpty()
    {
        return Data.IsEmpty();
    }

    /// <summary>Returns the set of all keys at the root of this section.</summary>
    /// <returns>All keys.</returns>
    public IEnumerable<string> GetRootKeys()
    {
        return Data.Keys;
    }

    /// <summary>
    /// Gets a list of strings from the section. Can stringify non-string values.
    /// Returns null if not found.
    /// </summary>
    /// <param name="key">The key to get data from.</param>
    /// <returns>The data found, or the default.</returns>
    public List<string> GetStringList(string key)
    {
        FDSData got = GetData(key);
        if (got == null)
        {
            return null;
        }
        return got.AsStringList;
    }

    /// <summary>
    /// Gets a list of data from the section.
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
        return got.AsDataList;
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
        return got.AsBool;
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
        return got.AsString;
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
        FDSData got = GetData(key);
        if (got == null)
        {
            return def;
        }
        return got.AsFloat ?? def;
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
        return got.AsDouble ?? def;
    }

    /// <summary>
    /// Gets an optional uint from the section.
    /// Returns def if not found.
    /// </summary>
    /// <param name="key">The key to get data from.</param>
    /// <param name="def">The default object.</param>
    /// <returns>The data found, or the default.</returns>
    public uint? GetUInt(string key, uint? def = null)
    {
        FDSData got = GetData(key);
        if (got == null)
        {
            return def;
        }
        return got.AsUInt ?? def;
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
        FDSData got = GetData(key);
        if (got == null)
        {
            return def;
        }
        return got.AsInt ?? def;
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
        return got.AsLong ?? def;
    }

    /// <summary>
    /// Gets an optional ulong from the section.
    /// Returns def if not found.
    /// </summary>
    /// <param name="key">The key to get data from.</param>
    /// <param name="def">The default object.</param>
    /// <returns>The data found, or the default.</returns>
    public ulong? GetUlong(string key, ulong? def = null)
    {
        FDSData got = GetData(key);
        if (got == null)
        {
            return def;
        }
        return got.AsULong ?? def;
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
    /// Sets data to the root of the section.
    /// May throw an FDSInputException if Set failed!
    /// </summary>
    /// <param name="key">The key to set data from.</param>
    /// <param name="input">The key to set data to.</param>
    public void SetRoot(string key, object input)
    {
        SetRootData(key, new FDSData() { Internal = FDSUtility.ProcessObject(input), PrecedingComments = new List<string>() });
    }

    /// <summary>
    /// Sets data to the section.
    /// May throw an FDSInputException if Set failed!
    /// </summary>
    /// <param name="key">The key to set data from.</param>
    /// <param name="input">The key to set data to.</param>
    public void Set(string key, object input)
    {
        SetData(key, new FDSData() { Internal = FDSUtility.ProcessObject(input), PrecedingComments = new List<string>() });
    }

    /// <summary>
    /// Sets data to the section.
    /// May throw an FDSInputException if SetData failed!
    /// </summary>
    /// <param name="key">The key to set data from.</param>
    /// <param name="data">The key to set data to.</param>
    public void SetData(string key, FDSData data)
    {
        int lind = key.LastIndexOf(SectionPathSplit);
        if (lind < 0)
        {
            SetRootData(key, data);
            return;
        }
        if (lind == key.Length - 1)
        {
            throw new FDSInputException("Invalid SetData key: Ends in a path splitter!");
        }

        FDSSection sec = GetSectionInternal(key[..lind], false, false);
        sec.SetRootData(key[(lind + 1)..], data);
    }

    /// <summary>Defaults data to the section (IE, sets it if not present).</summary>
    /// <param name="key">The key to set data from.</param>
    /// <param name="input">The key to set data to.</param>
    public void Default(string key, object input)
    {
        DefaultData(key, new FDSData() { Internal = FDSUtility.ProcessObject(input), PrecedingComments = new List<string>() });
    }

    /// <summary>Defaults data to the section (IE, sets it if not present).</summary>
    /// <param name="key">The key to set data from.</param>
    /// <param name="data">The key to set data to.</param>
    public void DefaultData(string key, FDSData data)
    {
        int lind = key.LastIndexOf(SectionPathSplit);
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
            throw new FDSInputException("Invalid SetData key: Ends in a path splitter!");
        }

        FDSSection sec = GetSectionInternal(key[..lind], false, false);
        string k = key[(lind + 1)..];
        if (sec.GetRootData(k) == null)
        {
            sec.SetRootData(k, data);
        }
    }

    /// <summary>Checks if a key exists in the FDS section.</summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether the key is present.</returns>
    public bool HasKey(string key)
    {
        return GetData(key) != null;
    }

    /// <summary>Checks if a key exists in the root of the FDS section.</summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether the key is present.</returns>
    public bool HasRootKey(string key)
    {
        return GetRootData(key) != null;
    }

    /// <summary>Checks if a case-insensitive key exists in the FDS section.</summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether the key is present.</returns>
    public bool HasKeyLowered(string key)
    {
        return GetDataLowered(key) != null;
    }

    /// <summary>Checks if a case-insensitive key exists in the root of the FDS section.</summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether the key is present.</returns>
    public bool HasRootKeyLowered(string key)
    {
        return GetRootDataLowered(key.ToLowerFast()) != null;
    }

    /// <summary>
    /// Gets data from the section.
    /// Returns null if not found.
    /// </summary>
    /// <param name="key">The key to get data from.</param>
    /// <returns>The data found, or null.</returns>
    public FDSData GetData(string key)
    {
        int lind = key.LastIndexOf(SectionPathSplit);
        if (lind < 0)
        {
            return GetRootData(key);
        }
        if (lind == key.Length - 1)
        {
            return null;
        }
        FDSSection sec = GetSection(key[..lind]);
        if (sec == null)
        {
            return null;
        }
        return sec.GetRootData(key[(lind + 1)..]);
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
        int lind = key.LastIndexOf(SectionPathSplit);
        if (lind < 0)
        {
            return GetRootDataLowered(key);
        }
        if (lind == key.Length - 1)
        {
            return null;
        }
        FDSSection sec = GetSectionInternal(key[..lind], true, true);
        if (sec == null)
        {
            return null;
        }
        return sec.GetRootDataLowered(key[(lind + 1)..]);
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

    /// <summary>Gets a sub-section of this FDS section.</summary>
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
        string[] dat = key.SplitFast(SectionPathSplit);
        FDSSection current = this;
        for (int i = 0; i < dat.Length; i++)
        {
            FDSData fdat = lowered ? current.GetRootDataLowered(dat[i]) : current.GetRootData(dat[i]);
            if (fdat != null && fdat.Internal is FDSSection asSection)
            {
                current = asSection;
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
                FDSSection temp = new();
                current.SetRootData(dat[i], new FDSData() { Internal = temp, PrecedingComments = new List<string>() });
                current = temp;
            }
        }
        return current;
    }

    /// <summary>Sets data direct on the root level.</summary>
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

    /// <summary>Removes data direct from the root level.</summary>
    /// <param name="key">The key to remove.</param>
    public void RemoveRoot(string key)
    {
        Data.Remove(key);
        DataLowered.Remove(key.ToLowerFast());
    }

    /// <summary>Removes data from the section.</summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(string key)
    {
        int lind = key.LastIndexOf(SectionPathSplit);
        if (lind < 0)
        {
            RemoveRoot(key);
            return;
        }
        if (lind == key.Length - 1)
        {
            return;
        }
        FDSSection sec = GetSection(key[..lind]);
        if (sec == null)
        {
            return;
        }
        sec.RemoveRoot(key[(lind + 1)..]);
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

    /// <summary>Helper for <see cref="SaveToString(int, string, bool)"/></summary>
    private void AppendListToString(StringBuilder output, List<FDSData> list, int tabulation, string newline, bool skipFirstTabs)
    {
        string tabs = new('\t', tabulation);
        foreach (FDSData cdat in list)
        {
            foreach (string com in cdat.PrecedingComments)
            {
                output.Append(tabs).Append('#').Append(com).Append(newline);
            }
            if (!skipFirstTabs)
            {
                output.Append(tabs);
            }
            skipFirstTabs = false;
            if (cdat.Internal is byte[])
            {
                output.Append("= ");
                output.Append(cdat.Outputable()).Append(newline);
            }
            else if (cdat.Internal is List<FDSData> subList)
            {
                output.Append(">\t");
                if (subList.IsEmpty())
                {
                    output.Append('\n');
                }
                AppendListToString(output, subList, tabulation + 1, newline, true);
            }
            else if (cdat.Internal is FDSSection subSection)
            {
                output.Append(">\t");
                if (subSection.IsEmpty())
                {
                    output.Append('\n');
                }
                output.Append(subSection.SaveToString(tabulation + 1, newline, true));
            }
            else
            {
                output.Append("- ");
                output.Append(cdat.Outputable()).Append(newline);
            }
        }
    }

    /// <summary>Converts this <see cref="FDSSection"/> to a textual representation of itself.</summary>
    public string SaveToString()
    {
        return SaveToString(0, "\n", false);
    }

    /// <summary>Converts this <see cref="FDSSection"/> to a textual representation of itself.</summary>
    /// <param name="tabulation">How many tabs to start with. Generally 0.</param>
    /// <param name="newline">What string to use as a new line. Generally \n.</param>
    /// <param name="skipFirstTabs">Whether to skip the first piece of tabulation. Generally false.</param>
    public string SaveToString(int tabulation, string newline, bool skipFirstTabs)
    {
        string tabs = new('\t', tabulation);
        StringBuilder outputBuilder = new(Data.Count * 100);
        foreach (KeyValuePair<string, FDSData> entry in Data)
        {
            FDSData dat = entry.Value;
            foreach (string str in dat.PrecedingComments)
            {
                outputBuilder.Append(tabs).Append('#').Append(str).Append(newline);
            }
            if (!skipFirstTabs)
            {
                outputBuilder.Append(tabs);
            }
            skipFirstTabs = false;
            outputBuilder.Append(FDSUtility.EscapeKey(entry.Key));
            if (dat.Internal is FDSSection subSection)
            {
                outputBuilder.Append(':').Append(newline).Append(subSection.SaveToString(tabulation + 1, newline, false));
            }
            else if (dat.Internal is byte[])
            {
                outputBuilder.Append("= ").Append(dat.Outputable()).Append(newline);
            }
            else if (dat.Internal is List<FDSData> list)
            {
                outputBuilder.Append(':').Append(newline);
                AppendListToString(outputBuilder, list, tabulation, newline, false);
            }
            else
            {
                outputBuilder.Append(": ").Append(dat.Outputable()).Append(newline);
            }
        }
        foreach (string str in PostComments)
        {
            outputBuilder.Append('#').Append(str).Append(newline);
        }
        return outputBuilder.ToString();
    }

    /// <summary>Implements <see cref="object.ToString()"/> as a redirect to <see cref="SaveToString()"/>.</summary>
    public override string ToString()
    {
        return SaveToString();
    }

    /// <summary>Returns a simple C# object representing this datapiece. Lists become "List&lt;object&gt;".</summary>
    public Dictionary<string, object> ToSimple()
    {
        Dictionary<string, object> toRet = new(Data.Count * 2);
        foreach ((string key, FDSData data) in Data)
        {
            toRet[key] = data.ToSimple();
        }
        return toRet;
    }

    /// <summary>Converts a simple C# object (as output by <see cref="ToSimple"/> to an <see cref="FDSSection"/>.</summary>
    public static FDSSection FromSimple(Dictionary<string, object> simple)
    {
        FDSSection toRet = new();
        foreach ((string key, object value) in simple)
        {
            toRet.SetRoot(key, value);
        }
        return toRet;
    }
}
