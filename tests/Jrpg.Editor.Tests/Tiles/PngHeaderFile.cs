using System.Buffers.Binary;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>Writes only a PNG's header: <c>PngSize</c>, and so the import's checks, read nothing past it.</summary>
internal static class PngHeaderFile
{
    /// <returns>The path written, its folder created.</returns>
    public static string Write(string path, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        byte[] header = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, (byte)'I', (byte)'H', (byte)'D', (byte)'R', 0, 0, 0, 0, 0, 0, 0, 0];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(16), width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(20), height);
        File.WriteAllBytes(path, header);
        return path;
    }
}
