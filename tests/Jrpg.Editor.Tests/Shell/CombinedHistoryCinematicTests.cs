using Jrpg.Core.Cinematics;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away a cinematic a map's event plays is refused naming the map (ADR 0046, as decision 581
// refuses a used shop).
public sealed class CombinedHistoryCinematicTests : IDisposable
{
    private static readonly CinematicDefinition _intro = new()
    {
        Id = "intro",
        Stills = [new CinematicStill { ImageFile = "intro/1.png", Seconds = 4 }],
    };

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_AddedCinematicPlayedOnAnotherMap_IsRefused()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetCinematics([_intro]);
        var mapId = SaveWithCellEvent(viewModel, new CinematicStep("intro"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: cinematic intro is played on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog!.Cinematics.ContainsKey("intro"));
    }

    [Fact]
    public void Undo_AddedCinematicNoEventPlays_Undoes()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetCinematics([_intro]);
        SaveWithCellEvent(viewModel, new OpenForgeStep());

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
        Assert.False(viewModel.Catalog!.Cinematics.ContainsKey("intro"));
    }

    // Puts an event with the step on the open map's top-left cell, saves, and opens another map so only the catalog
    // step is left to undo.
    private static string SaveWithCellEvent(MainViewModel viewModel, EventStep step)
    {
        var map = viewModel.CurrentMap!;
        var mapEvent = new MapEvent { Id = "cinematic_check", Activation = EventActivation.Touch, Steps = [step] };
        map.Do(SetCellEventEdit.For(map.Map.Layout.CellEvents, new GridCell(0, 0), mapEvent));
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != map.Map.Id));
        return map.Map.Id;
    }
}
