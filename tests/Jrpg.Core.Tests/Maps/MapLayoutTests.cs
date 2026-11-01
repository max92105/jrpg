using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Maps;

// A map's events live on its placed objects and on its cells; whoever looks for an event (a quest it names, an item it
// gives) reads them all from one place.
public sealed class MapLayoutTests
{
    [Fact]
    public void AllEvents_ObjectAndCellEvents_ListsObjectsFirstThenCells()
    {
        var onObject = new MapEvent { Id = "open", Activation = EventActivation.Interact, Steps = [] };
        var onCell = new MapEvent { Id = "creak", Activation = EventActivation.Touch, Steps = [] };
        var placed = new PlacedObject { Uid = "a1", ObjectId = "chest", Position = new Point2D(1, 1), Z = 0, Events = [onObject] };
        var layout = new MapLayoutBuilder().WithLayer("1").Build() with { Objects = [placed], CellEvents = [new CellEvent(new GridCell(0, 0), onCell)] };

        Assert.Equal([onObject, onCell], layout.AllEvents);
    }
}
