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
using System.Diagnostics;

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>
    /// A special helper for various mathematical functions.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Returns the next power of two.
        /// Meaning, the next number in the sequence:
        /// 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, ...
        /// Result is >= input.
        /// </summary>
        /// <param name="x">The value, less than or equal to the result.</param>
        /// <returns>The result, greater than or equal to the value.</returns>
        public static int NextPowerOfTwo(int x)
        {
            Debug.Assert(x > 0, $"For NextPowerOfTwo, X must be > 0, but was {x}");
            // Spread the Most Significant Bit all the way down
            // so eg "00100100" becomes "11111100"
            int spreadMSB = x | (x >> 1);
            spreadMSB |= spreadMSB >> 2;
            spreadMSB |= spreadMSB >> 4;
            spreadMSB |= spreadMSB >> 8;
            spreadMSB |= spreadMSB >> 16;
            // Full value minus the downshift of it = *only* the MSB
            int onlyMSB = spreadMSB - (spreadMSB >> 1);
            // Exactly on MSB = return that, otherwise we're greater so grow by one power.
            if (x == onlyMSB)
            {
                return onlyMSB;
            }
            return onlyMSB << 1;
        }

        /// <summary>
        /// Steps a value towards a goal by a specified amount, automatically moving the correct direction (positive or negative) and preventing going past the goal.
        /// </summary>
        /// <param name="start">The initial value.</param>
        /// <param name="target">The goal value.</param>
        /// <param name="amount">The amount to step by.</param>
        /// <returns>The result.</returns>
        public static double StepTowards(double start, double target, double amount)
        {
            if (start < target - amount)
            {
                return start + amount;
            }
            else if (start > target + amount)
            {
                return start - amount;
            }
            else
            {
                return target;
            }
        }

        /// <summary>
        /// Returns whether a number is close to another number, within a specified range.
        /// </summary>
        /// <param name="one">The first number.</param>
        /// <param name="target">The second number.</param>
        /// <param name="amount">The range.</param>
        /// <returns>Whether it's close.</returns>
        public static bool IsCloseTo(double one, double target, double amount)
        {
            return Math.Abs(one - target) < amount;
        }

        /// <summary>
        /// Clamps an integer value to within a range.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static int Clamp(int value, int minimum, int maximum)
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }

        /// <summary>
        /// Clamps a float value to within a range.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }

        /// <summary>
        /// Clamps a double value to within a range.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }
    }
}
