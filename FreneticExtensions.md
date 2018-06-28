FreneticExtensions
------------------

Frenetic Extensions is a collection of extensions and utilities for use with general C# operation, such as a pure-ASCII `ToLowerFast` string extension method.

Basically everything in here is things that C# / .NET should have, but currently don't.

## Included

- EnumerableExtensions: helpers for IEnumerable and related interfaces.
- EnumHelper: helper for C# enumerations, including a more efficient `HasFlag` option than the standard .NET Framework `Enum.HasFlag` (though still not as doing it with a manual bitflag comparison - needs improvement, ideally from the C# team at Microsoft).
- StringConversionHelper: helper for converting string input to and from various types.
- StringExtensions: helper extensions for string control.
