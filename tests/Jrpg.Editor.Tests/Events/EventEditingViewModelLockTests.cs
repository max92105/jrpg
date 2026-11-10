using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

/// <summary>
/// Events mode under Dim and lock other layers (decision 525): a chest on layer -3 under a chest on layer 1 at one spot
/// of the start map, the covering chest listed last.
/// </summary>
public sealed class EventEditingViewModelLockTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly Point2D _spot = new(4.5, 6.5);

    private readonly LayerChoiceViewModel _layer = new();
    private readonly EventEditingViewModel _eventEditing;

    public EventEditingViewModelLockTests()
    {
        var start = _catalog.Maps["start"];
        var covered = new PlacedObject { Uid = "chest_under", ObjectId = "chest", Position = new Point2D(4, 6), Z = -3 };
        var covering = covered with { Uid = "chest_over", Z = 1 };
        var map = new MapViewModel(new MapDefinition(start.Layout with { Objects = [covered, covering] }, start.Settings));
        _eventEditing = new EventEditingViewModel(new CatalogDraft(_catalog), new OpenMaps(), _layer) { OpenMap = map, MapIds = ["forest", "start"] };
    }

    [Fact]
    public void StartEditingAt_LockedOnTheCoveredObjectsLayer_OpensItsEvents()
    {
        _layer.Z = -3;
        _layer.AreOtherLayersDimmed = true;

        Assert.Equal("Events of chest_under (chest)", _eventEditing.StartEditingAt(_spot)?.Title);
    }

    [Fact]
    public void StartEditingAt_LockedOnALayerWithNoObjectThere_GivesNull()
    {
        _layer.Z = 2;
        _layer.AreOtherLayersDimmed = true;

        Assert.Null(_eventEditing.StartEditingAt(_spot));
    }

    [Fact]
    public void StartEditingAt_NotLocked_OpensTheTopObjectsEvents()
    {
        _layer.Z = -3;

        Assert.Equal("Events of chest_over (chest)", _eventEditing.StartEditingAt(_spot)?.Title);
    }
}
