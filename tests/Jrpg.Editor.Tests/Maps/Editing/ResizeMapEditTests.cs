using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class ResizeMapEditTests
{
    private const int GroundZ = -5;

    private static readonly Dictionary<string, ObjectDefinition> _definitions = new()
    {
        ["rock"] = new()
        {
            Id = "rock",
            Width = 2,
            Height = 1,
            CollisionPolygon = [],
            Category = "General",
            Tiles = [new ObjectTile(0, 0, 4), new ObjectTile(1, 0, 4)],
            AbovePlayerZones = [],
        },
    };

    // A 3x2 meadow: tiles 1 to 6, a rock in each corner of the top row, a warp cell at the bottom right.
    private static readonly PlacedObject _leftRock = Placed("rock_1", 0, 0);
    private static readonly PlacedObject _rightRock = Placed("rock_2", 2.5, 0);
    private static readonly CellEvent _warpCell = new(new GridCell(2, 1), new MapEvent { Id = "warp", Activation = EventActivation.Touch, Steps = [] });

    private static readonly MapDefinition _meadow = new(
        new MapLayout
        {
            Id = "meadow",
            Width = 3,
            Height = 2,
            Layers = [new TileLayer { Z = GroundZ, Rows = [[1, 2, 3], [4, 5, 6]] }],
            Objects = [_leftRock, _rightRock],
            CellEvents = [_warpCell],
        },
        new MapSettings { Id = "meadow", DisplayName = "Meadow" });

    private readonly MapDraft _draft = MapDraft.From(_meadow);

    [Fact]
    public void Apply_Shrink_DropsTilesAndObjectsOutside()
    {
        var edit = new ResizeMapEdit(_meadow.Layout, 2, 1, _definitions);

        edit.Apply(_draft);

        var layout = _draft.ToDefinition().Layout;
        Assert.Equal((2, 1), (layout.Width, layout.Height));
        Assert.Equal([[1, 2]], layout.Layers[0].Rows);
        Assert.Equal([_leftRock], layout.Objects);
        Assert.Empty(layout.CellEvents);
        Assert.Equal((4, 1, 1), (edit.DroppedTileCount, edit.DroppedObjectCount, edit.DroppedCellEventCount));
    }

    [Fact]
    public void Apply_ShrinkLeavingAnObjectAcrossTheEdge_KeepsIt()
    {
        // The right rock spans x 2.5 to 4.5; a width of 3 still overlaps it, as the meadow already does.
        var edit = new ResizeMapEdit(_meadow.Layout, 3, 1, _definitions);

        edit.Apply(_draft);

        Assert.Equal([_leftRock, _rightRock], _draft.ToDefinition().Layout.Objects);
    }

    [Fact]
    public void Apply_Grow_AddsEmptyCellsRightAndDown()
    {
        new ResizeMapEdit(_meadow.Layout, 4, 3, _definitions).Apply(_draft);

        Assert.Equal([[1, 2, 3, 0], [4, 5, 6, 0], [0, 0, 0, 0]], _draft.ToDefinition().Layout.Layers[0].Rows);
    }

    [Fact]
    public void Revert_RestoresDroppedContent()
    {
        var edit = new ResizeMapEdit(_meadow.Layout, 1, 1, _definitions);
        edit.Apply(_draft);

        edit.Revert(_draft);

        Assert.Equivalent(_meadow, _draft.ToDefinition(), strict: true);
    }

    [Fact]
    public void Apply_Grow_NewCellsCanBePainted()
    {
        new ResizeMapEdit(_meadow.Layout, 4, 2, _definitions).Apply(_draft);

        _draft.SetTileId(GroundZ, new GridCell(3, 1), 9);

        Assert.Equal([[1, 2, 3, 0], [4, 5, 6, 9]], _draft.ToDefinition().Layout.Layers[0].Rows);
    }

    private static PlacedObject Placed(string uid, double x, double y) =>
        new() { Uid = uid, ObjectId = "rock", Position = new Point2D(x, y), Z = 1 };
}
