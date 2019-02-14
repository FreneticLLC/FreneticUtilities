FreneticToolkit
---------------

Frenetic Toolkit is a collection of general utilities for use a variety of cases, such as common data type conversion tools.

## Included

- **AsciiMatcher**: helper to match specified ASCII characters within strings in a very fast and clean way.
- **EnumHelper**: helper for C# enumerations, including a more efficient `HasFlag` option than the standard .NET Framework `Enum.HasFlag` (though still not as fast as doing it with a manual bitflag comparison - needs improvement, ideally from the C# team at Microsoft).
- **FreneticEvent**: an event handler system, including special helpers for things like synchronous pausable firings and other handy 'script-like' features.
- **ObjectConversionHelper**: helper for converting arbitrary object input to and from various common types.
- **PrimitiveConversionHelper**: helper for converting raw byte array input to and from various primitive types.
- **StringConversionHelper**: helper for converting string input to and from various common types.
