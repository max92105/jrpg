using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// A warp from the map and a rest sound as their fade to black starts: a warp out of a cutscene's black is silent
// (decision 434), and the inn's question, its No and a refusal are silent; its paid Yes sounds the rest (decision 435).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterFadeSoundTests
{
    private const int Price = 20;

    [Fact]
    public void Update_WarpFromTheMap_AsksTheWarpSound()
    {
        var (_, presenter) = PresenterAtTheForestWarp();

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal([UiSoundFiles.Field.Warp], presenter.Sounds);
    }

    // The fade out is 0.25 s: the first later update is still going black, the second reaches the black hold, where the
    // warp moves the player.
    [Fact]
    public void Update_WarpUnderway_AsksNothing()
    {
        var (exploration, presenter) = PresenterAtTheForestWarp();
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(TimeSpan.FromSeconds(0.2), default);
        var goingBlack = presenter.Sounds.ToList();
        presenter.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Empty(goingBlack);
        Assert.Equal("forest", exploration.CurrentMap.Id);
        Assert.Empty(presenter.Sounds);
    }

    [Fact]
    public void Update_WarpAfterACutscene_AsksNothing()
    {
        Assert.DoesNotContain(UiSoundFiles.Field.Warp, SoundsThroughTheIntro(_ => default));
    }

    // A skip while a still shows ends the stills at once and goes black into the warp another way, through the still's
    // own update.
    [Fact]
    public void Update_WarpAfterASkippedCutscene_AsksNothing()
    {
        var skip = Pressing(GameButton.Confirm);

        Assert.DoesNotContain(UiSoundFiles.Field.Warp, SoundsThroughTheIntro(presenter => presenter.IsShowingStill ? skip : default));
    }

    [Fact]
    public void Update_FreeRestStarts_AsksTheRestSound()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowRestingChest);
        Assert.True(exploration.TryInteract());
        var presenter = NewPresenter(exploration);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal([UiSoundFiles.Field.Rest], presenter.Sounds);
    }

    [Fact]
    public void Update_InnQuestionOpens_AsksNothing()
    {
        var (_, presenter) = AtTheInnQuestion();

        Assert.NotNull(presenter.Question);
        Assert.Empty(presenter.Sounds);
    }

    // The question starts on No, so Up moves to Yes.
    [Fact]
    public void Update_YesAtTheInn_AsksTheRestSound()
    {
        var (_, presenter) = AtTheInnQuestion();
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Up));

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Field.Rest], presenter.Sounds);
    }

    [Fact]
    public void Update_YesAtTheInnShortOfGold_AsksNothing()
    {
        var (session, presenter) = AtTheInnQuestion();
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold - (Price - 1)));
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Up));

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(presenter.Question);
        Assert.Equal(0f, presenter.Blackness);
        Assert.Empty(presenter.Sounds);
    }

    [Fact]
    public void Update_NoAtTheInn_AsksNothing()
    {
        var (session, presenter) = AtTheInnQuestion();
        var gold = session.Party.Purse.Gold;

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(presenter.Question);
        Assert.Equal(gold, session.Party.Purse.Gold);
        Assert.Empty(presenter.Sounds);
    }

    // The intro's stills end into the warp to the town, black all along; one game step at a time, since a step longer
    // than the warp's 0.08 s hold at black would jump over it and fade twice.
    private static List<string> SoundsThroughTheIntro(Func<EventPresenter, InputFrame> inputFor)
    {
        var exploration = GameSession.StartNew(StartMapScenarios.TestWorld, "ragnir", new SeededRandom(1)).Exploration;
        var presenter = NewPresenter(exploration);
        var asked = new List<string>();
        for (var update = 0; update < 5000 && exploration.CurrentMap.Id == "intro"; update++)
        {
            presenter.Update(StartMapScenarios.OneUpdate, inputFor(presenter));
            asked.AddRange(presenter.Sounds);
        }

        Assert.Equal("start", exploration.CurrentMap.Id);
        return asked;
    }

    // Walks left off the town's edge until its warp runs, as EventPresenterTests does.
    private static (ExplorationState Exploration, EventPresenter Presenter) PresenterAtTheForestWarp()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && !exploration.IsEventRunning; update++)
        {
            exploration.Walk(StartMapScenarios.HoldingLeft.Move, StartMapScenarios.OneUpdate);
        }

        Assert.IsType<WarpStep>(exploration.CurrentEventStep);
        return (exploration, NewPresenter(exploration));
    }

    private static (GameSession Session, EventPresenter Presenter) AtTheInnQuestion()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowInnChest);
        Assert.True(session.Exploration.TryInteract());
        var presenter = NewPresenter(session.Exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        return (session, presenter);
    }

    private static EventPresenter NewPresenter(ExplorationState exploration) =>
        new(exploration, new EventLook(RepositoryScreenText.Text, new UiTextures()), new PlayerSettings(), new VoiceChannel());
}
