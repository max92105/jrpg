using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// Skipping a cinematic (decision 623): Confirm held for the hold time skips to what follows; a release before carries
/// on, Cancel does nothing, and the Confirm that started the event never counts. On the chest's cinematic, then the
/// line it says after.
/// </summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterCinematicSkipTests
{
    private static readonly InputFrame _hold = InputFrames.Holding(GameButton.Confirm);

    [Fact]
    public void Update_ConfirmHeldTwoSeconds_SkipsToTheDialogue()
    {
        var (exploration, presenter, _) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);
        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Run(presenter, SkipHold.HoldTime, _hold);

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Null(presenter.ShownCinematicImage);
    }

    [Fact]
    public void Update_ConfirmReleasedAtOneSecond_CinematicCarriesOn()
    {
        var (exploration, presenter, _) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);
        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        Run(presenter, TimeSpan.FromSeconds(1), _hold);

        Run(presenter, TimeSpan.FromSeconds(2), default);

        Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
        Assert.Equal("a.png", presenter.ShownCinematicImage);
    }

    [Fact]
    public void Update_CancelPressed_DoesNothing()
    {
        var (exploration, presenter, _) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);

        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Cancel));
        Run(presenter, SkipHold.HoldTime, InputFrames.Holding(GameButton.Cancel));

        Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_DuringACinematic_HintAsksForAHold()
    {
        var (_, presenter, _) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);

        Assert.Equal("Hold E to skip", presenter.SkipHint);
    }

    // The press that started the event, held on through the cinematic's first seconds.
    [Fact]
    public void Update_ConfirmHeldFromTheEventsStart_DoesNotSkip()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestPlayingCinematic);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, _hold);
        presenter.MarkStillsLoaded();

        Run(presenter, TimeSpan.FromSeconds(3), _hold);

        Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_HeldSkip_SilencesTheVoice()
    {
        var voiced = StartMapScenarios.BelowChestPlayingCinematic.Cinematics["test_cinematic"] with
        {
            Stills = [new CinematicStill { ImageFile = "a.png", Seconds = 4, VoiceFile = "one.wav" }],
        };
        var catalog = StartMapScenarios.WithChestSaying(
            StartMapScenarios.BelowChest with
            {
                Cinematics = new Dictionary<string, CinematicDefinition> { [voiced.Id] = voiced },
            },
            new CinematicStep(voiced.Id),
            new DialogueStep(new Speech(["The chest falls silent."])));
        var (_, presenter, voice) = StartPastTheLeadIn(catalog);
        Assert.Equal("one.wav", voice.VoiceFile);
        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Run(presenter, SkipHold.HoldTime, _hold);

        Assert.Null(voice.VoiceFile);
    }

    // The chest's event started, its pictures loaded, past the black before the first still.
    private static (ExplorationState Exploration, EventPresenter Presenter, VoiceChannel Voice) StartPastTheLeadIn(
        ContentCatalog catalog)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        exploration.TryInteract();
        var voice = new VoiceChannel();
        var presenter = EventPresenters.Over(exploration, voice: voice);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.MarkStillsLoaded();
        Run(presenter, CinematicRun.LeadIn + StartMapScenarios.OneUpdate, default);
        return (exploration, presenter, voice);
    }

    // At the game's fixed step, so no phase is jumped over.
    private static void Run(EventPresenter presenter, TimeSpan time, InputFrame input)
    {
        for (var elapsed = TimeSpan.Zero; elapsed < time; elapsed += StartMapScenarios.OneUpdate)
        {
            presenter.Update(StartMapScenarios.OneUpdate, input);
        }
    }
}
