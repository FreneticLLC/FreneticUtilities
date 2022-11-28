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
    - Some compiler messages are redundant or unhelpful. See the [FreneticUtilities GlobalSupressions file](https://github.com/FreneticLLC/FreneticUtilities/blob/master/FreneticUtilities/GlobalSuppressions.cs) for specific recommended suppressions.
- Directory patterns should always use a forward slash `/` symbol. Windows for some reasons encourages backslash `\` but parses `/` just fine, all other OS's exclusively use forward `/`. Therefore, use `/` for safe compatibility.

See **Sample 1** below for reference on basics.

### Various Specifics

- **Private fields are a pain:** do not use them. Use other organizational strategies.
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

- **Clear but concise naming:** all names given (namespace, type, field, method, local variable, anything else) should be clean/clear/readable.
    - For example, `static Path CalculatePath(Location start, Location end)` is great naming, whereas `static List<Loc> CalcTo(Loc a, Loc b)` is not!
    - Short names (especially single-letter names like `x`) should be avoided except where they are sufficiently clear alone. For example, a `Location` class would probably have fields `X`, `Y`, and `Z`, and that's sufficiently clear there. Additionally, `for` loops often use variable `i`, which generally is fine.
    - At the same time, horribly long names (like `BeginToGoSoFarAsToSeeMoreLikeTheCalculationOfTheMethodThatThuslyFollows`) should be avoided for conciseness reasons: put the long description in the summary documentation slot, not the method name.
    - In general, a name should describe exactly what it represents, and nothing more, without being repetitive.
    - A name including its own type should be used only in cases where the type is a critical point in what it is. For example, a `ConvertToLocation` method might have input parameter names like `inputString` on the grounds that the type is the critical information included.

- **Globalize by default, localize manually:** Microsoft tends to prefer to Localize by default, which leads to issues.
    - "**Globalize**" in this context means to write code that works the same no matter where you run it. It is globally compatible.
    - "**Localize**" in this context means to write code that changes depending on what country you're in.
        - An example of a Localized method is the default C# `ToLower`/`ToUpper` methods - if you use `ToUpper` while in the country *Turkey*, the letter `i` gets capitalized to a different unicode symbol than in any other country (the Turkish capital `I` is different from the Latin capital `I`).
            - This means in practice that `if ("example i".ToUpper() == "EXAMPLE I")` returns `true` most of the time, but `false` if you're in Turkey.
                - Or for a more realistic example, say you have a `Dictionary<String, SomeType> MyMap` where the keys are uppercase words such as `DIAMOND`, and you have a user-input field that runs something like `MyMap.TryGetValue(userInput.ToUpper(), out SomeType x)` a user can enter `diamond` to get the diamond object, only if they don't live in Turkey.
        - For another example: `Float#ToString()` usually uses `.` as the decimal point, but in some countries by default returns `,` as a decimal separator. So in the US `3.2` is formatted as `3.2`, but in Thailand `3.2` is formatted as `3,2`. `Float.Parse(String x)` does not process the difference.
            - Therefore, `Float.Parse((3.2f).ToString())` returns an instant error in Thailand. This is, obviously, unacceptable.
                - It is left as an exercise for the reader to attempt to decipher what was possibly going through Microsoft's heads to decide that any part of this is okay.
    - Reasons that localized code can break:
        - A: As per the `Float` example above, localized code can produced data incompatible with globalized code, and thus conflict and break. If all code is globalized, this can't happen.
        - B: For many programs it is common to share data, eg via config files, network connections, or even users copy/pasting values over chatrooms. Localized data is incompatible with other locales. Globalized data works anywhere.
        - C: Even "non data" gets shared. Consider an Exception message - if the message is translated to the end-users local language, that means the error can't be reported to the developers in way the developer understands. The end-user doesn't need to read exception messages, the developer does.
            - Once again it is left to the reader to decide why Microsoft thought it made any sense to localize Exception messages by default to the end-user locale instead of assembly creator's locale.
    - Reasons to localize:
        - Exclusively when outputting display text to the end-user for the end-users consumption. When the end-user needs to understand the information directly, then and only then should it be translated to their locale.
    - Additional justification for global-by-default:
        - If you still aren't convinced that you need to be globalized by default, I ask you which of the two scenarios would you rather encounter as a developer?:
            - 1: You are running global-by-default. A user from a foreign country runs your program without issue, but reports to you that some messages are not clear to them. You go update your code to localize those methods, improving that user's experience.
            - 2: You are running local-by-default. A user from a foreign country runs your program and has no trouble understanding the messages, but reports to you that the program is crashing unexpectedly and they are unable to run the program at all. If you aren't quick to respond, they'll leave a negative review of your non-functional program. You investigate their report but cannot understand what went wrong because it's a screenshot of a stacktrace where the message makes no sense to you and it relates to a line of code that 'should' work fine. After a few days, followup investigation eventually discovers an edge case like the `Float.Parse/ToString` example above where methods that normally work together just don't in some locales. You fix this bug by replacing those methods, and await the next report after the user gets farther into the program and finds some new edge case.
            - The above option #2 is a real example that actually happened and is what prompted both the FreneticUtilities tooling designed to counter this issue, and this rant in the middle of a guidelines document.
    - Solutions:
        - Globalize by default. All code should execute in a globally-compatible way by default, and only use locale intentionally in cases where information is being sent directly to the end user.
        - To assist in this, FreneticUtilities provides `SpecialTools.Internationalize()` which instantly forces the process to use the global `InvariantCulture`, ensuring all Microsoft auto-localized code is globalized. This method returns a reference to the original user's locale so you can use it when needed. Call this method at the very start of your `Main` method to minimize potential issues.
            - Additionally, all FreneticUtilities tools are globalized out-of-the-box by default (excluding where they rely on Microsoft-provided methods, which are globalized if-and-only-if you use the `SpecialTools` method in advance).

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
