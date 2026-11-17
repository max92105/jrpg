using Jrpg.Core.Cinematics;
using Jrpg.Core.Events;

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
        var mapId = EditorTestContent.SaveWithCellEvent(viewModel, new CinematicStep("intro"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: cinematic intro is played on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog!.Cinematics.ContainsKey("intro"));
    }

    [Fact]
    public void Undo_AddedCinematicNoEventPlays_Undoes()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetCinematics([_intro]);
        EditorTestContent.SaveWithCellEvent(viewModel, new OpenForgeStep());

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
        Assert.False(viewModel.Catalog!.Cinematics.ContainsKey("intro"));
    }
}
