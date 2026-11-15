namespace Jrpg.Editor.Tests.GifImport;

/// <summary>
/// Writes tiny GIFs byte by byte for the reader's tests: a four-color global palette (0 black, 1 red, 2 green, 3 blue)
/// and each frame's indices LZW-coded with a clear code before every pixel, so the code table never grows and every
/// code is 3 bits: the plainest stream a decoder must read.
/// </summary>
internal static class GifFixtures
{
    /// <summary>
    /// The tests that convert a GIF or count the strips' temporary folders run one after another, so a count sees no
    /// other test's folder.
    /// </summary>
    public const string StripFoldersCollection = "GIF strip folders";

    private const int ClearCode = 4;
    private const int EndCode = 5;
    private const int CodeBits = 3;
    private const int MaxSubBlock = 255;

    private static readonly byte[] _palette = [0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255];

    /// <returns>The path written.</returns>
    public static string Write(string path, int width, int height, params FixtureFrame[] frames)
    {
        using var bytes = new MemoryStream();
        bytes.Write("GIF89a"u8);
        WriteShort(bytes, width);
        WriteShort(bytes, height);
        bytes.Write([0x81, 0, 0]);
        bytes.Write(_palette);
        foreach (var frame in frames)
        {
            WriteFrame(bytes, frame);
        }

        bytes.WriteByte(0x3B);
        File.WriteAllBytes(path, bytes.ToArray());
        return path;
    }

    private static void WriteFrame(MemoryStream bytes, FixtureFrame frame)
    {
        if (frame.DelayCentiseconds is { } delay)
        {
            var packed = (frame.Disposal << 2) | (frame.TransparentColor is null ? 0 : 1);
            bytes.Write([0x21, 0xF9, 4, (byte)packed]);
            WriteShort(bytes, delay);
            bytes.Write([frame.TransparentColor ?? 0, 0]);
        }

        bytes.WriteByte(0x2C);
        WriteShort(bytes, frame.Left);
        WriteShort(bytes, frame.Top);
        WriteShort(bytes, frame.Width);
        WriteShort(bytes, frame.Height);
        bytes.Write([0, 2]);
        var coded = Coded(frame.Colors);
        for (var start = 0; start < coded.Length; start += MaxSubBlock)
        {
            var length = Math.Min(MaxSubBlock, coded.Length - start);
            bytes.WriteByte((byte)length);
            bytes.Write(coded, start, length);
        }

        bytes.WriteByte(0);
    }

    // Codes packed from the lowest bit up, as GIF's LZW stores them.
    private static byte[] Coded(byte[] colors)
    {
        var codes = colors.SelectMany(color => new[] { ClearCode, color }).Append(EndCode).ToList();
        var packed = new byte[((codes.Count * CodeBits) + 7) / 8];
        for (var index = 0; index < codes.Count; index++)
        {
            for (var bit = 0; bit < CodeBits; bit++)
            {
                var at = (index * CodeBits) + bit;
                packed[at / 8] |= (byte)(((codes[index] >> bit) & 1) << (at % 8));
            }
        }

        return packed;
    }

    private static void WriteShort(MemoryStream bytes, int value) => bytes.Write([(byte)(value & 0xFF), (byte)(value >> 8)]);
}
