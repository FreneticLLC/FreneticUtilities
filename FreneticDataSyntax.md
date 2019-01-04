FreneticDataSyntax
------------------

Frenetic Data Syntax is a data syntax designed as a replacement for data syntaxes or object notations such as JSON or YAML.

This is a C#/.NET reference implementation of an FDS parser.

### Status

FreneticDataSyntax is young but capable. It fully supports reading and re-saving the example FDS file below, as well as interaction with it via API methods. Some features are untested or not yet available.

### Some information (unordered)

- FDS supports (and encourages) tabs as a representative of 4 spaces.
    - This trims a few bytes and allows users to freely control how they view an FDS file.
- FDS is neat, clean, and user friendly.
- FDS preserves list order, but not section order.
- FDS preserves preceding comments (Comments that come before a data section).
- FDS interprets quotes as raw input.
- FDS supports binary data via the `=` syntax, in Base64.
- FDS does not guess at data types and cause copy-over errors. Instead, it preserves data exactly as input, while still translating type where possible.
- FDS uses UTF-8 for data input across all machines, regardless of operating system. This ensures Unicode data is preserved.
- FDS supports simple data lists.
- FDS reads line endings properly, and outputs with the correct system line endings (except when configured to use a specific ending).
- FDS supports 32-bit integers, 64-bit integers ("longs"), 32-bit floats, 64-bit floats ("doubles"), textual strings, binary arrays.
- FDS is case sensitive, but supports case-insensitive reads.
- FDS supports newlines in text via `\n`, and backslashes via `\\`. Also available: `\r` (carriage return), `\t` (tab), `\d` (dot), `\c` (colon), `\e` (equals sign), `\x` (nothing, to allow spaces at start or end of text).
- FDS has a developer-friendly API for interfacing with FDS data and data sections.
- FDS has a developer-unfriendly secondary API for interfacing with raw underlying data as well.
- FDS **DOES NOT** currently support:
    - Lists of binary information.
    - Multi-line keys.
    - Automatic serialization of non-basic types.
    - Comments at end-of-file.
    - Files or data bigger than your RAM.
    - Empty key labels.
    - Lists/maps inside of lists

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
    # Wrap up with more text.
    - three makes it complete!
```
