using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class MapFolderReaderTests : IDisposable
{
    private const string Layout = """{ "id": "forest", "width": 1, "height": 1, "layers": [ { "rows": [ [ 1 ] ] } ], "objects": [] }""";
    private const string Settings = """{ "id": "forest", "displayName": "Whispering Forest" }""";

    private static readonly MapReferences _references = new(new HashSet<int> { 1 }, new Dictionary<string, ObjectDefinition>());

    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-maps-{Guid.NewGuid():N}");

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void Read_LayoutAndSettings_ComposesTheMap()
    {
        WriteContent("generated/maps/forest.json", Layout);
        WriteContent("maps/forest.json", Settings);

        var map = Assert.Single(MapFolderReader.Read(_contentRoot, _references));

        Assert.Equal(("forest", "Whispering Forest", 1), (map.Id, map.Settings.DisplayName, map.Layout.Width));
    }

    [Fact]
    public void Read_LayoutWithoutSettings_ThrowsNamingTheMap()
    {
        WriteContent("generated/maps/forest.json", Layout);
        Directory.CreateDirectory(Path.Combine(_contentRoot, "maps"));

        var exception = Assert.Throws<ContentValidationException>(() => MapFolderReader.Read(_contentRoot, _references));

        Assert.Equal("generated/maps/forest.json: map 'forest' has no settings file in maps/.", exception.Message);
    }

    [Fact]
    public void Read_SettingsWithoutLayout_ThrowsNamingTheMap()
    {
        WriteContent("maps/forest.json", Settings);

        var exception = Assert.Throws<ContentValidationException>(() => MapFolderReader.Read(_contentRoot, _references));

        Assert.Equal("maps/forest.json: map 'forest' has no layout in generated/maps/; run the content converter.", exception.Message);
    }

    private void WriteContent(string relativePath, string json)
    {
        var path = Path.Combine(_contentRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
    }
}
