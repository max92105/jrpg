using System.Buffers.Binary;

namespace Jrpg.Game.Rendering;

/// <summary>
/// Whether a font file's bytes hold a whole TrueType table directory. FontStashSharp's rasterizer reads the tables
/// through unsafe pointers without bounds checks, so a cut or foreign file can crash the process instead of throwing;
/// this check refuses such a file first.
/// </summary>
/// <remarks>
/// Layout from the OpenType spec, "Table directory" (https://learn.microsoft.com/typography/opentype/spec/otff),
/// big-endian: sfnt version (4 bytes), numTables (2), three search fields (6), then one 16-byte record per table: tag,
/// checksum, offset, length (4 each).
/// </remarks>
internal static class FontFileHeader
{
    // Only TrueType outlines: the content's one font is a .ttf, and CFF ("OTTO") isn't needed.
    private const uint TrueTypeVersion = 0x00010000;
    private const int NumTablesOffset = 4;
    private const int DirectoryHeaderLength = 12;
    private const int TableRecordLength = 16;
    private const int RecordOffsetField = 8;
    private const int RecordLengthField = 12;

    public static bool IsComplete(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < DirectoryHeaderLength || BinaryPrimitives.ReadUInt32BigEndian(bytes) != TrueTypeVersion)
        {
            return false;
        }

        int tableCount = BinaryPrimitives.ReadUInt16BigEndian(bytes[NumTablesOffset..]);
        if (DirectoryHeaderLength + ((long)tableCount * TableRecordLength) > bytes.Length)
        {
            return false;
        }

        for (var table = 0; table < tableCount; table++)
        {
            if (!IsTableInside(bytes, bytes.Slice(DirectoryHeaderLength + (table * TableRecordLength), TableRecordLength)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsTableInside(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> record)
    {
        // Summed as long: offset and length are unsigned 32-bit, so their int sum could wrap below the file length.
        long offset = BinaryPrimitives.ReadUInt32BigEndian(record[RecordOffsetField..]);
        long length = BinaryPrimitives.ReadUInt32BigEndian(record[RecordLengthField..]);
        return offset + length <= bytes.Length;
    }
}
