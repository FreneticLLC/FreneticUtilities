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

namespace FreneticUtilities.FreneticExtensions;

/// <summary>Helper extensions for various types without better extension files.</summary>
public static class OtherExtensions
{
    /// <summary>Gets a Gaussian random value from a Random object.</summary>
    /// <param name="input">The random object.</param>
    /// <returns>The Gaussian value.</returns>
    public static double NextGaussian(this Random input)
    {
        double u1 = input.NextDouble();
        double u2 = input.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }

    /// <summary>Utility for "AutoFormatResult" to automatically apply an "s" when needed.</summary>
    private static string AutoS(int val)
    {
        return val == 1 ? "" : "s";
    }

    /// <summary>Utility for <see cref="SimpleFormat(TimeSpan, bool)"/> for building a result.</summary>
    private static string AutoFormatResult(int largeValue, string largeValueName, int smallValue, string smallValueName)
    {
        if (smallValue == 0 && largeValue == 0)
        {
            return $"0 {smallValueName}s";
        }
        string large = $"{largeValue} {largeValueName}{AutoS(largeValue)}";
        string small = $"{smallValue} {smallValueName}{AutoS(smallValue)}";
        if (smallValue == 0)
        {
            return large;
        }
        if (largeValue == 0)
        {
            return small;
        }
        return $"{large} and {small}";
    }

    /// <summary>
    /// Gets a simple human-friendly formatted text version of this timespan.
    /// <para>Contains 2 points of information, starting at the first non-zero value from: Years, Days, Hours, Minutes, Seconds, Milliseconds.</para>
    /// <para>Example result would be: "5 hours and 15 minutes".</para>
    /// <para>For timespans of zero, returns "0 seconds".</para>
    /// <para>Negative timespans will have their negative value ignored unless <paramref name="addAgo"/> is set.</para>
    /// <para>This is for human-friendly output and will NOT reliably reconstruct, as it loses precision.</para>
    /// </summary>
    /// <param name="duration">The duration of time.</param>
    /// <param name="addAgo">For positive timespans, ends the result with "from now". For negative timespans, ends the result with "ago".</param>
    /// <returns>The text formatting of the timespan.</returns>
    public static string SimpleFormat(this TimeSpan duration, bool addAgo)
    {
        bool wasNegative = duration.TotalMilliseconds < 0;
        if (wasNegative)
        {
            duration = -duration;
        }
        string result;
        if (duration.TotalMilliseconds < 0.1)
        {
            result = "0 seconds";
        }
        else if (duration.TotalMinutes < 1)
        {
            result = AutoFormatResult(duration.Seconds, "second", duration.Milliseconds, "millisecond");
        }
        else if (duration.TotalHours < 1)
        {
            result = AutoFormatResult(duration.Minutes, "minute", duration.Seconds, "second");
        }
        else if (duration.TotalDays < 1)
        {
            result = AutoFormatResult(duration.Hours, "hour", duration.Minutes, "minute");
        }
        else if (duration.TotalDays < 365)
        {
            result = AutoFormatResult(duration.Days, "day", duration.Hours, "hour");
        }
        else
        {
            int years = duration.Days / 365;
            result = AutoFormatResult(years, "year", duration.Days - (years * 365), "day");
        }
        if (addAgo)
        {
            result += wasNegative ? " ago" : " from now";
        }
        return result;
    }

    /// <summary>Forces a <see cref="ValueTask"/> to be waitable the normal way.</summary>
    /// <param name="task">The <see cref="ValueTask"/> that can't be waited for properly.</param>
    /// <returns>A properly waitable task.</returns>
    public static async Task<T> Normalize<T>(this ValueTask<T> task)
    {
        return await task;
    }
}
