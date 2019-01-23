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
using System.Globalization;

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>
    /// A special helper for converting arbitrary Object input to and from various types.
    /// </summary>
    public static class ObjectConversionHelper
    {
        /// <summary>
        /// Converts an object value to the long-integer value it represents.
        /// Returns the specified default value (or null if unset) if the object does not represent a long-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a nullable long-integer value.</returns>
        public static long? ObjectToLong(Object input, long? defaultValue = null)
        {
            switch (input)
            {
                case long _long:
                    return _long;
                case ulong _ulong:
                    return (long)_ulong;
                case int _int:
                    return _int;
                case uint _uint:
                    return _uint;
                case short _short:
                    return _short;
                case ushort _ushort:
                    return _ushort;
                case byte _byte:
                    return _byte;
                case sbyte _sbyte:
                    return _sbyte;
                case string str:
                    if (long.TryParse(str, out long _string_long))
                    {
                        return _string_long;
                    }
                    return defaultValue;
                default:
                    if (long.TryParse(input.ToString(), out long _object_string_long))
                    {
                        return _object_string_long;
                    }
                    return defaultValue;
            }
        }

        /// <summary>
        /// Converts an object value to the unsigned long-integer value it represents.
        /// Returns the specified default value (or null if unset) if the object does not represent an unsigned long-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>a nullable unsigned long-integer value.</returns>
        public static ulong? ObjectToULong(Object input, ulong? defaultValue = null)
        {
            switch (input)
            {
                case ulong _ulong:
                    return _ulong;
                case long _long:
                    return (ulong)_long;
                case int _int:
                    return (ulong)_int;
                case uint _uint:
                    return _uint;
                case short _short:
                    return (ulong)_short;
                case ushort _ushort:
                    return _ushort;
                case byte _byte:
                    return _byte;
                case sbyte _sbyte:
                    return (ulong)_sbyte;
                case string str:
                    if (ulong.TryParse(str, out ulong _string_long))
                    {
                        return _string_long;
                    }
                    return defaultValue;
                default:
                    if (ulong.TryParse(input.ToString(), out ulong _object_string_long))
                    {
                        return _object_string_long;
                    }
                    return defaultValue;
            }
        }
    }
}
