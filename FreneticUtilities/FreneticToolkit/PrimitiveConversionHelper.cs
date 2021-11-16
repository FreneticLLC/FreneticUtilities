//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>Helper class for primitive value conversions.</summary>
    public static class PrimitiveConversionHelper
    {
        #region helpers
        /// <summary>Helper for filling a 16-bit byte union helper struct section from byte array input.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyInputBytes16(out ByteUnionBase16 unionBytes, byte[] inputBytes) // TODO: Span<byte>?
        {
            unchecked
            {
                unionBytes.Byte1Value = inputBytes[1]; // Read highest value first to help compiler optimize correctly (don't need to check size twice)
                unionBytes.Byte0Value = inputBytes[0];
            }
        }

        /// <summary>Helper for filling a 16-bit byte union helper struct section from byte array input.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyInputBytes16(out ByteUnionBase16 unionBytes, byte[] inputBytes, int offset)
        {
            // TODO: properly help compiler avoid checking if 'offset' is negative?
            unchecked
            {
                unionBytes.Byte1Value = inputBytes[offset + 1]; // Read highest value first to help compiler optimize correctly (don't need to check size twice)
                unionBytes.Byte0Value = inputBytes[offset + 0];
            }
        }

        /// <summary>Helper for filling an array of 2 bytes from a 16-bit byte union helper struct section.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyOutputBytes16(in ByteUnionBase16 unionBytes, byte[] outputBytes, int offset)
        {
            // TODO: properly help compiler avoid checking if 'offset' is negative?
            unchecked
            {
                outputBytes[offset + 1] = unionBytes.Byte1Value; // Write highest value first to help compiler optimize correctly (don't need to check size twice)
                outputBytes[offset + 0] = unionBytes.Byte0Value;
            }
        }

        /// <summary>
        /// Helper to create a byte array from a 16-bit byte union helper struct section.
        /// Returns 2 bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetOutputBytes16(in ByteUnionBase16 unionBytes)
        {
            return new byte[]
            {
                unionBytes.Byte0Value,
                unionBytes.Byte1Value
            };
        }

        /// <summary>Helper for filling a 32-bit byte union helper struct section from byte array input.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyInputBytes32(out ByteUnionBase32 unionBytes, byte[] inputBytes) // TODO: Span<byte>?
        {
            unchecked
            {
                unionBytes.Byte3Value = inputBytes[3]; // Read highest value first to help compiler optimize correctly (don't need to check size 4 times)
                unionBytes.Byte2Value = inputBytes[2];
                unionBytes.Byte1Value = inputBytes[1];
                unionBytes.Byte0Value = inputBytes[0];
            }
        }

        /// <summary>Helper for filling a 32-bit byte union helper struct section from byte array input.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyInputBytes32(out ByteUnionBase32 unionBytes, byte[] inputBytes, int offset)
        {
            // TODO: properly help compiler avoid checking if 'offset' is negative?
            unchecked
            {
                unionBytes.Byte3Value = inputBytes[offset + 3]; // Read highest value first to help compiler optimize correctly (don't need to check size 4 times)
                unionBytes.Byte2Value = inputBytes[offset + 2];
                unionBytes.Byte1Value = inputBytes[offset + 1];
                unionBytes.Byte0Value = inputBytes[offset + 0];
            }
        }

        /// <summary>Helper for filling an array of 4 bytes from a 32-bit byte union helper struct section.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyOutputBytes32(in ByteUnionBase32 unionBytes, byte[] outputBytes, int offset)
        {
            // TODO: properly help compiler avoid checking if 'offset' is negative?
            unchecked
            {
                outputBytes[offset + 3] = unionBytes.Byte3Value; // Write highest value first to help compiler optimize correctly (don't need to check size 4 times)
                outputBytes[offset + 2] = unionBytes.Byte2Value;
                outputBytes[offset + 1] = unionBytes.Byte1Value;
                outputBytes[offset + 0] = unionBytes.Byte0Value;
            }
        }

        /// <summary>
        /// Helper to create a byte array from a 32-bit byte union helper struct section.
        /// Returns 4 bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetOutputBytes32(in ByteUnionBase32 unionBytes)
        {
            return new byte[]
            {
                unionBytes.Byte0Value,
                unionBytes.Byte1Value,
                unionBytes.Byte2Value,
                unionBytes.Byte3Value
            };
        }

        /// <summary>Helper for filling a 64-bit byte union helper struct section from byte array input.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyInputBytes64(out ByteUnionBase64 unionBytes, byte[] inputBytes) // TODO: Span<byte>?
        {
            unchecked
            {
                unionBytes.Byte7Value = inputBytes[7]; // Read highest value first to help compiler optimize correctly (don't need to check size 8 times)
                unionBytes.Byte6Value = inputBytes[6];
                unionBytes.Byte5Value = inputBytes[5];
                unionBytes.Byte4Value = inputBytes[4];
                unionBytes.Byte3Value = inputBytes[3];
                unionBytes.Byte2Value = inputBytes[2];
                unionBytes.Byte1Value = inputBytes[1];
                unionBytes.Byte0Value = inputBytes[0];
            }
        }

        /// <summary>Helper for filling a 64-bit byte union helper struct section from byte array input.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyInputBytes64(out ByteUnionBase64 unionBytes, byte[] inputBytes, int offset)
        {
            // TODO: properly help compiler avoid checking if 'offset' is negative?
            unchecked
            {
                unionBytes.Byte7Value = inputBytes[offset + 7]; // Read highest value first to help compiler optimize correctly (don't need to check size 8 times)
                unionBytes.Byte6Value = inputBytes[offset + 6];
                unionBytes.Byte5Value = inputBytes[offset + 5];
                unionBytes.Byte4Value = inputBytes[offset + 4];
                unionBytes.Byte3Value = inputBytes[offset + 3];
                unionBytes.Byte2Value = inputBytes[offset + 2];
                unionBytes.Byte1Value = inputBytes[offset + 1];
                unionBytes.Byte0Value = inputBytes[offset + 0];
            }
        }

        /// <summary>Helper for filling an array of 8 bytes from a 64-bit byte union helper struct section.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyOutputBytes64(in ByteUnionBase64 unionBytes, byte[] outputBytes, int offset)
        {
            // TODO: properly help compiler avoid checking if 'offset' is negative?
            unchecked
            {
                outputBytes[offset + 7] = unionBytes.Byte7Value; // Write highest value first to help compiler optimize correctly (don't need to check size 8 times)
                outputBytes[offset + 6] = unionBytes.Byte6Value;
                outputBytes[offset + 5] = unionBytes.Byte5Value;
                outputBytes[offset + 4] = unionBytes.Byte4Value;
                outputBytes[offset + 3] = unionBytes.Byte3Value;
                outputBytes[offset + 2] = unionBytes.Byte2Value;
                outputBytes[offset + 1] = unionBytes.Byte1Value;
                outputBytes[offset + 0] = unionBytes.Byte0Value;
            }
        }

        /// <summary>
        /// Helper to create a byte array from a 64-bit byte union helper struct section.
        /// Returns 8 bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetOutputBytes64(in ByteUnionBase64 unionBytes)
        {
            return new byte[]
            {
                unionBytes.Byte0Value,
                unionBytes.Byte1Value,
                unionBytes.Byte2Value,
                unionBytes.Byte3Value,
                unionBytes.Byte4Value,
                unionBytes.Byte5Value,
                unionBytes.Byte6Value,
                unionBytes.Byte7Value
            };
        }
        #endregion

        // Note for implementations below:
        // each builds the Union with 'default' just to get rid of compiler unassigned error (as compiler doesn't understand union setup)

        #region short16
        /// <summary>
        /// Converts a byte array to a 16-bit signed integer.
        /// Input must contain 2 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output short16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short BytesToShort16(byte[] inputBytes)
        {
            unchecked
            {
                Short16ByteUnion unionHelper = default;
                CopyInputBytes16(out unionHelper.Bytes, inputBytes);
                return unionHelper.Short16Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 16-bit signed integer.
        /// Input must contain 2 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output short16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short BytesToShort16(byte[] inputBytes, int offset)
        {
            unchecked
            {
                Short16ByteUnion unionHelper = default;
                CopyInputBytes16(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.Short16Value;
            }
        }

        /// <summary>
        /// Converts a 16-bit signed integer to a byte array.
        /// Output contains 2 bytes.
        /// </summary>
        /// <param name="inputShort16">The input short16.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Short16ToBytes(short inputShort16)
        {
            unchecked
            {
                Short16ByteUnion unionHelper = default;
                unionHelper.Short16Value = inputShort16;
                return GetOutputBytes16(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 16-bit signed integer to a byte array.
        /// Fills 2 bytes.
        /// </summary>
        /// <param name="inputShort16">The input short16.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Short16ToBytes(short inputShort16, byte[] outputBytes, int offset)
        {
            unchecked
            {
                Short16ByteUnion unionHelper = default;
                unionHelper.Short16Value = inputShort16;
                CopyOutputBytes16(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region ushort16
        /// <summary>
        /// Converts a byte array to a 16-bit unsigned integer.
        /// Input must contain 2 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output ushort16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort BytesToUShort16(byte[] inputBytes)
        {
            unchecked
            {
                UShort16ByteUnion unionHelper = default;
                CopyInputBytes16(out unionHelper.Bytes, inputBytes);
                return unionHelper.UShort16Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 16-bit unsigned integer.
        /// Input must contain 2 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output ushort16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort BytesToUShort16(byte[] inputBytes, int offset)
        {
            unchecked
            {
                UShort16ByteUnion unionHelper = default;
                CopyInputBytes16(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.UShort16Value;
            }
        }

        /// <summary>
        /// Converts a 16-bit unsigned integer to a byte array.
        /// Output contains 2 bytes.
        /// </summary>
        /// <param name="inputUShort16">The input ushort16.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] UShort16ToBytes(ushort inputUShort16)
        {
            unchecked
            {
                UShort16ByteUnion unionHelper = default;
                unionHelper.UShort16Value = inputUShort16;
                return GetOutputBytes16(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 16-bit unsigned integer to a byte array.
        /// Fills 2 bytes.
        /// </summary>
        /// <param name="inputUShort16">The input ushort16.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UShort16ToBytes(ushort inputUShort16, byte[] outputBytes, int offset)
        {
            unchecked
            {
                UShort16ByteUnion unionHelper = default;
                unionHelper.UShort16Value = inputUShort16;
                CopyOutputBytes16(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region int32
        /// <summary>
        /// Converts a byte array to a 32-bit signed integer.
        /// Input must contain 4 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output int32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BytesToInt32(byte[] inputBytes)
        {
            unchecked
            {
                Int32ByteUnion unionHelper = default;
                CopyInputBytes32(out unionHelper.Bytes, inputBytes);
                return unionHelper.Int32Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 32-bit signed integer.
        /// Input must contain 4 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output int32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BytesToInt32(byte[] inputBytes, int offset)
        {
            unchecked
            {
                Int32ByteUnion unionHelper = default;
                CopyInputBytes32(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.Int32Value;
            }
        }

        /// <summary>
        /// Converts a 32-bit signed integer to a byte array.
        /// Output contains 4 bytes.
        /// </summary>
        /// <param name="inputInt32">The input int32.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Int32ToBytes(int inputInt32)
        {
            unchecked
            {
                Int32ByteUnion unionHelper = default;
                unionHelper.Int32Value = inputInt32;
                return GetOutputBytes32(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 32-bit signed integer to a byte array.
        /// Fills 4 bytes.
        /// </summary>
        /// <param name="inputInt32">The input int32.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Int32ToBytes(int inputInt32, byte[] outputBytes, int offset)
        {
            unchecked
            {
                Int32ByteUnion unionHelper = default;
                unionHelper.Int32Value = inputInt32;
                CopyOutputBytes32(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region uint32
        /// <summary>
        /// Converts a byte array to a 32-bit unsigned integer.
        /// Input must contain 4 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output uint32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint BytesToUInt32(byte[] inputBytes)
        {
            unchecked
            {
                UInt32ByteUnion unionHelper = default;
                CopyInputBytes32(out unionHelper.Bytes, inputBytes);
                return unionHelper.UInt32Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 32-bit unsigned integer.
        /// Input must contain 4 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output uint32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint BytesToUInt32(byte[] inputBytes, int offset)
        {
            unchecked
            {
                UInt32ByteUnion unionHelper = default;
                CopyInputBytes32(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.UInt32Value;
            }
        }

        /// <summary>
        /// Converts a 32-bit unsigned integer to a byte array.
        /// Output contains 4 bytes.
        /// </summary>
        /// <param name="inputUInt32">The input uint32.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] UInt32ToBytes(uint inputUInt32)
        {
            unchecked
            {
                UInt32ByteUnion unionHelper = default;
                unionHelper.UInt32Value = inputUInt32;
                return GetOutputBytes32(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 32-bit unsigned integer to a byte array.
        /// Fills 4 bytes.
        /// </summary>
        /// <param name="inputUInt32">The input uint32.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UInt32ToBytes(uint inputUInt32, byte[] outputBytes, int offset)
        {
            unchecked
            {
                UInt32ByteUnion unionHelper = default;
                unionHelper.UInt32Value = inputUInt32;
                CopyOutputBytes32(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region long64
        /// <summary>
        /// Converts a byte array to a 64-bit signed integer.
        /// Input must contain 8 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output long64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long BytesToLong64(byte[] inputBytes)
        {
            unchecked
            {
                Long64ByteUnion unionHelper = default;
                CopyInputBytes64(out unionHelper.Bytes, inputBytes);
                return unionHelper.Long64Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 64-bit signed integer.
        /// Input must contain 8 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output long64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long BytesToLong64(byte[] inputBytes, int offset)
        {
            unchecked
            {
                Long64ByteUnion unionHelper = default;
                CopyInputBytes64(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.Long64Value;
            }
        }

        /// <summary>
        /// Converts a 64-bit signed integer to a byte array.
        /// Output contains 8 bytes.
        /// </summary>
        /// <param name="inputLong64">The input long64.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Long64ToBytes(long inputLong64)
        {
            unchecked
            {
                Long64ByteUnion unionHelper = default;
                unionHelper.Long64Value = inputLong64;
                return GetOutputBytes64(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 64-bit signed integer to a byte array.
        /// Fills 8 bytes.
        /// </summary>
        /// <param name="inputLong64">The input long64.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Long64ToBytes(long inputLong64, byte[] outputBytes, int offset)
        {
            unchecked
            {
                Long64ByteUnion unionHelper = default;
                unionHelper.Long64Value = inputLong64;
                CopyOutputBytes64(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region ulong64
        /// <summary>
        /// Converts a byte array to a 64-bit unsigned integer.
        /// Input must contain 8 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output ulong64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BytesToULong64(byte[] inputBytes)
        {
            unchecked
            {
                ULong64ByteUnion unionHelper = default;
                CopyInputBytes64(out unionHelper.Bytes, inputBytes);
                return unionHelper.ULong64Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 64-bit unsigned integer.
        /// Input must contain 8 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output ulong64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BytesToULong64(byte[] inputBytes, int offset)
        {
            unchecked
            {
                ULong64ByteUnion unionHelper = default;
                CopyInputBytes64(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.ULong64Value;
            }
        }

        /// <summary>
        /// Converts a 64-bit unsigned integer to a byte array.
        /// Output contains 8 bytes.
        /// </summary>
        /// <param name="inputULong64">The input ulong64.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ULong64ToBytes(ulong inputULong64)
        {
            unchecked
            {
                ULong64ByteUnion unionHelper = default;
                unionHelper.ULong64Value = inputULong64;
                return GetOutputBytes64(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 64-bit unsigned integer to a byte array.
        /// Fills 8 bytes.
        /// </summary>
        /// <param name="inputULong64">The input ulong64.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ULong64ToBytes(ulong inputULong64, byte[] outputBytes, int offset)
        {
            unchecked
            {
                ULong64ByteUnion unionHelper = default;
                unionHelper.ULong64Value = inputULong64;
                CopyOutputBytes64(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region float32
        /// <summary>
        /// Converts a byte array to a 32-bit floating point number.
        /// Input must contain 4 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output float32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BytesToFloat32(byte[] inputBytes)
        {
            unchecked
            {
                Float32ByteUnion unionHelper = default;
                CopyInputBytes32(out unionHelper.Bytes, inputBytes);
                return unionHelper.Float32Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 32-bit floating point number.
        /// Input must contain 4 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output float32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BytesToFloat32(byte[] inputBytes, int offset)
        {
            unchecked
            {
                Float32ByteUnion unionHelper = default;
                CopyInputBytes32(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.Float32Value;
            }
        }

        /// <summary>
        /// Converts a 32-bit floating point number to a byte array.
        /// Output contains 4 bytes.
        /// </summary>
        /// <param name="inputFloat32">The input float32.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Float32ToBytes(float inputFloat32)
        {
            unchecked
            {
                Float32ByteUnion unionHelper = default;
                unionHelper.Float32Value = inputFloat32;
                return GetOutputBytes32(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 32-bit floating point number to a byte array.
        /// Fills 4 bytes.
        /// </summary>
        /// <param name="inputFloat32">The input float32.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Float32ToBytes(float inputFloat32, byte[] outputBytes, int offset)
        {
            unchecked
            {
                Float32ByteUnion unionHelper = default;
                unionHelper.Float32Value = inputFloat32;
                CopyOutputBytes32(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region double64
        /// <summary>
        /// Converts a byte array to a 64-bit double-width floating point number.
        /// Input must contain 8 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The output double64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double BytesToDouble64(byte[] inputBytes)
        {
            unchecked
            {
                Double64ByteUnion unionHelper = default;
                CopyInputBytes64(out unionHelper.Bytes, inputBytes);
                return unionHelper.Double64Value;
            }
        }

        /// <summary>
        /// Converts a byte array to a 64-bit double-width floating point number.
        /// Input must contain 8 bytes.
        /// </summary>
        /// <param name="inputBytes">The input bytes.</param>
        /// <param name="offset">The starting offset within the input bytes.</param>
        /// <returns>The output double64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double BytesToDouble64(byte[] inputBytes, int offset)
        {
            unchecked
            {
                Double64ByteUnion unionHelper = default;
                CopyInputBytes64(out unionHelper.Bytes, inputBytes, offset);
                return unionHelper.Double64Value;
            }
        }

        /// <summary>
        /// Converts a 64-bit double-width floating point number to a byte array.
        /// Output contains 8 bytes.
        /// </summary>
        /// <param name="inputDouble64">The input double64.</param>
        /// <returns>The output byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Double64ToBytes(double inputDouble64)
        {
            unchecked
            {
                Double64ByteUnion unionHelper = default;
                unionHelper.Double64Value = inputDouble64;
                return GetOutputBytes64(unionHelper.Bytes);
            }
        }

        /// <summary>
        /// Converts a 64-bit double-width floating point number to a byte array.
        /// Fills 8 bytes.
        /// </summary>
        /// <param name="inputDouble64">The input double64.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the byte array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Double64ToBytes(double inputDouble64, byte[] outputBytes, int offset)
        {
            unchecked
            {
                Double64ByteUnion unionHelper = default;
                unionHelper.Double64Value = inputDouble64;
                CopyOutputBytes64(in unionHelper.Bytes, outputBytes, offset);
            }
        }
        #endregion

        #region inter-conversion
        /// <summary>Converts a 64-bit unsigned long integer to a 64-bit double-width floating point number.</summary>
        /// <param name="inputUlong">The input ulong.</param>
        /// <returns>The output double64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ULongToDouble64(ulong inputUlong)
        {
            unchecked
            {
                Double64UlongUnion unionHelper = default;
                unionHelper.ULong64 = inputUlong;
                return unionHelper.Double64Value;
            }
        }

        /// <summary>Converts a 64-bit double-width floating point number to a 64-bit unsigned long integer.</summary>
        /// <param name="inputDouble">The input double64.</param>
        /// <returns>The output ulong64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Double64ToULong(double inputDouble)
        {
            unchecked
            {
                Double64UlongUnion unionHelper = default;
                unionHelper.Double64Value = inputDouble;
                return unionHelper.ULong64;
            }
        }
        #endregion

#pragma warning disable 1591
        #region union structs
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
        public struct ByteUnionBase16
        {
            [FieldOffset(0)]
            public byte Byte0Value;

            [FieldOffset(1)]
            public byte Byte1Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        public struct ByteUnionBase32
        {
            [FieldOffset(0)]
            public byte Byte0Value;

            [FieldOffset(1)]
            public byte Byte1Value;

            [FieldOffset(2)]
            public byte Byte2Value;

            [FieldOffset(3)]
            public byte Byte3Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct ByteUnionBase64
        {
            [FieldOffset(0)]
            public byte Byte0Value;

            [FieldOffset(1)]
            public byte Byte1Value;

            [FieldOffset(2)]
            public byte Byte2Value;

            [FieldOffset(3)]
            public byte Byte3Value;

            [FieldOffset(4)]
            public byte Byte4Value;

            [FieldOffset(5)]
            public byte Byte5Value;

            [FieldOffset(6)]
            public byte Byte6Value;

            [FieldOffset(7)]
            public byte Byte7Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
        public struct Short16ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase16 Bytes;

            [FieldOffset(0)]
            public short Short16Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
        public struct UShort16ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase16 Bytes;

            [FieldOffset(0)]
            public ushort UShort16Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        public struct Int32ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase32 Bytes;

            [FieldOffset(0)]
            public int Int32Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        public struct UInt32ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase32 Bytes;

            [FieldOffset(0)]
            public uint UInt32Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct Long64ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase64 Bytes;

            [FieldOffset(0)]
            public long Long64Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct ULong64ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase64 Bytes;

            [FieldOffset(0)]
            public ulong ULong64Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        public struct Float32ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase32 Bytes;

            [FieldOffset(0)]
            public float Float32Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct Double64ByteUnion
        {
            [FieldOffset(0)]
            public ByteUnionBase64 Bytes;

            [FieldOffset(0)]
            public double Double64Value;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct Double64UlongUnion
        {
            [FieldOffset(0)]
            public ulong ULong64;

            [FieldOffset(0)]
            public double Double64Value;
        }
        #endregion
#pragma warning restore 1591
    }
}
