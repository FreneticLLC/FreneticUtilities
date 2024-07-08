FreneticToolkit
---------------

Frenetic Toolkit is a collection of general utilities for use a variety of cases, such as common data type conversion tools.

## General

- **AsciiMatcher**: helper to match specified ASCII characters within strings in a very fast and clean way.
- **FreneticEvent**: an event handler system, including special helpers for things like synchronous pausable firings and other handy 'script-like' features.
- **ILGeneratorTracker**: helper to replace .NET's `ILGenerator` with one that's capable of input tracking and error checking.
- **ResizableArray**: special optimization hack, essentially a `List<T>` that allows direct access to the underlying array.

## Broad Helpers

- **EnumHelper**: helper for C# enumerations, including a more efficient `HasFlag` option than the standard .NET Framework `Enum.HasFlag` (though still not as fast as doing it with a manual bitflag comparison - needs improvement, ideally from the C# team at Microsoft).
- **MathHelper**: some handy basic math functions not built into .NET.
- **ObjectConversionHelper**: helper for converting arbitrary object input to and from various common types.
- **PrimitiveConversionHelper**: helper for converting raw byte array input to and from various primitive types.
- **SpecialTools**: special fixes for .NET issues, notably broken localization behavior in .NET (eg `float.Parse((1.5f).ToString())` returns an error in some cultures, and the only sane solution is to turn autocultures off so C# behaves predictably, ie so that C# will work in non-American locations. The fact that we have to *disable* localization to have code reliably work in different localities is concerning - Microsoft needs to get their code in order).
- **StringConversionHelper**: helper for converting string input to and from various common types.

## Async

- **AsyncAutoResetEvent**: like `AutoResetEvent` but awaitable. I don't know why this isn't built into .NET itself.
- **LockObject**: just a handy tiny little empty object class to use with C# `lock` syntax.
- **MultiLockSet**: tracks a set of `LockObject`s within a simple static table with hash based lookups for any hashable data type. Useful for preventing async overlap of a complex set without dynamic tracking.
- **MultiSemaphoreSet**: same as `MultiLockSet` but for `SemaphoreSlim` objects.
- **SingleCacheAsync**: caches a single value based on a current lookup key, in a tiny fast async-safe way.
- **SingleValueExperingCacheAsync**: similar to `SingleCacheAsync`, but with a stable expiration time.

