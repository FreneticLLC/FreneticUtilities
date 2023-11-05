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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticDataSyntax;

/// <summary>
/// Helper class for parsing out FDS data.
/// <para>Generally only for internal use. Use <see cref="FDSSection"/> for most external access.</para>
/// </summary>
public static class FDSParser
{
    /// <summary>Matcher for list prefix symbols (dash, equals, greater than).</summary>
    public static AsciiMatcher ListPrefixMatcher = new("-=>");

    /// <summary>Matcher for symbols that separate a key from a value.</summary>
    public static AsciiMatcher KeySeparatorMatcher = new(":=");

    /// <summary>Helper for <see cref="ParseSection(string[], int, int, int, out int, FDSSection)"/> to parse a list.</summary>
    public static void ParseList(string[] allLines, int startLine, int skip, int spacing, out int endLine, List<FDSData> outList)
    {
        List<string> currentComments = new();
        endLine = startLine;
        for (int lineNum = startLine + skip; lineNum < allLines.Length; lineNum++)
        {
            string fullLine = allLines[lineNum];
            string trimmedStart = fullLine.TrimStart(' ');
            if (trimmedStart.Length == 0)
            {
                continue;
            }
            int spaces = fullLine.Length - trimmedStart.Length;
            string trimmedLine = trimmedStart.TrimEnd(' ');
            char firstSymbol = trimmedLine[0];
            if (firstSymbol == '#')
            {
                currentComments.Add(trimmedLine[1..]);
                continue;
            }
            if (spaces < spacing)
            {
                return;
            }
            if (spaces > spacing)
            {
                throw Exception(lineNum, fullLine, $"Spacing grew for no reason (expected {spacing} but got {spaces}) inside a list - possibly forgot a key, or mixed up tabs?");
            }
            if (ListPrefixMatcher.IsMatch(firstSymbol))
            {
                string valueText = trimmedLine[1..].TrimStart();
                FDSData valueData = ParseSubListValue(firstSymbol, valueText, allLines, lineNum, spacing + (trimmedLine.Length - valueText.Length), out lineNum);
                valueData.PrecedingComments.AddRange(currentComments);
                currentComments.Clear();
                outList.Add(valueData);
                endLine = lineNum;
                continue;
            }
            return;
        }
    }

    /// <summary>Helper for <see cref="ParseSection(string[], int, int, int, out int, FDSSection)"/> to parse a value within a list.</summary>
    public static FDSData ParseSubListValue(char prefix, string valueText, string[] allLines, int startLine, int spacing, out int endLine)
    {
        endLine = startLine;
        if (prefix == '-' || prefix == '=')
        {
            return InterpretBasicObject(prefix, valueText, allLines, startLine);
        }
        else if (prefix == '>')
        {
            if (valueText.Length == 0)
            {
                throw Exception(startLine, allLines[startLine], $"Complex-list invalid: must specify a value after the '>' symbol.");
            }
            char subSymbol = valueText[0];
            if (ListPrefixMatcher.IsMatch(subSymbol))
            {
                string lineText = valueText[1..].TrimStart(' ');
                List<FDSData> outList = new()
                {
                    ParseSubListValue(subSymbol, lineText, allLines, startLine, spacing, out endLine)
                };
                ParseList(allLines, endLine, 1, spacing, out endLine, outList);
                return new FDSData(outList);
            }
            int keySeparatorIndex = KeySeparatorMatcher.FirstMatchingIndex(valueText);
            if (keySeparatorIndex == -1)
            {
                throw Exception(startLine, allLines[startLine], "Content inside complex list has unknown purpose - are you missing a symbol after the '>'?");
            }
            string key = valueText[..keySeparatorIndex];
            FDSData valueData;
            FDSSection outSection = new();
            if (keySeparatorIndex == valueText.Length - 1)
            {
                if (valueText[keySeparatorIndex] == '=')
                {
                    throw Exception(startLine, allLines[startLine], "Cannot create a binary subsection - use ':', or put the binary data on the same line.");
                }
                valueData = ParseSubSection(allLines, startLine, 1, spacing, out endLine);
            }
            else
            {
                string keyedValueText = FDSUtility.UnEscape(valueText[(keySeparatorIndex + 1)..].TrimStart(' '));
                valueData = InterpretBasicObject(valueText[keySeparatorIndex], keyedValueText, allLines, startLine);
            }
            outSection.SetRootData(key, valueData);
            ParseSection(allLines, endLine, 1, spacing, out endLine, outSection);
            return new FDSData(outSection);
        }
        else
        {
            throw Exception(startLine, allLines[startLine], "Line inside list has unknown purpose - are you missing a symbol?");
        }
    }

    /// <summary>Helper for <see cref="ParseSection(string[], int, int, int, out int, FDSSection)"/> to intepret an object as text or binary.</summary>
    public static FDSData InterpretBasicObject(char prefix, string valueText, string[] allLines, int lineNum)
    {
        if (prefix == '=')
        {
            try
            {
                return new FDSData(FDSUtility.FromBase64(valueText));
            }
            catch (FormatException ex)
            {
                throw Exception(lineNum, allLines[lineNum], $"Binary data ({valueText}) invalid - got FormatException {ex.Message}");
            }
        }
        else
        {
            return new FDSData(FDSUtility.InterpretType(valueText));
        }
    }

