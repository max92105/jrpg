using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// The placed object panel's Facing box (decision 572), on the forest with a villager (an NPC) facing left at (2, 3)
/// and a chest at (6, 6).
/// </summary>
public sealed class PlacedObjectEditingFacingTests
{
    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);

    private readonly ObjectPlacingViewModel _placing;

    public PlacedObjectEditingFacingTests()
    {
        var forest = _catalog.Maps["forest"];
        PlacedObject[] objects =
        [
            new() { Uid = "villager_1", ObjectId = "villager", Position = new Point2D(2, 3), Z = 1, Facing = Facing.Left },
            new() { Uid = "chest_1", ObjectId = "chest", Position = new Point2D(6, 6), Z = 1 },
        ];
        var map = new MapDefinition(forest.Layout with { Objects = objects }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), new LayerChoiceViewModel(), new OpenMaps())
        {
            OpenMap = new MapViewModel(map),
        };
    }

    private PlacedObjectEditing Editing => _placing.Editing;

    private MapViewModel Map => _placing.OpenMap!;

    [Fact]
    public void Facing_PlacedNpcFacingLeft_ShowsLeft()
    {
        _placing.Select(["villager_1"]);

        Assert.True(Editing.HasFacing);
        Assert.Equal(Facing.Left, Editing.Facing);
    }

    [Fact]
    public void HasFacing_TileObject_IsFalse()
    {
        _placing.Select(["chest_1"]);

        Assert.False(Editing.HasFacing);
    }

    [Fact]
    public void Apply_FacingChanged_IsOneMapStep()
    {
        _placing.Select(["villager_1"]);
        Editing.Facing = Facing.Up;

        Editing.ApplyCommand.Execute(null);

        Assert.Equal(Facing.Up, Villager().Facing);
        Map.UndoCommand.Execute(null);
        Assert.Equal(Facing.Left, Villager().Facing);
    }

    // Down is the layout's absence (decision 572), so an NPC turned back down writes no facing.
    [Fact]
    public void Apply_FacingDown_WritesNoFacing()
    {
        _placing.Select(["villager_1"]);
        Editing.Facing = Facing.Down;

        Editing.ApplyCommand.Execute(null);

        Assert.Null(Villager().Facing);
    }

    // Data refuses a facing on a tile object, so swapping the object drops it.
    [Fact]
    public void Apply_ObjectSwappedToTileObject_DropsTheFacing()
    {
        _placing.Select(["villager_1"]);
        Editing.ObjectId = "chest";
        Assert.False(Editing.HasFacing);

        Editing.ApplyCommand.Execute(null);

        Assert.Null(Villager().Facing);
    }

    [Fact]
    public void Apply_PositionOnly_KeepsTheFacing()
    {
        _placing.Select(["villager_1"]);
        Editing.XText = "3";

        Editing.ApplyCommand.Execute(null);

        Assert.Equal((3.0, Facing.Left), (Villager().Position.X, Villager().Facing));
    }

    private PlacedObject Villager() => Map.Map.Layout.Objects.Single(placed => placed.Uid == "villager_1");
}
