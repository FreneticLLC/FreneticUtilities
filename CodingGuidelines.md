Coding Guidelines
-----------------

General purpose coding guidelines for C# development.

These guidelines are recommended for C# programmers in general, and strongly expected of all Frenetic contributors.

### Basics

- Generally, follow the standard Visual Studio formatting ruleset (so: braces on own lines, spaces like `void Name(Type name, Type name2)`, etc.)
- Spaces, not tabs.
- Don't use the `var` keyword: use the explicit expected type. (So: NOT `var x = 0;`, only use `int x = 0;`).
- Start every file (after header text) with `using System;` (the standard first `using` statement). Do not remove that specific `using` line, and do not place it lower down. Avoid intentionally reordering that list beyond necessity. Generally, the list should be grouped together by root folder name (so, for example, `using FreneticUtilities.FreneticExtensions;` and `using FreneticUtilities.FreneticToolkit;` both have root folder `FreneticUtilities` and so should be together).
- Namespace should match the file folder path, and type name should match the file name (So: `public class MyType` goes in `MyType.cs`, `namespace MyProject.MySet` goes in file directory `MyProject/MySet/`).
- When reflection is involved, avoid hardcoded strings when possible. For example, instead of `"MyType"` when reflecting type `MyType`, use `nameof(MyType)`. This is useful in particular for later project updating (If `MyType` is renamed, the `nameof(MyType)` will be a compiler error if not updated, whereas `"MyType"` will be a runtime only error).
- Always include xml code documentation on fields and methods.
- Never leave unhandled compiler Errors or Warnings. Keep compiler Messages to a minimum, and handle them as soon as reasonably possible.

See **Sample 1** below for reference on basics.

### Various Specifics

- Private fields are a pain. Do not use them. Use other organizational strategies.
    - Private fields are, conceptually, the developer of a project establishing a limited and controlled API that may not be deviated from.
        - This is, of course, not the reality of C#. Private fields are not API restrictions, they are API nuisances. They can be gotten around with ease - as they should be, there's no security or other significant reason to block access.
            - Access is blocked by the `private` keyword to discourage misuse, not to prevent it.
            - The `private` keyword can be gotten around using reflection.
                - This is completely achievable, but is a pain to write code using reflection, and almost any reasonable reflection code will be very slow.
        - In the real world, very often a local code library is not going to be used exactly as prescribed by the API developer. And that's okay when it isn't used that way. It's not a remote web API (which has security concerns), it's a set of tools (which exists to serve the user).
    - We can achieve the same goal as `private` fields, without the annoying side effects, by not using the keyword `private`.
    - We prefer a very clean and quality alternative: an internal struct for private data
        - Define, within your current class/struct definition, a struct with some basic name like `Impl` (it will externally become of the form `YourType.Impl`)
            - Define all "private" fields within here (and optionally private methods as well), but mark them public.
            - Define within your type a public field holding the struct (like `public Impl Internal;`).
            - Choose one of these two options:
                - Option A: Have all references to private data in your code class to be prefixed by your public field name (like `Internal.MyValue`).
                - Option B: Define private redirect properties within your type to allow short names to still be used (like `private int MyValue { get { return Internal.MyValue; } set { Internal.MyValue = value; } }`).
            - If both options seem like too much work: just make your fields public at normal root level. If you don't want to write a few more lines to make your code work as a clean API-style organized library, then you don't want a clean API-style organized library, and thus public fields are fine.
        - Now your type is presented with the API-style organization you wish to be put forth, but allows users with higher needs (like modders) to use more advanced actions with relative simplicity (like `myObj.Internal.MyValue = 0;`).
        - If access to private fields is near-guaranteed to be an error (it is never entirely guaranteed to be done in error), you might want to add a deprecation notice to the `Internal` field to better discourage misuse.
            - As easy as `[Obsolete("This is an internal control, do NOT use this unless you absolutely need to!")]` on the `Internal` field.
            - Modders who wish to access it anyway and remove the compiler warning can wrap their code with `#pragma warning disable CS0618` and `#pragma warning restore CS0618` (you may need to wrap your own type with these, to avoid warnings from using your own internal object).
        - See **Sample 2** below for an example of this.

- All names given (namespace, type, field, method, local variable, anything else) should be clean/clear/readable.
    - For example, `static Path CalculatePath(Location start, Location end)` is great naming, whereas `static List<Loc> CalcTo(Loc a, Loc b)` is not!
    - Short names (especially single-letter names like `x`) should be avoided except where they are sufficiently clear alone. For example, a `Location` class would probably have fields `X`, `Y`, and `Z`, and that's sufficiently clear there. Additionally, `for` loops often use variable `i`, which generally is fine.
    - At the same time, horribly long names (like `BeginToGoSoFarAsToSeeMoreLikeTheCalculationOfTheMethodThatThuslyFollows`) should be avoided for conciseness reasons: put the long description in the summary documentation slot, not the method name.
    - In general, a name should describe exactly what it represents, and nothing more, without being repetitive.
    - A name including its own type should be used only in cases where the type is a critical point in what it is. For example, a `ConvertToLocation` method might have input parameter names like `inputString` on the grounds that the type is the critical information included.

### Samples

**Sample 1:** Simple method:
```cs
/// <summary>This is my method, it does something.</summary>
/// <param name="input">The value that is input to the method.</param>
public void MyMethod(int input)
{
    int myInt = input + 1;
    DoSomething(myInt);
}
```

**Sample 2:** A class with "private" fields:
```cs
/// <summary>This is my class, it does something.</summary>
public class MyClassHere
{
    /// <summary>Internal implementation data, do not touch.</summary>
    public struct Impl
    {
        /// <summary>Internal integer value that is used for things.</summary>
        public int MyValue;
        
        /// <summary>Internal string value that is used for things.</summary>
        public string MyName;
    }
    
    /// <summary>Internal implementation data, do not touch.</summary>
    public Impl Internal;
    
    /// <summary>Construct my class.</summary>
    /// <param name="name">The name of the object.</param>
    /// <param name="val">The value for the object.</param>
    public MyClassHere(string name, int val)
    {
        Internal.MyName = name;
        Internal.MyValue = val;
    }
    
    /// <summary>Get the name of the object.</summary>
    public string Name => Internal.Name;
    
    /// <summary>Get the value of the object.</summary>
    public int Value
    {
        get => Internal.MyValue;
        set
        {
            if (value <= 0)
            {
                throw new Exception("Value must be > 0!");
            }
            Internal.MyValue = value;
        }
    }
}
```
