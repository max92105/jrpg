using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

public sealed class EventEditingViewModelTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly CatalogDraft _draft = new(_catalog);

    private readonly OpenMaps _openMaps = new();

    private readonly EventEditingViewModel _eventEditing;

    public EventEditingViewModelTests() =>
        _eventEditing = new EventEditingViewModel(_draft, _openMaps) { OpenMap = new MapViewModel(_catalog.Maps["start"]), MapIds = ["forest", "start"] };

    // The start map's chest, which gives a potion and turns into an opened chest.
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
        Assert.Contains("general_store", events.Choices.ShopIds);
    }

    [Fact]
    public void StartEditingAt_CatalogWithQuest_OffersItsIdAndStages()
    {
        var events = _eventEditing.StartEditingAt(ChestCenter)!;

        Assert.Contains("placeholder_errand", events.Choices.QuestIds);
        Assert.Equal(
            [new QuestStageChoice("placeholder_errand", "ask_innkeeper"), new QuestStageChoice("placeholder_errand", "return_to_giver")],
            events.Choices.QuestStages.Where(stage => stage.QuestId == "placeholder_errand"));
    }

    // The dialog reads the draft when it opens, so a quest made in the Quests window is offered without a restart.
    [Fact]
    public void StartEditingAt_QuestSetInTheDraft_OffersIt()
    {
        var heist = new QuestDefinition("heist", "The heist", "Case the vault.", QuestKind.Main, [new QuestStage("case", "Case the vault.")]);
        _draft.SetQuests([.. _catalog.Quests.Values, heist]);

        var events = _eventEditing.StartEditingAt(ChestCenter)!;

        Assert.Contains("heist", events.Choices.QuestIds);
        Assert.Contains(new QuestStageChoice("heist", "case"), events.Choices.QuestStages);
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
    public void AddStep_Rest_UndoRemovesIt()
    {
        var events = _eventEditing.StartEditingAt(ChestCenter)!;
        events.SelectedEvent!.AddRestCommand.Execute(null);
        _eventEditing.Apply(events);
        var map = _eventEditing.OpenMap!;
        var chestSteps = () => map.Map.Layout.Objects.Single(placed => placed.Uid == "0bff6243").Events[0].Steps;
        var stepsWithRest = chestSteps();

        map.UndoCommand.Execute(null);

        Assert.Contains(stepsWithRest, step => step is RestStep);
        Assert.DoesNotContain(chestSteps(), step => step is RestStep);
    }

    [Fact]
    public void StartEditingCellAt_CellWithEvent_EditsIt()
    {
        _eventEditing.OpenMap = new MapViewModel(_catalog.Maps["interior_1"]);

        var cellEvent = _eventEditing.StartEditingCellAt(new GridCell(4, 9))!;

        Assert.Equal("cell_4_9", cellEvent.Event.Id);
    }

    [Fact]
    public void StartEditingCellAt_AnyCell_OffersTheCatalogsMembersAsLeaders()
    {
        var cellEvent = _eventEditing.StartEditingCellAt(new GridCell(0, 0))!;

        Assert.Contains("sarxian", cellEvent.LeaderChoices);
        Assert.Contains("zanyr", cellEvent.LeaderChoices);
        Assert.Equal("", cellEvent.LeaderChoices[0]);
    }

    [Fact]
    public void StartEditingCellAt_OffTheMap_IsNull()
    {
        Assert.Null(_eventEditing.StartEditingCellAt(new GridCell(-1, 0)));
    }

    [Fact]
    public void RemoveCellEventAt_CellWithEvent_IsOneUndoStep()
    {
        var map = _eventEditing.OpenMap = new MapViewModel(_catalog.Maps["interior_1"]);
        var before = map.Map.Layout.CellEvents.Count;

        _eventEditing.RemoveCellEventAt(new GridCell(4, 9));

        Assert.Equal(before - 1, map.Map.Layout.CellEvents.Count);
        map.UndoCommand.Execute(null);
        Assert.Equal(before, map.Map.Layout.CellEvents.Count);
    }

    [Fact]
    public void RemoveCellEventAt_CellWithoutEvent_ChangesNothing()
    {
        _eventEditing.RemoveCellEventAt(new GridCell(0, 0));

        Assert.False(_eventEditing.OpenMap!.IsDirty);
    }

    [Fact]
    public void Apply_NewCellEvent_AddsIt()
    {
        var cellEvent = _eventEditing.StartEditingCellAt(new GridCell(1, 1))!;
        cellEvent.Event.AddWarpCommand.Execute(null);
        cellEvent.Event.SelectedStep!.MapId = "forest";

        _eventEditing.Apply(cellEvent);

        Assert.Contains(_eventEditing.OpenMap!.Map.Layout.CellEvents, added => added.Event.Id == "cell_1_1");
    }

    [Fact]
    public void Apply_NothingChanged_AddsNoUndoStep()
    {
        _eventEditing.Apply(_eventEditing.StartEditingAt(ChestCenter)!);

        Assert.False(_eventEditing.OpenMap!.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void StartEditing_UidOnTheMap_StartsOnItsEvents()
    {
        var events = _eventEditing.StartEditing("0bff6243");

        Assert.StartsWith("Events of 0bff6243 ", events?.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void StartEditing_UnknownUid_IsNull()
    {
        Assert.Null(_eventEditing.StartEditing("no_such_uid"));
    }

    // Forest is painted and hidden behind start: the picker draws forest as it is now, not as saved.
    [Fact]
    public void StartEditingAt_HiddenOpenMapPainted_WarpMapsHoldItsDraftLayout()
    {
        var (editing, forest) = EditingStartWithForestOpen();
        forest.PutTiles(forest.Layers[0].Z, 0, [new GridCell(0, 0)]);

        var dialog = editing.StartEditingAt(ChestCenter)!;

        Assert.Same(forest.Map.Layout, dialog.Choices.WarpMaps!.Layouts["forest"]);
    }

    [Fact]
    public void StartEditingAt_NewMapOpen_WarpMapsListIt()
    {
        var (editing, _) = EditingStartWithForestOpen();
        var mill = MapViewModel.ForNewMap(new MapDefinition(_catalog.Maps["forest"].Layout with { Id = "old_mill" }, _catalog.Maps["forest"].Settings with { Id = "old_mill", DisplayName = "Old Mill" }));
        _openMaps.Add(mill);

        var warpMaps = editing.StartEditingAt(ChestCenter)!.Choices.WarpMaps!;

        Assert.Contains(warpMaps.Entries, entry => entry.Id == "old_mill");
        Assert.True(warpMaps.Layouts.ContainsKey("old_mill"));
    }

    [Fact]
    public void StartEditingAt_WarpMaps_AreTheMapListsRows()
    {
        var (editing, _) = EditingStartWithForestOpen();

        var warpMaps = editing.StartEditingAt(ChestCenter)!.Choices.WarpMaps!;

        Assert.Equal(_openMaps.EntriesWith(_catalog), warpMaps.Entries);
    }

    // The editing state on start with forest open too, through the same open maps.
    private (EventEditingViewModel Editing, MapViewModel Forest) EditingStartWithForestOpen()
    {
        var forest = new MapViewModel(_catalog.Maps["forest"]);
        _openMaps.Add(forest);
        _openMaps.Add(_eventEditing.OpenMap!);
        return (_eventEditing, forest);
    }
}
