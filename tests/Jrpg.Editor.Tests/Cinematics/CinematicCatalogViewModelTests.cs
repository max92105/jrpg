using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// The Cinematics window's state (ADR 0046), on two cinematics built here and boxes offering what they name.
public sealed class CinematicCatalogViewModelTests
{
    private static readonly CinematicMediaChoices _choices =
        new(() => ["outro/1.png", "intro/1.png"], () => [], () => [], _ => null);

    private readonly CinematicCatalogViewModel _window = new([Cinematic("outro"), Cinematic("intro")], _choices);

    [Fact]
    public void Cinematics_SavedThenNew_InIdOrderThenAdded()
    {
        _window.NewId = "epilogue";

        _window.AddCinematicCommand.Execute(null);

        Assert.Equal(["intro", "outro", "epilogue"], _window.Cinematics.Select(cinematic => cinematic.Id));
    }

    [Fact]
    public void Add_TypedId_AddsAndChoosesIt()
    {
        _window.NewId = "epilogue";

        _window.AddCinematicCommand.Execute(null);

        Assert.Equal(("epilogue", true), (_window.SelectedCinematic!.Id, _window.SelectedCinematic.IsNew));
        Assert.Equal("", _window.NewId);
    }

    [Fact]
    public void Add_IdTaken_ShowsProblemAndAddsNothing()
    {
        _window.NewId = "intro";

        Assert.Equal("The id intro is taken.", _window.NewIdProblem);
        Assert.False(_window.AddCinematicCommand.CanExecute(null));
    }

    [Fact]
    public void Remove_Saved_IsRefused()
    {
        Assert.False(_window.RemoveCinematicCommand.CanExecute(null));
    }

    [Fact]
    public void Remove_New_RemovesIt()
    {
        _window.NewId = "epilogue";
        _window.AddCinematicCommand.Execute(null);

        _window.RemoveCinematicCommand.Execute(null);

        Assert.Equal(["intro", "outro"], _window.Cinematics.Select(cinematic => cinematic.Id));
    }

    // The row reads, so only Data's range rule refuses it.
    [Fact]
    public void Problem_StillZoomPast150_IsDatasMessage()
    {
        _window.Cinematics[0].Stills[0].ZoomEndText = "151";

        Assert.Equal(
            "cinematics.json: cinematic 'intro': stills[0]: zoom endPercent must be within 100..150 (was 151).",
            _window.Problem);
    }

    [Fact]
    public void Problem_RowCantBeRead_NamesTheCinematic()
    {
        _window.Cinematics[1].Stills[0].SecondsText = "";

        Assert.Equal("Cinematic outro: Still 1: the seconds must be a number above 0 (was '').", _window.Problem);
    }

    [Fact]
    public void Problem_NothingWrong_IsNull()
    {
        Assert.Null(_window.Problem);
    }

    [Fact]
    public void CanApply_ProblemShown_IsFalse()
    {
        _window.Cinematics[0].Stills[0].ImageFile = "";

        Assert.False(_window.CanApply);
    }

    [Fact]
    public void PropertyChanged_CanApply_RaisedWhenATypedSecondsChanges()
    {
        var raised = new List<string?>();
        _window.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _window.Cinematics[0].Stills[0].SecondsText = "5";

        Assert.Contains(nameof(CinematicCatalogViewModel.CanApply), raised);
    }

    [Fact]
    public void PropertyChanged_CanApply_RaisedWhenTheTrackChanges()
    {
        var raised = new List<string?>();
        _window.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _window.Cinematics[0].MusicFile = "town/loop.wav";

        Assert.Contains(nameof(CinematicCatalogViewModel.CanApply), raised);
    }

    [Fact]
    public void MissingNote_EverythingThere_IsEmpty()
    {
        Assert.Equal("", _window.MissingNote);
    }

    [Fact]
    public void MissingNote_PictureRetypedToOneNotThere_NamesItAndIsRaised()
    {
        var raised = new List<string?>();
        _window.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _window.SelectedCinematic!.Stills[0].ImageFile = "intro/7.png";

        Assert.Equal("Not in the folders: intro/7.png.", _window.MissingNote);
        Assert.Contains(nameof(CinematicCatalogViewModel.MissingNote), raised);
    }

    private static CinematicDefinition Cinematic(string id) =>
        new() { Id = id, Stills = [new CinematicStill { ImageFile = $"{id}/1.png", Seconds = 4 }] };
}
