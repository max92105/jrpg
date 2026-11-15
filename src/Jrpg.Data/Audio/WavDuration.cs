using System.Buffers.Binary;
using System.Text;

namespace Jrpg.Data.Audio;

/// <summary>
/// A WAV file's playing length from its header, for the Cinematics window's timing line (decision 620); the game never
/// reads it. The RIFF chunks are walked, others skipped, until the <c>fmt </c> chunk's byte rate and the <c>data</c>
/// chunk's size are both known.
/// </summary>
public static class WavDuration
{
    // RIFF + size + WAVE, then each chunk's 4-byte id and 4-byte size.
    private const int RiffHeaderLength = 12;
    private const int ChunkHeaderLength = 8;

    // The byte rate sits after the format tag, the channel count and the sample rate.
    private const int ByteRateOffset = 8;

    /// <returns>
    /// The length, or null for a file that isn't a readable RIFF WAVE with both chunks: the developer's file never
    /// makes the window throw, and an I/O error reads as unknown too, since the window only shows text.
    /// </returns>
    public static TimeSpan? Read(string path)
    {
        try
        {
            using var file = File.OpenRead(path);
            using var reader = new BinaryReader(file);
            return IsRiffWave(reader.ReadBytes(RiffHeaderLength)) ? LengthFromChunks(reader, file.Length) : null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool IsRiffWave(byte[] header) =>
        header.Length == RiffHeaderLength
        && Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
        && Encoding.ASCII.GetString(header, 8, 4) == "WAVE";

    private static TimeSpan? LengthFromChunks(BinaryReader reader, long fileLength)
    {
        uint? byteRate = null;
        uint? dataSize = null;
        while (reader.BaseStream.Position <= fileLength - ChunkHeaderLength && (byteRate is null || dataSize is null))
        {
            var header = reader.ReadBytes(ChunkHeaderLength);
            var id = Encoding.ASCII.GetString(header, 0, 4);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4));
            var start = reader.BaseStream.Position;
            if (id == "fmt " && size >= ByteRateOffset + 4)
            {
                reader.BaseStream.Position = start + ByteRateOffset;
                byteRate = reader.ReadUInt32();
            }
            else if (id == "data")
            {
                dataSize = size;
            }

            // Chunks are padded to an even size.
            reader.BaseStream.Position = start + size + (size % 2);
        }

        return byteRate is > 0 && dataSize is { } bytes ? TimeSpan.FromSeconds((double)bytes / byteRate.Value) : null;
    }
}
