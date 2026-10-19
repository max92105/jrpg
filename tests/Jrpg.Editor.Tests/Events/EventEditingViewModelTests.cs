using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

public sealed class EventEditingViewModelTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    private readonly EventEditingViewModel _eventEditing = new(_catalog) { OpenMap = new MapViewModel(_catalog.Maps["start"]), MapIds = ["forest", "start"] };

    // The start map's chest, which gives a potion and turns into an opened chest (converted from the legacy demo).
    private Point2D ChestCenter
    {
        get
        {
            var chest = _eventEditing.OpenMap!.Map.Layout.Objects.Single(placed => placed.Uid == "0bff6243");
            return new Point2D(chest.Position.X + 0.5, chest.Position.Y + 0.5);
        }
    }

    [Fact]
    public void StartEditingAt_ObjectWithEvents_ListsThem()
    {
        var events = _eventEditing.StartEditingAt(ChestCenter)!;

        Assert.Equal(["0bff6243_0"], events.Events.Select(item => item.Id));
        Assert.Equal(["forest", "start"], events.Choices.MapIds);
        Assert.Contains("potion", events.Choices.ItemIds);
    }

    [Fact]
    public void StartEditingAt_NoObjectThere_IsNull()
    {
        Assert.Null(_eventEditing.StartEditingAt(new Point2D(-5, -5)));
    }

    [Fact]
    public void Apply_ChangedEvents_IsOneUndoStep()
    {
        var events = _eventEditing.StartEditingAt(ChestCenter)!;
        events.SelectedEvent!.Prompt = "Search";

        _eventEditing.Apply(events);

        var map = _eventEditing.OpenMap!;
        Assert.Equal("Search", map.Map.Layout.Objects.Single(placed => placed.Uid == "0bff6243").Events[0].Prompt);
        map.UndoCommand.Execute(null);
        Assert.False(map.IsDirty);
    }

    [Fact]
    public void Apply_NothingChanged_AddsNoUndoStep()
    {
        _eventEditing.Apply(_eventEditing.StartEditingAt(ChestCenter)!);

        Assert.False(_eventEditing.OpenMap!.UndoCommand.CanExecute(null));
    }
}
