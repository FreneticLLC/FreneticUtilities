FreneticDataSyntax
------------------

Frenetic Data Syntax is a data syntax designed by FreneticXYZ as a replacement for data syntaxes or object notations such as JSON or YAML.

This is a C#/.NET reference implementation of an FDS parser.

### Status

FreneticDataSyntax is in VERY EARLY development. Meaning, it's not even worth using in its present state. Please do watch/star/follow/whatever the development to see it progress!

### Some information (unordered)

- FDS supports tabs as a representative of 4 spaces.
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
- FDS **DOES NOT** currently support:
	- Lists of binary information.
	- Multi-line keys.
	- Automatic serialization of non-basic types.
	- Comments at end-of-file.
	- Files or data bigger than your RAM.
	- Empty key labels.

### Example

The following is an example of FDS syntax:

```FDS
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
	my list key:
	- 1
	- two
	- three makes it complete!
```

### Licensing pre-note:

This is an open source project, provided entirely freely, for everyone to use and contribute to.

If you make any changes that could benefit the community as a whole, please contribute upstream.

### The short of the license is:

You can do basically whatever you want, except you may not hold any developer liable for what you do with the software.

### The long version of the license follows:

The MIT License (MIT)

Copyright (c) 2016 Frenetic XYZ

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

