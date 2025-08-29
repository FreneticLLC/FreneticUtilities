//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticUtilities.FreneticToolkit;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace FreneticUtilitiesUnitTests;

/// <summary>Represents any test in Frenetic Utilities. Should be derived from.</summary>
public abstract class FreneticUtilitiesUnitTest
{
    /// <summary>ALWAYS call this in a test's static OneTimeSetUp!</summary>
    public static void Setup()
    {
        SpecialTools.Internationalize();
    }

    /// <summary>Asserts that two normal-range doubles are approximately equal (down to 4 decimal places).</summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">The message to display if they aren't roughly equal.</param>
    public static void AssertAreRoughlyEqual(double expected, double actual, string message)
    {
        ClassicAssert.AreEqual((int)Math.Round(expected * 10000), (int)Math.Round(actual * 10000), message);
    }
}
