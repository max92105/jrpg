using System.Buffers.Binary;
using System.Text;
using Jrpg.Game.Rendering;

namespace Jrpg.Game.Tests.Rendering;

public sealed class FontFileHeaderTests
{
    private const uint TrueTypeVersion = 0x00010000;
    private const uint OpenTypeCffVersion = 0x4F54544F; // "OTTO"

    [Fact]
    public void IsComplete_RepositoryFont_ReturnsTrue()
    {
        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", "art", "fonts", "Lato-Regular.ttf"));

        Assert.True(FontFileHeader.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_ShorterThanTheDirectoryHeader_ReturnsFalse()
    {
        var bytes = Encoding.UTF8.GetBytes("<html>not a font</html>");

        Assert.False(FontFileHeader.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_OpenTypeCffTag_ReturnsFalse()
    {
        var bytes = TableDirectory(OpenTypeCffVersion, tableCount: 0, fileLength: 12);

        Assert.False(FontFileHeader.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_TableRecordsPastTheEnd_ReturnsFalse()
    {
        var bytes = TableDirectory(TrueTypeVersion, tableCount: 10, fileLength: 40);

        Assert.False(FontFileHeader.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_TableEndsExactlyAtTheFileEnd_ReturnsTrue()
    {
        var bytes = TableDirectory(TrueTypeVersion, tableCount: 1, fileLength: 40, (Offset: 28, Length: 12));

        Assert.True(FontFileHeader.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_TableEndsPastTheFile_ReturnsFalse()
    {
        var bytes = TableDirectory(TrueTypeVersion, tableCount: 1, fileLength: 40, (Offset: 28, Length: 13));

        Assert.False(FontFileHeader.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_TableOffsetNearIntMax_ReturnsFalse()
    {
        var bytes = TableDirectory(TrueTypeVersion, tableCount: 1, fileLength: 40, (Offset: int.MaxValue, Length: 16));

        Assert.False(FontFileHeader.IsComplete(bytes));
    }

    private static byte[] TableDirectory(uint version, ushort tableCount, int fileLength, params (uint Offset, uint Length)[] records)
    {
        var bytes = new byte[fileLength];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, version);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(4), tableCount);
        for (var index = 0; index < records.Length; index++)
        {
            var record = bytes.AsSpan(12 + (index * 16));
            BinaryPrimitives.WriteUInt32BigEndian(record[8..], records[index].Offset);
            BinaryPrimitives.WriteUInt32BigEndian(record[12..], records[index].Length);
        }

        return bytes;
    }
}
