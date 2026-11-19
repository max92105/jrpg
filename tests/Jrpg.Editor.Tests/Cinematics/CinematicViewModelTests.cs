using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// One cinematic as the Cinematics page edits it (ADR 0046): its stills added, removed and moved, numbered from 1, and
// the first thing that keeps it from being built.
public sealed class CinematicViewModelTests
{
    private static readonly CinematicDefinition _intro = new()
    {
        Id = "intro",
        Stills = [Still("intro/1.png"), Still("intro/2.png"), Still("intro/3.png")],
    };

    [Fact]
    public void From_Cinematic_ListsStillsInOrderNumberedFromOne()
    {
        var cinematic = CinematicViewModel.From(_intro);

        Assert.Equal([(1, "intro/1.png"), (2, "intro/2.png"), (3, "intro/3.png")], Rows(cinematic));
    }

    [Fact]
    public void AddStill_StillChosen_InsertsAfterItAndChoosesIt()
    {
        var cinematic = CinematicViewModel.From(_intro);
        cinematic.SelectedStill = cinematic.Stills[0];
        cinematic.ImageToAdd = "intro/new.png";

        cinematic.AddStillCommand.Execute(null);

        Assert.Equal([(1, "intro/1.png"), (2, "intro/new.png"), (3, "intro/2.png"), (4, "intro/3.png")], Rows(cinematic));
        Assert.Same(cinematic.Stills[1], cinematic.SelectedStill);
    }

    [Fact]
    public void AddStill_NoneChosen_AppendsIt()
    {
        var cinematic = CinematicViewModel.New();
        cinematic.ImageToAdd = "intro/new.png";

        cinematic.AddStillCommand.Execute(null);

        Assert.Equal([(1, "intro/new.png")], Rows(cinematic));
    }

    [Fact]
    public void AddStillCommand_NoPicturePicked_CannotExecute()
    {
        Assert.False(CinematicViewModel.From(_intro).AddStillCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveStill_Middle_RenumbersTheRest()
    {
        var cinematic = CinematicViewModel.From(_intro);
        cinematic.SelectedStill = cinematic.Stills[1];

        cinematic.RemoveStillCommand.Execute(null);

        Assert.Equal([(1, "intro/1.png"), (2, "intro/3.png")], Rows(cinematic));
    }

    [Fact]
    public void MoveStillDown_First_SwapsAndRenumbers()
    {
        var cinematic = CinematicViewModel.From(_intro);

        cinematic.MoveStillDownCommand.Execute(null);

        Assert.Equal([(1, "intro/2.png"), (2, "intro/1.png"), (3, "intro/3.png")], Rows(cinematic));
    }

    [Fact]
    public void MoveStillUp_First_DoesNothing()
    {
        var cinematic = CinematicViewModel.From(_intro);

        cinematic.MoveStillUpCommand.Execute(null);

        Assert.Equal([(1, "intro/1.png"), (2, "intro/2.png"), (3, "intro/3.png")], Rows(cinematic));
    }

    [Fact]
    public void Problem_NewWithBadId_NamesTheId()
    {
        var cinematic = CinematicViewModel.New();
        cinematic.Id = "Ragnir Intro";

        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", cinematic.Problem);
    }

    [Fact]
    public void Problem_NoStills_AsksForOne()
    {
        var cinematic = CinematicViewModel.New();
        cinematic.Id = "intro";

        Assert.Equal("Add at least one still.", cinematic.Problem);
    }

    [Fact]
    public void Problem_RowProblem_IsTheFirstRows()
    {
        var cinematic = CinematicViewModel.From(_intro);
        cinematic.Stills[1].SecondsText = "x";
        cinematic.Stills[2].ImageFile = "";

        Assert.Equal("Still 2: the seconds must be a number above 0 (was 'x').", cinematic.Problem);
    }

    [Fact]
    public void Problem_RowTyped_IsRaised()
    {
        var cinematic = CinematicViewModel.From(_intro);
        var raised = new List<string?>();
        cinematic.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        cinematic.Stills[0].SecondsText = "x";

        Assert.Contains(nameof(CinematicViewModel.Problem), raised);
    }

    [Fact]
    public void ToCinematic_NoTrack_HasNullMusic()
    {
        Assert.Null(CinematicViewModel.From(_intro).ToCinematic().MusicFile);
    }

    [Fact]
    public void ToCinematic_Track_KeepsIt()
    {
        var cinematic = CinematicViewModel.From(_intro);
        cinematic.MusicFile = "town/loop.wav";

        Assert.Equal(_intro with { MusicFile = "town/loop.wav" }, cinematic.ToCinematic());
    }

    [Fact]
    public void StillsChanged_TypingInARow_IsRaised()
    {
        var cinematic = CinematicViewModel.From(_intro);
        var raised = 0;
        cinematic.StillsChanged += (_, _) => raised++;

        cinematic.Stills[2].VoiceFile = "end.wav";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void StillsChanged_Moving_IsRaised()
    {
        var cinematic = CinematicViewModel.From(_intro);
        var raised = 0;
        cinematic.StillsChanged += (_, _) => raised++;

        cinematic.MoveStillDownCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    // A moved row is still watched once: one change typed in it is one notice.
    [Fact]
    public void StillsChanged_TypingInAMovedRow_IsRaisedOnce()
    {
        var cinematic = CinematicViewModel.From(_intro);
        cinematic.MoveStillDownCommand.Execute(null);
        var raised = 0;
        cinematic.StillsChanged += (_, _) => raised++;

        cinematic.Stills[1].VoiceFile = "moved.wav";

        Assert.Equal(1, raised);
    }

    private static CinematicStill Still(string imageFile) => new() { ImageFile = imageFile, Seconds = 4 };

    private static List<(int, string)> Rows(CinematicViewModel cinematic) =>
        [.. cinematic.Stills.Select(row => (row.Number, row.ImageFile))];
}