    /// <summary>Helper for <see cref="ParseSection(string[], int, int, int, out int, FDSSection)"/> to parse a single sub-section.</summary>
    public static FDSData ParseSubSection(string[] allLines, int startLine, int skip, int spacing, out int endLine)
    {
        endLine = startLine;
        for (int lineNum = startLine + skip; lineNum < allLines.Length; lineNum++)
        {
            string fullLine = allLines[lineNum];
            string trimmedStart = fullLine.TrimStart(' ');
            if (trimmedStart.Length == 0)
            {
                continue;
            }
            int spaces = fullLine.Length - trimmedStart.Length;
            string trimmedLine = trimmedStart.TrimEnd(' ');
            char firstSymbol = trimmedLine[0];
            if (firstSymbol == '#')
            {
                continue;
            }
            if (spaces < spacing)
            {
                break;
            }
            if (spaces > spacing)
            {
                FDSSection subSection = new();
                ParseSection(allLines, startLine, skip, spaces, out endLine, subSection);
                return new FDSData(subSection);
            }
            if (ListPrefixMatcher.IsMatch(firstSymbol))
            {
                List<FDSData> subList = new();
                ParseList(allLines, startLine + skip, 0, spacing, out endLine, subList);
                return new FDSData(subList);
            }
            break;
        }
        return new FDSData(new FDSSection());
    }

    /// <summary>Internal path to parse a single base section of an FDS file.</summary>
    /// <param name="allLines">The array of all lines of text.</param>
    /// <param name="startLine">The line index to start parsing at.</param>
    /// <param name="skip">The additional number of lines to skip (to ensure endLine doesn't get misset).</param>
    /// <param name="spacing">The minimum valid spacing.</param>
    /// <param name="endLine">Output: the line number of the section's end.</param>
    /// <param name="section">The section to store into.</param>
    public static void ParseSection(string[] allLines, int startLine, int skip, int spacing, out int endLine, FDSSection section)
    {
        List<string> currentComments = new();
        endLine = startLine;
        for (int lineNum = startLine + skip; lineNum < allLines.Length; lineNum++)
        {
            string fullLine = allLines[lineNum];
            string trimmedStart = fullLine.TrimStart(' ');
            if (trimmedStart.Length == 0)
            {
                continue;
            }
            int spaces = fullLine.Length - trimmedStart.Length;
            string trimmedLine = trimmedStart.TrimEnd(' ');
            char firstSymbol = trimmedLine[0];
            if (firstSymbol == '#')
            {
                currentComments.Add(trimmedLine[1..]);
                continue;
            }
            if (spaces < spacing)
            {
                return;
            }
            if (ListPrefixMatcher.IsMatch(firstSymbol))
            {
                throw Exception(lineNum, fullLine, "Tried to build a list without a key - likely forgot to specify a key?");
            }
            if (spaces > spacing)
            {
                throw Exception(lineNum, fullLine, $"Spacing grew for no reason (expected {spacing} but got {spaces}) - possibly forgot a key, or mixed up tabs?");
            }
            int keySeparatorIndex = KeySeparatorMatcher.FirstMatchingIndex(trimmedLine);
            if (keySeparatorIndex == -1)
            {
                throw Exception(lineNum, fullLine, "Line inside general section has unknown purpose - are you missing a symbol?");
            }
            string key = trimmedLine[..keySeparatorIndex];
            if (key.Length == 0)
            {
                throw Exception(lineNum, fullLine, "Empty key label - use '\\x' to create an intentionally empty key.");
            }
            key = FDSUtility.UnEscapeKey(key);
            FDSData valueData;
            if (keySeparatorIndex == trimmedLine.Length - 1)
            {
                if (trimmedLine[keySeparatorIndex] == '=')
                {
                    throw Exception(lineNum, fullLine, "Cannot create a binary subsection - use ':', or put the binary data on the same line.");
                }
                valueData = ParseSubSection(allLines, lineNum, 1, spacing, out lineNum);
            }
            else
            {
                string valueText = FDSUtility.UnEscape(trimmedLine[(keySeparatorIndex + 1)..].TrimStart(' '));
                valueData = InterpretBasicObject(trimmedStart[keySeparatorIndex], valueText, allLines, lineNum);
            }
            valueData.PrecedingComments.AddRange(currentComments);
            currentComments.Clear();
            section.SetRootData(key, valueData);
            endLine = lineNum;
        }
    }

    /// <summary>
    /// Parses the input text into the given <see cref="FDSSection"/> object.
    /// <para>Generally only for internal use. Use <see cref="FDSSection"/> for most external access.</para>
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="section">The section object to output into.</param>
    /// <exception cref="FDSInputException">If parsing fails.</exception>
    public static void Parse(string text, FDSSection section)
    {
        text = FDSUtility.CleanFileData(text);
        string[] allLines = text.SplitFast('\n');
        ParseSection(allLines, 0, 0, 0, out int endLine, section);
        for (int lineNum = endLine + 1; lineNum < allLines.Length; lineNum++)
        {
            string trimmed = allLines[lineNum].Trim(' ');
            if (trimmed.Length == 0)
            {
                continue;
            }
            if (trimmed.StartsWithFast('#'))
            {
                section.PostComments.Add(trimmed[1..]);
                continue;
            }
            throw Exception(lineNum, allLines[lineNum], "Line inside root has unknown purpose - are you missing a symbol?");
        }
    }

    /// <summary>Creates an <see cref="FDSInputException"/>.</summary>
    /// <param name="linenumber">The line number where the exception occurred.</param>
    /// <param name="line">The text of the line that caused the exception.</param>
    /// <param name="reason">The reason for an exception.</param>
    public static FDSInputException Exception(int linenumber, string line, string reason)
    {
        return new FDSInputException($"[FDS Parsing error] Line {linenumber + 1}: {reason}, from line as follows: {line}");
    }
}
