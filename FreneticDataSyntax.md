FreneticDataSyntax
------------------

Frenetic Data Syntax is a data syntax designed as a replacement for data syntaxes or object notations such as JSON or YAML.

This is a C#/.NET reference implementation of an FDS parser.

### Status

FreneticDataSyntax is young but capable. It fully supports reading and re-saving the example FDS file below, as well as interaction with it via API methods. Most features are tested and working.

### Some information (unordered)

- FDS supports (and encourages) tabs as a representative of 4 spaces.
    - This trims a few bytes and allows users to freely control how they view an FDS file.
- FDS is neat, clean, and user friendly.
- FDS preserves list and section key order.
- FDS preserves comments in their positions based on what data follows (comments follow what's below them in the file. Comments at the end of the file remain at the end).
- FDS interprets quotes as raw input (ie they are treated as just textual quotes, no special handling is applied).
- FDS supports binary data via the `=` syntax, in Base64.
- FDS does not guess at data types and cause copy-over errors. Instead, it preserves data exactly as input, while still translating type where possible
    - Eg, input of "3.5" will be read as double with value 3.5, but "3.50" will not be, as the double value will output "3.5", which is an inconsistent copy-over error.
- FDS uses UTF-8 for data input across all machines, regardless of operating system. This ensures Unicode data is preserved.
- FDS supports data lists.
- FDS supports maps or lists inside of lists via the `>` syntax.
- FDS reads any line endings properly (for Windows, old-Mac, and Unix file endings), and outputs with the correct standard line endings (single-newline, Unix-like) except when configured to use a specific ending.
- FDS explicitly supports 32-bit integers, 64-bit integers ("longs"), 32-bit floats, 64-bit floats ("doubles"), booleans, textual strings, binary arrays.
- FDS implicitly supports other types, particularly when they can be naturally encoded as a string (such as `decimal`, or just any type that you define your own string encode/decode for).
- FDS is case sensitive, but supports case-insensitive reads.
- FDS supports newlines in text via `\n`, and backslashes via `\s`. Also available: `\r` (carriage return), `\t` (tab), `\d` (dot), `\c` (colon), `\e` (equals sign), `\x` (nothing, to allow spaces at start or end of text).
- FDS has a developer-friendly API for interfacing with FDS data and data sections.
- FDS has a developer-unfriendly secondary API for interfacing with raw underlying data as well.
- FDS **DOES NOT** support:
    - Multi-line keys.
    - Automatic serialization of non-basic types.
    - Files or data bigger than your RAM.

### Example

The following is an example of FDS syntax:

```fds
# MyFDSFile.fds
my root section 1:
	# This represents some data.
	my_sub_section:
		# The key is set with a value of 3.
		my numeric key: 3
		my decimal number key: 3.14159
	my other section:
		my first string key: alpha
		my second string key: Wow what a system!
my second root section:
	# contains UTF-8 text: Hello world, and all who inhabit it!
	my binary key= SGVsbG8gd29ybGQsIGFuZCBhbGwgd2hvIGluaGFiaXQgaXQh
	# This is a list.
	my list key:
	# This will be correct integer type.
	- 1
	# This will be text.
	- two
	# This will be boolean. 'false' is the only other valid boolean value.
	- true
	# A binary entry in the list.
	= SGVsbG8gd29ybGQsIGFuZCBhbGwgd2hvIGluaGFiaXQgaXQh
	# Wrap up with more text.
	- three makes it complete!
this key is empty:
my third root section:
	contains a list:
	- with a text value
	# This symbol indicates a list entry that is itself a list or map
	>	- and a sublist
		- with two values
	# And the content within can be any list prefix, - or =
	>	= SGVsbG8gd29ybGQsIGFuZCBhbGwgd2hvIGluaGFiaXQgaXQh
	>	- and a sublist
		>	- with a subsublist
			- with also two values
		# Or a mapping of any form
		>	and a submap:
			- to a sublist
		>	and another submap:
			now to a subsubmap:
			- which is now a list
			and a subsubmap:
				that is a subsubsubmap: with a subval
		>	and a key: to value
			direct: mapping
		>	one key: one value
		>	one nowhere key:
# That's all, folks!
```

### AutoConfiguration

FDS also includes the `AutoConfiguration` class, which automatically converts between FDS Sections and C# class objects.

Example of an `AutoConfiguration` class:
```cs
    class TestConfig : AutoConfiguration
    {
        public bool BoValue = true;

        [ConfigComment("Wow!\nWhat a comment!")]
        public int NumVal = 5;

        public float ValF = 7.2f;

        public string Text = "Wow\nText here!";

        public List<string> StringList = new() { "alpha", "bravo string", "string number three" };

        public List<int> IntList = new() { 1, 7, 12, 42, 96 };

        public List<short> ShortList = new() { 5 };

        public class SubClass : AutoConfiguration
        {
            public sbyte WeirdData = -5;

            [ConfigComment("This encodes as a binary key!")]
            public byte[] WeirdArray = new byte[] { 7, 12, 42 };
        }

        public SubClass SubData = new();
    }
```

That will auto-encode as:

```fds
BoValue: true
#Wow!
#What a comment!
NumVal: 5
ValF: 7.2
Text: Wow\nText here!
StringList:
- alpha
- bravo string
- string number three
IntList:
- 1
- 7
- 12
- 42
- 96
ShortList:
- 5
SubData:
    WeirdData: -5
    #This encodes as a binary key!
    WeirdArray= Bwwq
```

- Note that all default values should be set and non-null.
    - Lists for example should be empty lists (like `new List<Type>();`), not `null`.
- Supports:
    - `AutoConfiguration` sub-instances (that is, configs within configs, to be sub-mapped)
    - Basic types: `string`, `bool`
    - Signed integer types: `long`, `int`, `short`, `sbyte`
    - Unsigned integer types: `ulong`, `uint`, `ushort`, `byte`
    - Floating point types: `float`, `double`, `decimal`
    - Generic types: `ICollection<T>`
        - Can have any collection type (`List<T>`, `LinkedList<T>`, etc.) so long as it supports a no-argument constructor and extends `ICollection<T>`.
        - Note that the variable type should match the real type (ie don't use an interface as the actual variable type, use a concrete type).
        - Can have any supported type as `T`.
