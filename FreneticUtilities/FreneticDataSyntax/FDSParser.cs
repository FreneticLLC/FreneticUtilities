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

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>
    /// Helper class for parsing out FDS data.
    /// <para>Generally only for internal use. Use <see cref="FDSSection"/> for most external access.</para>
    /// </summary>
    public static class FDSParser
    {
        /// <summary>
        /// Parses the input text into the given <see cref="FDSSection"/> object.
        /// <para>Generally only for internal use. Use <see cref="FDSSection"/> for most external access.</para>
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="section">The section object to output into.</param>
        /// <exception cref="FDSInputException">If parsing fails.</exception>
        public static void Parse(string text, FDSSection section)
        {
            // TODO: Clean code base! Current code contains a lot of poorly named variables and messy code.
            text = FDSUtility.CleanFileData(text);
            Dictionary<int, FDSSection> spacedsections = new Dictionary<int, FDSSection>() { { 0, section } };
            List<string> ccomments = new List<string>();
            List<string> seccomments = new List<string>();
            FDSSection csection = section;
            string[] data = text.SplitFast('\n');
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
                if (datum[0] == '-' || datum[0] == '=')
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
                    string unescaped = FDSUtility.UnEscape(clistline);
                    clist.Add(new FDSData()
                    {
                        PrecedingComments = new List<string>(ccomments),
                        Internal = datum[0] == '=' ? FDSUtility.FromBase64(unescaped) : FDSUtility.InterpretType(unescaped)
                    });
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
                        endofline = spot == datum.Length - 1 ? "" : datum.Substring(spot + 1);
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
                    csection.SetRootData(FDSUtility.UnEscapeKey(secwaiting), new FDSData()
                    {
                        PrecedingComments = new List<string>(seccomments),
                        Internal = sect
                    });
                    seccomments.Clear();
                    csection = sect;
                    spacedsections[spaces] = sect;
                    secwaiting = null;
                }
                if (type == '=')
                {
                    csection.SetRootData(FDSUtility.UnEscapeKey(startofline), new FDSData()
                    {
                        PrecedingComments = new List<string>(ccomments),
                        Internal = FDSUtility.FromBase64(FDSUtility.UnEscape(endofline))
                    });
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
                        csection.SetRootData(FDSUtility.UnEscapeKey(startofline), new FDSData()
                        {
                            PrecedingComments = new List<string>(ccomments),
                            Internal = FDSUtility.InterpretType(FDSUtility.UnEscape(endofline))
                        });
                        ccomments.Clear();
                    }
                }
                else
                {
                    Exception(i, line, "Internal issue: unrecognize 'type' value: " + type);
                }
                pspaces = spaces;
            }
            section.PostComments.AddRange(ccomments);
        }

        /// <summary>
        /// Throws an <see cref="FDSInputException"/>.
        /// </summary>
        /// <param name="linenumber">The line number where the exception occurred.</param>
        /// <param name="line">The text of the line that caused the exception.</param>
        /// <param name="reason">The reason for an exception.</param>
        public static void Exception(int linenumber, string line, string reason)
        {
            throw new FDSInputException("[FDS Parsing error] Line " + (linenumber + 1) + ": " + reason + ", from line as follows: `" + line + "`");
        }
    }
}
