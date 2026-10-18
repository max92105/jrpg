using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
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
        IReadOnlyList<TileDefinition> tiles = [new() { Id = 1, IsWalkable = true, SheetId = "city", Column = 0, Row = 2 }, new() { Id = 7, IsWalkable = false, SheetId = "city", Column = 3, Row = 0 }];

        ContentFileWriter.Write(_path, tiles);
        var readBack = ContentFileReader.Read<IReadOnlyList<TileDefinition>>(_path);

        Assert.Equal(tiles, readBack);
    }

    [Fact]
    public void Write_EveryEventStepKind_ReadsBackAsTheSameSteps()
    {
        IReadOnlyList<EventStep> steps =
        [
            new DialogueStep(new Speech(["Hello."], Speaker: "Sarxian")),
            new GiveItemStep("potion", 2),
            new SwapObjectStep("chest_opened"),
            new WarpStep("forest", new GridCell(1, 2)),
        ];

        ContentFileWriter.Write(_path, steps);
        var readBack = ContentFileReader.Read<IReadOnlyList<EventStep>>(_path);

        Assert.Equal(steps.Select(step => step.GetType()), readBack.Select(step => step.GetType()));
        Assert.Equal(steps.Skip(1), readBack.Skip(1));
        var speech = ((DialogueStep)readBack[0]).Speech;
        Assert.Equal(["Hello."], speech.Lines);
        Assert.Equal(("Sarxian", null), (speech.Speaker, speech.PortraitFile));
        Assert.Contains("\"type\": \"giveItem\"", File.ReadAllText(_path), StringComparison.Ordinal);
    }

    [Fact]
    public void Write_GridCell_LeavesOutTheComputedCenter()
    {
        ContentFileWriter.Write(_path, new GridCell(2, 3));

        Assert.Equal("{\r\n  \"x\": 2,\r\n  \"y\": 3\r\n}\r\n", File.ReadAllText(_path));
    }

    [Fact]
    public void Write_TileList_UsesCamelCaseCrlfAndFinalNewline()
    {
        IReadOnlyList<TileDefinition> tiles = [new() { Id = 1, IsWalkable = true, SheetId = "city", Column = 4, Row = 2 }];

        ContentFileWriter.Write(_path, tiles);

        Assert.Equal("[\r\n  {\r\n    \"id\": 1,\r\n    \"isWalkable\": true,\r\n    \"sheetId\": \"city\",\r\n    \"column\": 4,\r\n    \"row\": 2\r\n  }\r\n]\r\n", File.ReadAllText(_path));
    }
}
