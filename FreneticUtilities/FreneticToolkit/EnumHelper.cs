//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>Helper for <see cref="Enum"/> types.</summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static class EnumHelper<T> where T : Enum
    {
        /// <summary>
        /// A map of names to values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly Dictionary<string, T> NameValueMap;

        /// <summary>
        /// A map of lowercased names to values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly Dictionary<string, T> LoweredNameValueMap;

        /// <summary>
        /// A map of values to names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly Dictionary<T, string> ValueNameMap;

        /// <summary>
        /// An array of all names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly string[] Names;

        /// <summary>
        /// A set of all names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly HashSet<string> NameSet;

        /// <summary>
        /// An array of all lowercased names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly string[] LoweredNames;

        /// <summary>
        /// A set of all lowercased names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly HashSet<string> LoweredNameSet;

        /// <summary>
        /// An array of all values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly T[] Values;

        /// <summary>
        /// A set of all values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly HashSet<T> ValueSet;

        /// <summary>Gets the underlying type for the enum type.</summary>
        public static readonly Type UnderlyingType;

        /// <summary>Whether this is a flags enum.</summary>
        public static readonly bool IsFlags;

        /// <summary>A long converter function. Should only be used for very special case situations - usually, a normal cast works fine.</summary>
        public static readonly Func<T, long> LongConverter;

        /// <summary>
        /// A flag tester function. Ideally there will be a way to do this cleanly without dynamic code gen some day...
        /// (Other than just implementing the mathematical comparison inline).
        /// </summary>
        public static readonly Func<T, T, bool> FlagTester;

        static EnumHelper()
        {
            Names = Enum.GetNames(typeof(T));
            NameSet = new HashSet<string>(Names);
            Values = Enum.GetValues(typeof(T)) as T[];
            ValueSet = new HashSet<T>(Values);
            NameValueMap = Names.ToDictionaryWithNoDup(Values);
            LoweredNameValueMap = Names.Select(StringExtensions.ToLowerFast).ToList().ToDictionaryWith(Values);
            ValueNameMap = Values.ToDictionaryWith(Names);
            UnderlyingType = Enum.GetUnderlyingType(typeof(T));
            IsFlags = typeof(T).GetCustomAttributes(typeof(FlagsAttribute), true).Length != 0;
            LongConverter = CreateLongConverter();
            FlagTester = CreateFlagTester();
        }

        /// <summary>This is a gross hack used since C# handles enum types poorly. This should be destroyed and replaced as soon as C# does it better. (Or perhaps a T4 generator should be used?)</summary>
        /// <returns>A long converter function.</returns>
        static Func<T, long> CreateLongConverter()
        {
            // long EnumToLong(T val) { return (long)val; }
            DynamicMethod method = new("EnumToLong", typeof(long), new Type[] { typeof(T) }, typeof(EnumHelper<T>).Module, true);
            ILGenerator ilgen = method.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0); // Load arg 0 (flag)
            ilgen.Emit(OpCodes.Conv_I8); // Convert it to an int64 (long)
            ilgen.Emit(OpCodes.Ret); // Return the long
            return (Func<T, long>)method.CreateDelegate(typeof(Func<T, long>));
        }

        /// <summary>This is a gross hack used since C# handles enum types poorly. This should be destroyed and replaced as soon as C# does it better. (Or perhaps a T4 generator should be used?)</summary>
        /// <returns>A flag tester function.</returns>
        static Func<T, T, bool> CreateFlagTester()
        {
            // bool FlagTester(T one, T two) { return (one & two) == two; }
            DynamicMethod method = new("FlagTester", typeof(bool), new Type[] { typeof(T), typeof(T) }, typeof(EnumHelper<T>).Module, true);
            ILGenerator ilgen = method.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0); // Load arg 0 (flag to be tested)
            ilgen.Emit(OpCodes.Ldarg_1); // Load arg 1 (flag to test for)
            ilgen.Emit(OpCodes.And); // Bitwise-AND of the two args
            ilgen.Emit(OpCodes.Ldarg_1); // Loard arg 1 (flag to test for)
            ilgen.Emit(OpCodes.Ceq); // Compare equality of the AND result and arg 1, push boolean result
            ilgen.Emit(OpCodes.Ret); // return the boolean result
            return (Func<T, T, bool>)method.CreateDelegate(typeof(Func<T, T, bool>));
        }

        /// <summary>
        /// Gets the value for the name, ignoring case.
        /// Returns whether the name was found.
        /// </summary>
        /// <param name="name">The name input.</param>
        /// <param name="value">The value output (when returning true).</param>
        /// <returns>Success state.</returns>
        public static bool TryParseIgnoreCase(string name, out T value)
        {
            return LoweredNameValueMap.TryGetValue(name.ToLowerFast(), out value);
        }

        /// <summary>
        /// Gets the value for the name.
        /// Returns whether the name was found.
        /// </summary>
        /// <param name="name">The name input.</param>
        /// <param name="value">The value output (when returning true).</param>
        /// <returns>Success state.</returns>
        public static bool TryParse(string name, out T value)
        {
            return NameValueMap.TryGetValue(name, out value);
        }

        /// <summary>
        /// Gets the value for the name, ignoring case.
        /// Throws an exception if name is invalid.
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The enum value.</returns>
        public static T ParseIgnoreCase(string name)
        {
            return LoweredNameValueMap[name.ToLowerFast()];
        }

        /// <summary>
        /// Gets the value for the name.
        /// Throws an exception if name is invalid.
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The enum value.</returns>
        public static T Parse(string name)
        {
            return NameValueMap[name];
        }

        /// <summary>Returns whether the name is defined in the enumeration, ignoring case.</summary>
        /// <param name="name">The name to test.</param>
        /// <returns>Whether it's defined.</returns>
        public static bool IsNameDefinedIgnoreCase(string name)
        {
            return LoweredNameSet.Contains(name.ToLowerFast());
        }

        /// <summary>Returns whether the name is defined in the enumeration.</summary>
        /// <param name="name">The name to test.</param>
        /// <returns>Whether it's defined.</returns>
        public static bool IsNameDefined(string name)
        {
            return Names.Contains(name);
        }

        /// <summary>Returns whether the value is defined in the enumeration.</summary>
        /// <param name="value">The value to test.</param>
        /// <returns>Whether it's defined.</returns>
        public static bool IsDefined(T value)
        {
            return ValueSet.Contains(value);
        }

        /// <summary>Returns whether the <paramref name="mainValue"/> (as a bitflag set) has the required <paramref name="testValue"/> (as a bitflag set).</summary>
        /// <param name="mainValue">The set of flags present.</param>
        /// <param name="testValue">The set of flags required.</param>
        /// <returns>Whether the flags are present as required.</returns>
        public static bool HasFlag(T mainValue, T testValue)
        {
            return FlagTester(mainValue, testValue);
        }

        /// <summary>
        /// Gets the name for a value (if it is defined).
        /// Returns success state.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name output (when returning true).</param>
        /// <returns>Success state.</returns>
        public static bool TryGetName(T value, out string name)
        {
            return ValueNameMap.TryGetValue(value, out name);
        }

        /// <summary>Gets the name for a value (if it is defined).</summary>
        /// <param name="value">The value.</param>
        /// <returns>The name.</returns>
        public static string GetName(T value)
        {
            return ValueNameMap[value];
        }
    }
}
