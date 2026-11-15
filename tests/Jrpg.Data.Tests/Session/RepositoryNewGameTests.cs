using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Session;

public sealed class RepositoryNewGameTests
{
    [Fact]
    public void StartNew_RepositoryContent_LeaderStartsAlone()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(0));

        Assert.Equal("sarxian", session.Party.Leader.Id);
        Assert.Equal(["sarxian"], session.Party.Members.Select(member => member.Id));
        Assert.Equal(["sarxian"], session.Party.ActiveMembers.Select(member => member.Id));
        Assert.Equal(5, session.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void StartNew_RepositoryContent_EveryLeaderChoiceStarts()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var parties = catalog.NewGame.LeaderChoices.Select(choice => GameSession.StartNew(catalog, choice.MemberId, new SeededRandom(0)).Party);

        Assert.Equal(
            catalog.NewGame.LeaderChoices.Select(choice => new[] { choice.MemberId }),
            parties.Select(party => party.Members.Select(member => member.Id).ToArray()));
    }

    // Placeholder recruit points, so any party can be put together for testing (decision 163).
    [Fact]
    public void StartMap_RepositoryContent_OffersEveryCharacterToRecruit()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var recruitSteps = catalog.Maps["milestone"].Layout.Objects
            .SelectMany(placed => placed.Events)
            .SelectMany(mapEvent => mapEvent.Steps)
            .OfType<RecruitStep>()
            .ToList();

        var stepsPerCharacter = catalog.NewGame.LeaderChoices.Select(choice => recruitSteps.Count(step => step.MemberId == choice.MemberId));

        Assert.All(stepsPerCharacter, count => Assert.Equal(1, count));
    }

    [Fact]
    public void StartNew_RepositoryContent_StartsOnTheIntroWithTheLeadersIntroRunning()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var exploration = GameSession.StartNew(catalog, "zanyr", new SeededRandom(0)).Exploration;

        Assert.Equal("intro", exploration.CurrentMap.Id);
        var step = Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
        Assert.Contains("Zanyr", catalog.Cinematics[step.CinematicId].Stills[0].Caption, StringComparison.Ordinal);
    }

    [Fact]
    public void StartNew_RepositoryContent_EveryIntroPlaysItsCinematic()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var intros = catalog.NewGame.LeaderChoices
            .Select(choice => (choice.MemberId, Intro: IntrosOf(catalog, choice.MemberId).Single()));

        // Each leader's own record, by the content's naming: a cell naming another leader's would play the wrong intro.
        Assert.All(intros, leader => Assert.Contains(new CinematicStep($"{leader.MemberId}_intro"), leader.Intro.Steps));
        Assert.All(intros, leader => Assert.True(catalog.Cinematics.ContainsKey($"{leader.MemberId}_intro")));
    }

    [Fact]
    public void Restore_SaveLandingOnTheIntroMap_PlaysTheIntroAgain()
    {
        // Where a save whose map left the content is repaired to (decision 186): the intro plays again, then warps out.
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var session = GameSession.StartNew(catalog, "zanyr", new SeededRandom(0));
        session.Exploration.SkipEventPresentation();
        session.Exploration.ContinueEvent();
        var save = session.Capture(DateTimeOffset.UnixEpoch) with { Location = catalog.NewGame.StartLocation };

        var restored = GameSession.Restore(save, catalog, new SeededRandom(0));

        Assert.IsType<CinematicStep>(restored.Exploration.CurrentEventStep);
    }

    // What the rebuilt world must keep (decision 441), read by what its events do, never by a cell (decision 447).
    [Fact]
    public void StartNew_RepositoryContent_EveryLeaderHasOneIntroEndingWithAWarpToTheStartMap()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var introsPerLeader = catalog.NewGame.LeaderChoices.Select(choice => IntrosOf(catalog, choice.MemberId));

        Assert.All(introsPerLeader, intros => Assert.Equal("milestone", Assert.IsType<WarpStep>(Assert.Single(intros).Steps[^1]).MapId));
    }

    [Fact]
    public void StartNew_RepositoryContent_SkippedIntroLandsOnTheStartMapWhereItsWarpSays()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var warp = (WarpStep)IntrosOf(catalog, "thaali").Single().Steps[^1];
        var exploration = GameSession.StartNew(catalog, "thaali", new SeededRandom(0)).Exploration;

        exploration.SkipEventPresentation();
        exploration.ContinueEvent();

        Assert.False(exploration.IsEventRunning);
        Assert.Equal(new MapLocation("milestone", warp.Cell.Center, Facing.Down), exploration.Location);
    }

    // A door is a placed object's event; an interior has no random battles. Nothing else tells an interior apart.
    [Fact(Skip = "Decision 454: the developer places the start map's new door later; remove this Skip then.")]
    public void StartMap_RepositoryContent_HasADoorIntoAnInterior()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var warps = catalog.Maps["milestone"].Layout.Objects.SelectMany(placed => placed.Events).SelectMany(mapEvent => mapEvent.Steps).OfType<WarpStep>();

        Assert.Contains(warps, warp => warp.MapId != "milestone" && catalog.Maps[warp.MapId].Settings.RandomEncounters is null);
    }

    // An edge exit is a cell the player walks onto; the wild beyond it rolls random battles.
    [Fact]
    public void StartMap_RepositoryContent_HasAnExitToAMapWithRandomBattles()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var warps = catalog.Maps["milestone"].Layout.CellEvents.SelectMany(cellEvent => cellEvent.Event.Steps).OfType<WarpStep>();

        Assert.Contains(warps, warp => catalog.Maps[warp.MapId].Settings.RandomEncounters is not null);
    }

    private static List<MapEvent> IntrosOf(ContentCatalog catalog, string leaderId) =>
        catalog.Maps["intro"].Layout.CellEvents
            .Select(cellEvent => cellEvent.Event)
            .Where(mapEvent => mapEvent.Activation == EventActivation.Arrive && mapEvent.OnlyWhenLeaderId == leaderId)
            .ToList();
}
