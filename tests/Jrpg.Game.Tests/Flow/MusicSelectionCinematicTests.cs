using Jrpg.Core.Cinematics;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Flow;
using Jrpg.Game.Tests.Exploration;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

// A cinematic's track plays while its step is the event's (decision 614), on the start map, whose track is Map.wav, and
// the chest's cinematic, then its line.
public sealed class MusicSelectionCinematicTests
{
    [Fact]
    public void TrackFor_CinematicWithTrack_IsItsTrack()
    {
        var session = SessionAtTheChestsCinematic(cinematicTrack: "Story.wav");

        Assert.Equal("Story.wav", MusicSelection.TrackFor(session, null));
    }

    [Fact]
    public void TrackFor_CinematicWithoutTrack_IsTheMapsTrack()
    {
        var session = SessionAtTheChestsCinematic(cinematicTrack: null);

        Assert.Equal("Map.wav", MusicSelection.TrackFor(session, null));
    }

    [Fact]
    public void TrackFor_AfterTheCinematicStep_IsTheMapsTrack()
    {
        var session = SessionAtTheChestsCinematic(cinematicTrack: "Story.wav");

        session.Exploration.ContinueEvent();

        Assert.Equal("Map.wav", MusicSelection.TrackFor(session, null));
    }

    private static GameSession SessionAtTheChestsCinematic(string? cinematicTrack)
    {
        var catalog = WithStartTrack(StartMapScenarios.BelowChestPlayingCinematic, "Map.wav");
        var cinematic = catalog.Cinematics["test_cinematic"] with { MusicFile = cinematicTrack };
        var withTrack = catalog with { Cinematics = new Dictionary<string, CinematicDefinition> { [cinematic.Id] = cinematic } };
        var session = GameSession.StartNew(withTrack, "sarxian", new SeededRandom(1));
        session.Exploration.TryInteract();
        return session;
    }
}
