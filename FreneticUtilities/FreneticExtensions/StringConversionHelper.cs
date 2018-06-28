using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace FreneticUtilities.FreneticExtensions
{
    /// <summary>
    /// A special helper for converting <see cref="string"/> input to and from various types.
    /// </summary>
    public static class StringConversionHelper
    {
        /// <summary>
        /// A standard UTF-8 encoding helper object instance.
        /// <para>This is not equialent to <see cref="Encoding.UTF8"/> as that will output BOM when converting a <see cref="string"/> to binary data (which is usually bad).</para>
        /// <para>This instance, by contrast, is guaranteed to not do that.</para>
        /// </summary>
        public static readonly UTF8Encoding UTF8Encoding = new UTF8Encoding(false);

        /// <summary>
        /// Converts a string value to the long-integer value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a long-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a long-integer value.</returns>
        public static long StringToLong(string input, long defaultValue = 0)
        {
            if (long.TryParse(input, out long output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the integer value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent an integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>an integer value.</returns>
        public static int StringToInt(string input, int defaultValue = 0)
        {
            if (int.TryParse(input, out int output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the double value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a double.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a double value.</returns>
        public static double StringToDouble(string input, double defaultValue = 0.0)
        {
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string value to the float value it represents.
        /// Returns the specified default value (or zero if unset) if the string does not represent a float.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a float value.</returns>
        public static float StringToFloat(string input, float defaultValue = 0f)
        {
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float output))
            {
                return output;
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string to a date-time.
        /// <para>Parsing errors will give a null result.</para>
        /// <para>Expected format is "YYYY/MM/DD hh:mm:ss UTC+OO:oo".</para>
        /// <para>YYYY = 4 digit year, MM = 2 digit month, DD = 2 digit day.</para>
        /// <para>hh = 2 digit hour, mm = 2 digit minute, ss = 2 digit second.</para>
        /// <para>OO = 2 digit offset hours, oo = 2 digit offset minutes. Can be prefixed with a '+' or '-'.</para>
        /// <para>Optionally add 4-digit millisecond, like "YYYY/MM/DD hh:mm:ss:tttt UTC+OO:oo".</para>
        /// <para>Inverted by <see cref="DateTimeToString(DateTimeOffset, bool)"/>.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The date-time.</returns>
        public static DateTimeOffset? StringToDateTime(string input)
        {
            string[] inputSplit = input.SplitFast(' ');
            if (inputSplit.Length != 3)
            {
                return null;
            }
            string[] yearmonthday = inputSplit[0].SplitFast('/');
            if (yearmonthday.Length != 3)
            {
                return null;
            }
            if (!int.TryParse(yearmonthday[0], out int year)
                || !int.TryParse(yearmonthday[1], out int month)
                || !int.TryParse(yearmonthday[2], out int day))
            {
                return null;
            }
            string[] hourminutesecond = inputSplit[1].SplitFast(':');
            if (hourminutesecond.Length != 3 && hourminutesecond.Length != 4)
            {
                return null;
            }
            if (!int.TryParse(hourminutesecond[0], out int hour)
                || !int.TryParse(hourminutesecond[1], out int minute)
                || !int.TryParse(hourminutesecond[2], out int second))
            {
                return null;
            }
            int millisecond = 0;
            if (hourminutesecond.Length == 4 && !int.TryParse(hourminutesecond[3], out millisecond))
            {
                return null;
            }
            int offsetHours = 0;
            int offsetMinutes = 0;
            if (inputSplit[2].Contains('-'))
            {
                string[] offsetinfo = inputSplit[2].SplitFast('-');
                string[] subinf = offsetinfo[1].SplitFast(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                if (!int.TryParse(subinf[0], out offsetHours)
                    && !int.TryParse(subinf[1], out offsetMinutes))
                {
                    return null;
                }
                offsetHours = -offsetHours;
                offsetMinutes = -offsetMinutes;
            }
            else
            {
                string[] offsetinfo = inputSplit[2].SplitFast('+');
                if (offsetinfo.Length != 2)
                {
                    return null;
                }
                string[] subinf = offsetinfo[1].SplitFast(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                if (!int.TryParse(subinf[0], out offsetHours)
                    && !int.TryParse(subinf[1], out offsetMinutes))
                {
                    return null;
                }
            }
            TimeSpan offset = new TimeSpan(offsetHours, offsetMinutes, 0);
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
        }

        /// <summary>
        /// Local utility used for <see cref="DateTimeToString(DateTimeOffset, bool)"/>.
        /// </summary>
        private static string QPad(string input, int length = 2)
        {
            return input.PadLeft(length, '0');
        }

        /// <summary>
        /// Returns a string representation of the specified time.
        /// <para>Format is "YYYY/MM/DD hh:mm:ss UTC+OO:oo".</para>
        /// <para>YYYY = 4 digit year, MM = 2 digit month, DD = 2 digit day.</para>
        /// <para>hh = 2 digit hour, mm = 2 digit minute, ss = 2 digit second.</para>
        /// <para>OO = 2 digit offset hours, oo = 2 digit offset minutes. Can be prefixed with a '+' or '-'.</para>
        /// <para>if 'showMilliseconds' is true, will add 4-digit millisecond, like "YYYY/MM/DD hh:mm:ss:tttt UTC+OO:oo".</para>
        /// <para>Inverted by <see cref="StringToDateTime(string)"/>.</para>
        /// </summary>
        /// <param name="input">The datetime object.</param>
        /// <param name="showMilliseconds">Whether to include milliseconds.</param>
        /// <returns>The time as a string.</returns>
        public static string DateTimeToString(DateTimeOffset input, bool showMilliseconds)
        {
            string utcoffset;
            if (input.Offset.TotalMilliseconds < 0)
            {
                // TODO: Are minutes correctly handled here (for negative offset)?
                utcoffset = "-" + QPad(((int)Math.Abs(Math.Floor(input.Offset.TotalHours))).ToString()) + ":" + QPad(input.Offset.Minutes.ToString());
            }
            else
            {
                utcoffset = "+" + QPad(((int)Math.Floor(input.Offset.TotalHours)).ToString()) + ":" + QPad(input.Offset.Minutes.ToString());
            }
            return QPad(input.Year.ToString(), 4) + "/" + QPad(input.Month.ToString()) + "/" + QPad(input.Day.ToString())
                + " " + QPad(input.Hour.ToString()) + ":" + QPad(input.Minute.ToString()) + ":" + QPad(input.Second.ToString())
                + (showMilliseconds ? ":" + QPad(input.Millisecond.ToString(), 4) : "")
                + " UTC" + utcoffset;
        }
    }
}
