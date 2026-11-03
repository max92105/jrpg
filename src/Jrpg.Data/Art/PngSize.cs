using System.Buffers.Binary;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>Reads an image's size from its PNG header, so art can be checked without a graphics device.</summary>
public static class PngSize
{
    // The 8-byte signature, then the IHDR chunk's length and type, then its width and height (PNG spec, section 5 and 11.2.2).
    private const int HeaderLength = 24;

    private static ReadOnlySpan<byte> Signature => [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A];

    /// <exception cref="ContentValidationException">The file isn't a PNG image.</exception>
    public static (int Width, int Height) Read(string path)
    {
        Span<byte> header = stackalloc byte[HeaderLength];
        int length;
        using (var stream = File.OpenRead(path))
        {
            length = stream.ReadAtLeast(header, HeaderLength, throwOnEndOfStream: false);
        }

        if (length < HeaderLength || !header[..8].SequenceEqual(Signature) || !header[12..16].SequenceEqual("IHDR"u8))
        {
            throw new ContentValidationException($"{Path.GetFileName(path)}: not a PNG image.");
        }

        return (BinaryPrimitives.ReadInt32BigEndian(header[16..20]), BinaryPrimitives.ReadInt32BigEndian(header[20..24]));
    }

    /// <summary>
    /// Whether the file is there and its PNG header reads: the one answer the editor's sheet box and its drawing share,
    /// so a copy cut short or another format renamed counts as missing in both (decision 442).
    /// </summary>
    public static bool IsReadable(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            Read(path);
            return true;
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
