//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace FreneticUtilitiesUnitTests.FreneticExtensionsTests;

/// <summary>Tests expectations of <see cref="StreamExtensions"/>.</summary>
[TestFixture]
public class StreamExtensionTests : FreneticUtilitiesUnitTest
{
    /// <summary>Prepares the basics.</summary>
    [OneTimeSetUp]
    public static void PreInit()
    {
        Setup();
    }

    /// <summary>Tests "AllLinesOfText"</summary>
    [Test]
    public static void AllLinesOfTextTest()
    {
        string input = "Wow\nThis\nIs a big ol'\nlist of text\nyep";
        string[] splitInput = input.Split('\n');
        Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        IEnumerable<string> result = stream.AllLinesOfText();
        int i = 0;
        foreach (string str in result)
        {
            ClassicAssert.AreEqual(splitInput[i++], str, $"Stream AllLinesOfText broke at line {i}");
        }
        ClassicAssert.AreEqual(5, i, "Stream AllLinesOfText length wrong");
    }
}
