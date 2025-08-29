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

namespace FreneticUtilitiesUnitTests.FreneticExtensionsTests;

/// <summary>Tests expectations of <see cref="OtherExtensions"/>.</summary>
[TestFixture]
public class OtherExtensionTests : FreneticUtilitiesUnitTest
{
    /// <summary>Prepares the basics.</summary>
    [OneTimeSetUp]
    public static void PreInit()
    {
        Setup();
    }

    /// <summary>
    /// Tests "NextGaussian".
    /// Note: this test can/should fail if the underlying implementation of <see cref="Random"/> changes.
    /// </summary>
    [Test]
    public static void NextGaussian()
    {
        Random random = new(12345);
        AssertAreRoughlyEqual(0.992784877525018, random.NextGaussian(), "Gaussian first try failed");
        AssertAreRoughlyEqual(-0.0499612497007243, random.NextGaussian(), "Gaussian second try failed");
        AssertAreRoughlyEqual(-0.594917629112312, random.NextGaussian(), "Gaussian third try failed");
        AssertAreRoughlyEqual(-1.88807372437318, random.NextGaussian(), "Gaussian fourth try failed");
        AssertAreRoughlyEqual(-0.0618950052147339, random.NextGaussian(), "Gaussian fifth try failed");
    }
}
