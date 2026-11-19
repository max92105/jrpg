using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// A still's subtitles over the map: each line shows at its turn in the still's sequence, nothing during a pause or
/// once the last line's time is over; at the game's 1/60 s step.
/// </summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterCinematicCaptionTests
{
    [Fact]
    public void Update_FirstStillStarted_ShowsItsLine()
    {
        Assert.Equal("Long ago.", CaptionAfter(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Update_PastTheOnlyLinesTime_ShowsNothingWhileTheStillStays()
    {
        Assert.Null(CaptionAfter(TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public void Update_SecondStillStarted_ShowsItsFirstLine()
    {
        Assert.Equal("Far away.", CaptionAfter(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void Update_InThePauseBetweenTwoLines_ShowsNothing()
    {
        Assert.Null(CaptionAfter(TimeSpan.FromSeconds(6.25)));
    }

    [Fact]
    public void Update_PastThePause_ShowsTheNextLine()
    {
        Assert.Equal("So far.", CaptionAfter(TimeSpan.FromSeconds(7)));
    }

    [Fact]
    public void Update_PastTheLastStill_ShowsNothing()
    {
        Assert.Null(CaptionAfter(TimeSpan.FromSeconds(8.5)));
    }

    // The chest's cinematic, its pictures loaded, this long after the black before its first still.
    private static string? CaptionAfter(TimeSpan intoCinematic)
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestPlayingCinematic);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.MarkStillsLoaded();
        for (var elapsed = TimeSpan.Zero; elapsed < CinematicRun.LeadIn + intoCinematic; elapsed += StartMapScenarios.OneUpdate)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }

        return presenter.ShownCinematicCaption;
    }
}
