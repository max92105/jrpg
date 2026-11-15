using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class MapFolderReaderTests : IDisposable
{
    private const string Layout = """{ "id": "forest", "width": 1, "height": 1, "layers": [ { "z": 0, "rows": [ [ 1 ] ] } ], "objects": [] }""";
    private const string Settings = """{ "id": "forest", "displayName": "Whispering Forest" }""";

    private static readonly HashSet<int> _tileIds = [1];

    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-maps-{Guid.NewGuid():N}");

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void Read_LayoutAndSettings_ComposesTheMap()
    {
        WriteContent("map-layouts/forest.json", Layout);
        WriteContent("maps/forest.json", Settings);

        var map = Assert.Single(Read());

        Assert.Equal(("forest", "Whispering Forest", 1), (map.Id, map.Settings.DisplayName, map.Layout.Width));
    }

    [Fact]
    public void Read_LayoutWithoutSettings_ThrowsNamingTheMap()
    {
        WriteContent("map-layouts/forest.json", Layout);
        Directory.CreateDirectory(Path.Combine(_contentRoot, "maps"));

        var exception = Assert.Throws<ContentValidationException>(() => Read());

        Assert.Equal("map-layouts/forest.json: map 'forest' has no settings file in maps/.", exception.Message);
    }

    [Fact]
    public void Read_SettingsWithoutLayout_ThrowsNamingTheMap()
    {
        WriteContent("maps/forest.json", Settings);

        var exception = Assert.Throws<ContentValidationException>(() => Read());

        Assert.Equal("maps/forest.json: map 'forest' has no layout in map-layouts/.", exception.Message);
    }

    [Fact]
    public void Read_WarpToMapReadLater_LoadsWithTheStepTypeAnywhereInTheStep()
    {
        // "woods" sorts after "forest", so the warp target is only known because every layout is read first.
        WriteContent("maps/forest.json", Settings);
        WriteContent(
            "map-layouts/forest.json",
            """
            { "id": "forest", "width": 1, "height": 1, "layers": [ { "z": 0, "rows": [ [ 1 ] ] } ], "objects": [],
              "cellEvents": [ { "cell": { "x": 0, "y": 0 }, "event": { "id": "cell_0_0", "activation": "touch",
                "steps": [ { "mapId": "woods", "type": "warp", "cell": { "x": 1, "y": 0 } } ] } } ] }
            """);
        WriteContent("map-layouts/woods.json", """{ "id": "woods", "width": 2, "height": 1, "layers": [ { "z": 0, "rows": [ [ 1, 1 ] ] } ], "objects": [] }""");
        WriteContent("maps/woods.json", """{ "id": "woods", "displayName": "Woods" }""");

        var maps = Read();

        var step = maps.Single(map => map.Id == "forest").Layout.CellEvents[0].Event.Steps[0];
        Assert.Equal(new WarpStep("woods", new GridCell(1, 0)), step);
    }

    private IReadOnlyList<MapDefinition> Read() =>
        MapFolderReader.Read(_contentRoot, new MapContentReferences(_tileIds, new Dictionary<string, ObjectDefinition>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>()));

    private void WriteContent(string relativePath, string json)
    {
        var path = Path.Combine(_contentRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
    }
}
