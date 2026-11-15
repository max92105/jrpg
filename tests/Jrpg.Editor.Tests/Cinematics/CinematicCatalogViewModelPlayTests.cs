using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// Play and Stop in the Cinematics window (decision 620): greyed while the chosen cinematic has a box that can't be
// read, stopped by any box typed or another cinematic chosen. Two cinematics of two 4 s stills each.
public sealed class CinematicCatalogViewModelPlayTests
{
    private static readonly CinematicMediaChoices _choices = new(() => [], () => [], () => [], _ => null);

    private readonly CinematicCatalogViewModel _window = new([Cinematic("intro"), Cinematic("outro")], _choices);

    [Fact]
    public void Play_ChosenStill_StartsThere()
    {
        var intro = _window.Cinematics[0];
        intro.SelectedStill = intro.Stills[1];

        _window.PlayCommand.Execute(null);

        Assert.Equal("intro/2.png", _window.Preview.ShownImage);
    }

    [Fact]
    public void PlayCommand_ChosenCinematicHasProblem_IsGreyed()
    {
        _window.Cinematics[0].Stills[0].SecondsText = "x";

        Assert.False(_window.PlayCommand.CanExecute(null));
    }

    // A time each box reads, but past what the game's clock can count: Data refuses it, so Play doesn't overflow.
    [Fact]
    public void PlayCommand_SecondsPastTheGamesClock_IsGreyed()
    {
        _window.Cinematics[0].Stills[0].SecondsText = "1e15";

        Assert.Null(_window.Cinematics[0].Problem);
        Assert.False(_window.PlayCommand.CanExecute(null));
    }

    [Fact]
    public void PlayCommand_CanExecute_RaisedWhenATypedSecondsChanges()
    {
        var raised = 0;
        _window.PlayCommand.CanExecuteChanged += (_, _) => raised++;

        _window.Cinematics[0].Stills[0].SecondsText = "x";

        Assert.True(raised > 0);
    }

    [Fact]
    public void TypingSeconds_WhilePlaying_StopsThePreview()
    {
        _window.PlayCommand.Execute(null);

        _window.Cinematics[0].Stills[0].SecondsText = "5";

        Assert.False(_window.Preview.IsPlaying);
    }

    // A click on the picture is a typed change (decision 754).
    [Fact]
    public void SetFocusAt_WhilePlaying_StopsThePreview()
    {
        _window.PlayCommand.Execute(null);

        _window.Cinematics[0].Stills[0].SetFocusAt(0.25, 0.75);

        Assert.False(_window.Preview.IsPlaying);
    }

    [Fact]
    public void ClearFocus_RaisesTheProblemLine()
    {
        var still = _window.Cinematics[0].Stills[0];
        still.SetFocusAt(0.25, 0.75);
        var raised = new List<string?>();
        _window.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        still.ClearFocusCommand.Execute(null);

        Assert.Contains(nameof(CinematicCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void ChoosingAnotherCinematic_WhilePlaying_StopsThePreview()
    {
        _window.PlayCommand.Execute(null);

        _window.SelectedCinematic = _window.Cinematics[1];

        Assert.False(_window.Preview.IsPlaying);
    }

    [Fact]
    public void Stop_WhilePlaying_StopsThePreview()
    {
        _window.PlayCommand.Execute(null);

        _window.StopCommand.Execute(null);

        Assert.False(_window.Preview.IsPlaying);
    }

    private static CinematicDefinition Cinematic(string id) => new()
    {
        Id = id,
        Stills =
        [
            new CinematicStill { ImageFile = $"{id}/1.png", Seconds = 4 },
            new CinematicStill { ImageFile = $"{id}/2.png", Seconds = 4 },
        ],
    };
}
