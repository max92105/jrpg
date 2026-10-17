using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentFileWriterTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"jrpg-writer-{Guid.NewGuid():N}.json");

    public void Dispose() => File.Delete(_path);

    [Fact]
    public void Write_TileList_ReadsBackEqualThroughContentFileReader()
    {
        IReadOnlyList<TileDefinition> tiles = [new() { Id = 1, IsWalkable = true }, new() { Id = 7, IsWalkable = false }];

        ContentFileWriter.Write(_path, tiles);
        var readBack = ContentFileReader.Read<IReadOnlyList<TileDefinition>>(_path);

        Assert.Equal(tiles, readBack);
    }

    [Fact]
    public void Write_TileList_UsesCamelCaseCrlfAndFinalNewline()
    {
        IReadOnlyList<TileDefinition> tiles = [new() { Id = 1, IsWalkable = true }];

        ContentFileWriter.Write(_path, tiles);

        Assert.Equal("[\r\n  {\r\n    \"id\": 1,\r\n    \"isWalkable\": true\r\n  }\r\n]\r\n", File.ReadAllText(_path));
    }
}
