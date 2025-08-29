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
using FreneticUtilities.FreneticToolkit;
using NUnit.Framework;

namespace FreneticUtilitiesUnitTests.FreneticToolkitTests;

/// <summary>Tests expectations of <see cref="StringConversionHelper"/>.</summary>
public class StringConversionHelperTests : FreneticUtilitiesUnitTest
{
    /// <summary>Prepares the basics.</summary>
    [OneTimeSetUp]
    public static void PreInit()
    {
        Setup();
    }

    /// <summary>Tests "SimpleCliArgsParser".</summary>
    [Test]
    public static void SimpleCliArgsParserTest()
    {
        Dictionary<string, string> testOne = StringConversionHelper.SimpleCliArgsParser(["--key", "value", "--flag", "--otherkey=othervalue", "--longkey", "some", "value"]);
        Assert.That(testOne.Count, Is.EqualTo(4), "CliArgsParse correct number of args");
        Assert.That(testOne.ContainsKey("key"), "CliArgsParse contains 'key'");
        Assert.That(testOne.ContainsKey("flag"), "CliArgsParse contains 'flag'");
        Assert.That(testOne.ContainsKey("otherkey"), "CliArgsParse contains 'otherkey'");
        Assert.That(testOne.ContainsKey("longkey"), "CliArgsParse contains 'longkey'");
        Assert.That(testOne["key"], Is.EqualTo("value"), "CliArgsParse 'key' value");
        Assert.That(testOne["flag"], Is.EqualTo(""), "CliArgsParse 'flag' value");
        Assert.That(testOne["otherkey"], Is.EqualTo("othervalue"), "CliArgsParse 'otherkey' value");
        Assert.That(testOne["longkey"], Is.EqualTo("some value"), "CliArgsParse 'longkey' value");
        Dictionary<string, string> emptyTest = StringConversionHelper.SimpleCliArgsParser([]);
        Assert.That(emptyTest.Count, Is.EqualTo(0), "CliArgsParse empty args");
        Dictionary<string, string> simpleKeyTest = StringConversionHelper.SimpleCliArgsParser(["--justkey", "MyVal"]);
        Assert.That(simpleKeyTest.Count, Is.EqualTo(1), "CliArgsParse simple key count");
        Assert.That(simpleKeyTest.ContainsKey("justkey"), "CliArgsParse simple key");
        Assert.That(simpleKeyTest["justkey"], Is.EqualTo("MyVal"), "CliArgsParse simple key value");
        Dictionary<string, string> singleFlagTest = StringConversionHelper.SimpleCliArgsParser(["--onlyflag"]);
        Assert.That(singleFlagTest.Count, Is.EqualTo(1), "CliArgsParse single flag count");
        Assert.That(singleFlagTest.ContainsKey("onlyflag"), "CliArgsParse single flag");
        Assert.That(singleFlagTest["onlyflag"], Is.EqualTo(""), "CliArgsParse single flag value");
        Dictionary<string, string> caseTest = StringConversionHelper.SimpleCliArgsParser(["--JustKey", "MyVal"]);
        Assert.That(caseTest.Count, Is.EqualTo(1), "CliArgsParse simple cased key count");
        Assert.That(caseTest.ContainsKey("JustKey"), "CliArgsParse simple cased key");
        Assert.That(caseTest.ContainsKey("justkey"), "CliArgsParse simple cased key 2");
        Assert.That(caseTest["JustKey"], Is.EqualTo("MyVal"), "CliArgsParse simple cased key value");
        Assert.That(caseTest["justkey"], Is.EqualTo("MyVal"), "CliArgsParse simple cased key value 2");
        Dictionary<string, string> longEqualsKeyTest = StringConversionHelper.SimpleCliArgsParser(["--longkey=first", "second", "third"]);
        Assert.That(longEqualsKeyTest.Count, Is.EqualTo(1), "CliArgsParse long equals key count");
        Assert.That(longEqualsKeyTest.ContainsKey("longkey"), "CliArgsParse long equals key");
        Assert.That(longEqualsKeyTest["longkey"], Is.EqualTo("first second third"), "CliArgsParse long equals key value");
    }
}
