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

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>
    /// Helper class for parsing out FDS data.
    /// <para>Generally only for internal use. Use <see cref="FDSSection"/> for most external access.</para>
    /// </summary>
    public static class FDSParser
    {
        /// <summary>
        /// Matcher for list prefix symbols (dash, equals, greater than).
        /// </summary>
        public static AsciiMatcher ListPrefixMatcher = new AsciiMatcher("-=>");

        /// <summary>
        /// Matcher for symbols that separate a key from a value.
        /// </summary>
        public static AsciiMatcher KeySeparatorMatcher = new AsciiMatcher(":=");

        public static void ParseList(string[] allLines, int startLine, int skip, int spacing, out int endLine, List<FDSData> outList)
        {
            Console.WriteLine(new string('\t', spacing) + $"ParseList {startLine}");
            List<string> currentComments = new List<string>();
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
                    Console.WriteLine(new string('\t', spacing) + $"ParseList ending at {lineNum} because has {spaces} / {spacing}");
                    return;
                }
                if (spaces > spacing)
                {
                    Console.WriteLine(new string('\t', spacing) + "Trying to include into list: " + string.Join(", ", outList) + "       :    " + trimmedStart);
                    throw Exception(lineNum, fullLine, $"Spacing grew for no reason (expected {spacing} but got {spaces}) inside a list - possibly forgot a key, or mixed up tabs?");
                }
                if (ListPrefixMatcher.IsMatch(firstSymbol))
                {
                    string valueText = trimmedLine[1..].TrimStart();
                    Console.WriteLine(new string('\t', spacing) + $"Try sub-list for {spacing} with {trimmedLine.Length - valueText.Length} on line {startLine}: {valueText}");
                    FDSData valueData = ParseSubListValue(firstSymbol, valueText, allLines, lineNum, spacing + (trimmedLine.Length - valueText.Length), out lineNum);
                    valueData.PrecedingComments.AddRange(currentComments);
                    currentComments.Clear();
                    outList.Add(valueData);
                    endLine = lineNum;
                    Console.WriteLine(new string('\t', spacing) + $"ParseList {lineNum} to size {outList.Count} via {valueData}");
                    continue;
                }
                return;
            }
        }

        public static FDSData ParseSubListValue(char prefix, string valueText, string[] allLines, int startLine, int spacing, out int endLine)
        {
            Console.WriteLine(new string('\t', spacing) + $"ParseSubList {startLine} : {allLines[startLine]}");
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
                    //int addedSpaces = valueText.Length - lineText.Length;
                    Console.WriteLine(new string('\t', spacing) + $"Try list for {spacing} with on line {startLine}: {valueText}");
                    List<FDSData> outList = new List<FDSData>
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
                FDSSection outSection = new FDSSection();
                Console.WriteLine(new string('\t', spacing) + $"Try map for {spacing} as ({valueText}) vs ({valueText}) on line {startLine}");
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

        public static FDSData ParseSubSection(string[] allLines, int startLine, int skip, int spacing, out int endLine)
        {
            Console.WriteLine(new string('\t', spacing) + $"ParseSubSection {startLine} : {allLines[startLine]}");
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
                    Console.WriteLine(new string('\t', spacing) + $"{spaces} / {spacing} therefore section for {trimmedLine}");
                    FDSSection subSection = new FDSSection();
                    ParseSection(allLines, startLine, skip, spaces, out endLine, subSection);
                    return new FDSData(subSection);
                }
                if (ListPrefixMatcher.IsMatch(firstSymbol))
                {
                    List<FDSData> subList = new List<FDSData>();
                    ParseList(allLines, startLine + skip, 0, spacing, out endLine, subList);
                    return new FDSData(subList);
                }
                Console.WriteLine(new string('\t', spacing) + $"ParseSubSection break at {startLine} : {fullLine}");
                break;
            }
            return new FDSData(new FDSSection());
        }

        public static void ParseSection(string[] allLines, int startLine, int skip, int spacing, out int endLine, FDSSection section)
        {
            Console.WriteLine(new string('\t', spacing) + $"ParseSection {startLine} : {allLines[startLine]}");
            List<string> currentComments = new List<string>();
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
                Console.WriteLine(new string('\t', spacing) + $"Core FoundKey {key} at {lineNum}");
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
                    Console.WriteLine("PC " + trimmed);
                    section.PostComments.Add(trimmed[1..]);
                    continue;
                }
                throw Exception(lineNum, allLines[lineNum], "Line inside root has unknown purpose - are you missing a symbol?");
            }
        }

        /// <summary>
        /// Creates an <see cref="FDSInputException"/>.
        /// </summary>
        /// <param name="linenumber">The line number where the exception occurred.</param>
        /// <param name="line">The text of the line that caused the exception.</param>
        /// <param name="reason">The reason for an exception.</param>
        public static FDSInputException Exception(int linenumber, string line, string reason)
        {
            return new FDSInputException($"[FDS Parsing error] Line {linenumber + 1}: {reason}, from line as follows: {line}");
        }
    }
}
