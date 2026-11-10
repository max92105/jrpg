using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

/// <summary>
/// A cell's event reached under an object (decision 526, Ctrl + click in Events mode): the interior's cell (4, 9),
/// which holds an event, with a chest placed over it.
/// </summary>
public sealed class EventEditingCellTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly GridCell _cell = new(4, 9);

    private readonly MapViewModel _map;
    private readonly EventEditingViewModel _eventEditing;

    public EventEditingCellTests()
    {
        var interior = _catalog.Maps["interior_1"];
        var chest = new PlacedObject { Uid = "chest_over", ObjectId = "chest", Position = new Point2D(_cell.X, _cell.Y), Z = 1 };
        _map = new MapViewModel(new MapDefinition(interior.Layout with { Objects = [.. interior.Layout.Objects, chest] }, interior.Settings));
        _eventEditing = new EventEditingViewModel(new CatalogDraft(_catalog), new OpenMaps(), new LayerChoiceViewModel()) { OpenMap = _map, MapIds = ["start"] };
    }

    // A plain click there opens the chest's events: the cell is covered, so the three tests below reach under it.
    [Fact]
    public void StartEditingAt_OverTheCell_OpensTheChestsEvents()
    {
        Assert.Equal("Events of chest_over (chest)", _eventEditing.StartEditingAt(new Point2D(_cell.X + 0.5, _cell.Y + 0.5))?.Title);
    }

    [Fact]
    public void StartEditingCellAt_CellUnderAnObject_OpensTheCellsEvent()
    {
        var cellEvent = _eventEditing.StartEditingCellAt(_cell)!;

        Assert.Equal("cell_4_9", cellEvent.Event.Id);
    }

    [Fact]
    public void RemoveCellEventAt_CellUnderAnObject_RemovesIt()
    {
        _eventEditing.RemoveCellEventAt(_cell);

        Assert.DoesNotContain(_map.Map.Layout.CellEvents, cellEvent => cellEvent.Cell == _cell);
    }

    [Fact]
    public void RemoveCellEventAt_CellUnderAnObject_Undo_PutsItBack()
    {
        _eventEditing.RemoveCellEventAt(_cell);

        _map.UndoCommand.Execute(null);

        Assert.Contains(_map.Map.Layout.CellEvents, cellEvent => cellEvent.Cell == _cell && cellEvent.Event.Id == "cell_4_9");
    }
}
