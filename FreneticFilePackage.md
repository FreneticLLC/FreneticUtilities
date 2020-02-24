FreneticFilePackage
-------------------

Frenetic File Packages are files that contain multiple other files within the main package file, in a way similar to how ".zip" files work.

## General Information

- Frenetic File Package files use the `.ffp` file extensions.
- FFP files are specifically optimized for fast reading.
    - Faster than zip files as FFP files do not spend significant CPU resources on decompression (except where the file space advantage is significant enough to justify it).
        - A **lot** faster. Initial testing on packaged image files (that wouldn't receive a benefit from zip, and so FFP stores them raw) showed about 30x faster read times.
    - Faster than raw file directories as common operating systems use file-systems that add significant time and complexity to reading separate files (such as fragmentation issues, slow tree traversals, large padding widths, file lock acquisition handling, ...).
- FFP files are handled by preloading the general file header data into memory (ie, the file names, positions, etc.) and maintaining a file system lock on the package, to allow quickly reading data from held files as needed.

## Format Definition

A `.ffp` file consists of the FFP HEADER followed by a tight array of all file data (positional data of files within this array is handled by the header).

**FFP HEADER FORMAT**:
- 3 bytes: ASCII 'FFP'
- 3 bytes: ASCII version label (currently, '001'). Must be ASCII numbers.
- int32: number of contained files
- Tight array of FILE HEADER

**FILE HEADER FORMAT**:
- int64: file start position (relative to end of header)
- int64: file length (in stream)
- byte: encoding mode (see FFPEncoding enumeration)
- int64: actual file length (after decompression)
- int32: length of file name (in bytes)
- UTF-8 string: file name
