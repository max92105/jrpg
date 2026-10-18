using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class MapLayoutValidatorTests
{
    private const string FileName = "generated/maps/forest.json";

    private static readonly MapReferences _references = new(
        new MapContentReferences(
        TileIds: new HashSet<int> { 1, 2 },
        Objects: new Dictionary<string, ObjectDefinition>
        {
            ["tree"] = new() { Id = "tree", Width = 4, Height = 4, CollisionPolygon = [], Category = "General", Tiles = [new ObjectTile(0, 0, 1)], AbovePlayerZones = [] },
        },
        ItemIds: new HashSet<string>(),
        EncounterIds: new HashSet<string>()),
        Layouts: new Dictionary<string, MapLayout>());

    private static readonly PlacedObject _tree = new() { Uid = "1@0,0", ObjectId = "tree", Position = new Point2D(0, 0) };

    private static readonly MapLayout _forest = new()
    {
        Id = "forest",
        Width = 3,
        Height = 2,
        Layers = [new TileLayer { Rows = [[1, 1, 2], [1, 0, 1]] }],
        Objects = [_tree],
    };

    public static TheoryData<MapLayout, string> InvalidLayouts => new()
    {
        { _forest with { Width = 0, Layers = [] }, "width and height must be at least 1 (was 0x2)." },
        { _forest with { Layers = [new TileLayer { Rows = [[1, 1, 2]] }] }, "layer 0: must have 2 rows, one per map row (had 1)." },
        { _forest with { Layers = [_forest.Layers[0], new TileLayer { Rows = [[1, 1, 2], [1, 1]] }] }, "layer 1: row 1 must have 3 tile ids, one per map column (had 2)." },
        { _forest with { Layers = [new TileLayer { Rows = [[1, 1, 2], [1, 9, 1]] }] }, "layer 0: cell (1, 1) has unknown tile id 9." },
        { _forest with { Objects = [_tree, _tree with { Position = new Point2D(1, 1) }] }, "object uid '1@0,0' is used more than once." },
        { _forest with { Objects = [_tree, null!] }, "objects holds a null entry." },
        { _forest with { Objects = [_tree with { Uid = " " }] }, "an object 'tree' has an empty uid." },
    };

    [Fact]
    public void Validate_ValidLayout_DoesNotThrow()
    {
        var exception = Record.Exception(() => MapLayoutValidator.Validate(FileName, _forest, _references));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_IdDiffersFromFileName_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => MapLayoutValidator.Validate(FileName, _forest with { Id = "woods" }, _references));

        Assert.Equal("generated/maps/forest.json: id must equal the file name 'forest' (was 'woods').", exception.Message);
    }

    [Fact]
    public void Validate_RowCountDiffersFromHeight_ThrowsNamingTheLayer()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => MapLayoutValidator.Validate(FileName, _forest with { Height = 3 }, _references));

        Assert.Equal("generated/maps/forest.json: layer 0: must have 3 rows, one per map row (had 2).", exception.Message);
    }

    [Fact]
    public void Validate_ShortRow_ThrowsNamingTheLayerAndRow()
    {
        var layout = _forest with { Layers = [new TileLayer { Rows = [[1, 1], [1, 0, 1]] }] };

        var exception = Assert.Throws<ContentValidationException>(() => MapLayoutValidator.Validate(FileName, layout, _references));

        Assert.Equal("generated/maps/forest.json: layer 0: row 0 must have 3 tile ids, one per map column (had 2).", exception.Message);
    }

    [Fact]
    public void Validate_UnknownObjectId_ThrowsNamingTheUid()
    {
        var layout = _forest with { Objects = [_tree with { ObjectId = "house_9" }] };

        var exception = Assert.Throws<ContentValidationException>(() => MapLayoutValidator.Validate(FileName, layout, _references));

        Assert.Equal("generated/maps/forest.json: object '1@0,0' has unknown object id 'house_9'.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidLayouts))]
    public void Validate_InvalidField_ThrowsNamingTheProblem(MapLayout layout, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => MapLayoutValidator.Validate(FileName, layout, _references));

        Assert.Equal($"generated/maps/forest.json: {problem}", exception.Message);
    }
}
