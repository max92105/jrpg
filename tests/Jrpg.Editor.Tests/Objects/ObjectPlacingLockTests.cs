using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Dim and lock other layers (decision 525): the locked layers' objects ignore every click. A chest on layer -3 under a
/// chest on layer 1 at one spot of the forest map, cleared of its objects; the covering chest listed last.
/// </summary>
public sealed class ObjectPlacingLockTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly Point2D _spot = new(4.5, 6.5);

    private readonly LayerChoiceViewModel _layer = new();
    private readonly ObjectPlacingViewModel _placing;

    public ObjectPlacingLockTests()
    {
        var forest = _catalog.Maps["forest"];
        var emptyForest = new MapDefinition(forest.Layout with { Objects = [] }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), _layer, new OpenMaps()) { OpenMap = new MapViewModel(emptyForest) };
        _placing.Browser.SelectedObject = _catalog.Objects["chest"];
        _layer.Z = -3;
        _placing.PlaceObjectAt(_spot);
        _layer.Z = 1;
        _placing.PlaceObjectAt(_spot);
    }

    [Fact]
    public void ObjectAt_LockedAndCoveredByAnotherLayer_GivesTheChosenLayersObject()
    {
        LockOn(-3);

        Assert.Equal("chest_1", _placing.ObjectAt(_spot)?.Uid);
    }

    [Fact]
    public void ObjectAt_NotLocked_GivesTheTop()
    {
        _layer.Z = -3;

        Assert.Equal("chest_2", _placing.ObjectAt(_spot)?.Uid);
    }

    [Fact]
    public void ObjectAt_LockTurnedOff_ReachesEveryLayerAgain()
    {
        LockOn(-3);
        Assert.Equal("chest_1", _placing.ObjectAt(_spot)?.Uid);

        _layer.AreOtherLayersDimmed = false;

        Assert.Equal("chest_2", _placing.ObjectAt(_spot)?.Uid);
    }

    [Fact]
    public void SelectObjectsIn_Locked_LeavesOtherLayersOut()
    {
        LockOn(-3);

        _placing.SelectObjectsIn(new Point2D(3.9, 5.9), new Point2D(5.1, 7.1));

        Assert.Equal(["chest_1"], _placing.SelectedObjectUids);
    }

    [Fact]
    public void GrabbedObjectAt_Locked_SkipsOtherLayers()
    {
        LockOn(-3);

        Assert.Equal("chest_1", _placing.GrabbedObjectAt(_spot)?.Uid);
    }

    [Fact]
    public void PickNote_LockedAndOnlyOtherLayersHere_SaysLocked()
    {
        LockOn(2);

        _placing.SelectObjectAt(_spot);

        Assert.Empty(_placing.SelectedObjectUids);
        Assert.Equal("Locked: only layer z 2 takes clicks", _placing.Picking.Note);
    }

    [Fact]
    public void PickNote_LockTurnedOff_IsCleared()
    {
        LockOn(2);
        _placing.SelectObjectAt(_spot);
        Assert.Equal("Locked: only layer z 2 takes clicks", _placing.Picking.Note);

        _layer.AreOtherLayersDimmed = false;

        Assert.Equal("", _placing.Picking.Note);
    }

    [Fact]
    public void PickNote_LockedWithNothingThereAtAll_IsEmpty()
    {
        LockOn(2);

        _placing.SelectObjectAt(new Point2D(9.5, 9.5));

        Assert.Equal("", _placing.Picking.Note);
    }

    private void LockOn(int z)
    {
        _layer.Z = z;
        _layer.AreOtherLayersDimmed = true;
    }
}
