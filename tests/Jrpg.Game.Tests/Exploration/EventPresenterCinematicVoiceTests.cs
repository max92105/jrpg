using Jrpg.Core.Cinematics;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// A cinematic's voices (decisions 613, 622): each still's voice starts with it and plays on until the next voiced
/// still or the end; an earlier voice step's line is cut by the cinematic's track at its start, or else by its first
/// voiced still, and doesn't come back. The chest's cinematic shows a.png (one.wav), b.png (no voice), c.png
/// (three.wav), 4 s each.
/// </summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterCinematicVoiceTests
{
    private static readonly TimeSpan _stillTime = TimeSpan.FromSeconds(4);
    private static readonly VoiceStep _oldVoice = new("old.wav");
    private static readonly DialogueStep _afterwards = new(new Speech(["The chest falls silent."]));

    [Fact]
    public void Update_LeadIn_AsksNoVoice()
    {
        var (_, presenter, voice) = Start(Voiced(), new CinematicStep("voiced"), _afterwards);
        presenter.MarkStillsLoaded();

        Run(presenter, CinematicRun.LeadIn / 2);

        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_FirstStillShown_AsksItsVoice()
    {
        var (_, presenter, voice) = StartPastTheLeadIn(Voiced(), new CinematicStep("voiced"), _afterwards);

        Assert.Equal("one.wav", voice.VoiceFile);
    }

    [Fact]
    public void Update_StillWithoutVoice_KeepsTheEarlierOne()
    {
        var (_, presenter, voice) = StartPastTheLeadIn(Voiced(), new CinematicStep("voiced"), _afterwards);

        Run(presenter, _stillTime);

        Assert.Equal("b.png", presenter.ShownCinematicImage);
        Assert.Equal("one.wav", voice.VoiceFile);
    }

    [Fact]
    public void Update_NextStillsVoice_ReplacesIt()
    {
        var (_, presenter, voice) = StartPastTheLeadIn(Voiced(), new CinematicStep("voiced"), _afterwards);

        Run(presenter, _stillTime * 2);

        Assert.Equal("three.wav", voice.VoiceFile);
    }

    [Fact]
    public void Update_LeadOut_SilencesTheVoice()
    {
        var (exploration, presenter, voice) = StartPastTheLeadIn(Voiced(), new CinematicStep("voiced"), _afterwards);

        Run(presenter, _stillTime * 3);

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_CinematicWithTrack_CutsTheEarlierVoiceAtItsStart()
    {
        var (_, _, voice) = Start(Voiced("Story.wav"), _oldVoice, new CinematicStep("voiced"), _afterwards);

        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_CinematicWithoutTrackOrVoices_EarlierVoicePlaysOn()
    {
        var (exploration, presenter, voice) = StartPastTheLeadIn(Silent(), _oldVoice, new CinematicStep("silent"), _afterwards);

        Run(presenter, _stillTime * 1.5);

        Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
        Assert.Equal("old.wav", voice.VoiceFile);
    }

    [Fact]
    public void Update_CinematicWithoutTrack_EarlierVoicePlaysUntilTheFirstVoicedStill()
    {
        var (_, presenter, voice) = Start(Voiced(), _oldVoice, new CinematicStep("voiced"), _afterwards);
        presenter.MarkStillsLoaded();
        Assert.Equal("old.wav", voice.VoiceFile);

        Run(presenter, CinematicRun.LeadIn + StartMapScenarios.OneUpdate);

        Assert.Equal("one.wav", voice.VoiceFile);
    }

    [Fact]
    public void Update_AfterTheCinematicCutIt_EarlierVoiceDoesNotComeBack()
    {
        var (exploration, presenter, voice) = StartPastTheLeadIn(Voiced(), _oldVoice, new CinematicStep("voiced"), _afterwards);

        Run(presenter, (_stillTime * 3) + CinematicRun.LeadOut + StartMapScenarios.OneUpdate);

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_VoiceStepAfterTheCinematic_Plays()
    {
        var steps = new EventStep[] { _oldVoice, new CinematicStep("voiced"), new VoiceStep("new.wav"), _afterwards };
        var (_, presenter, voice) = StartPastTheLeadIn(Voiced(), steps);

        Run(presenter, (_stillTime * 3) + CinematicRun.LeadOut + StartMapScenarios.OneUpdate);

        Assert.Equal("new.wav", voice.VoiceFile);
    }

    // The cut is forgotten when the event ends: the same steps run again play the earlier voice until the cut again.
    [Fact]
    public void Update_EventRunAgain_PlaysItsVoiceUntilTheCut()
    {
        var (exploration, presenter, voice) = StartPastTheLeadIn(Voiced(), _oldVoice, new CinematicStep("voiced"));
        Run(presenter, (_stillTime * 3) + CinematicRun.LeadOut + StartMapScenarios.OneUpdate);
        Assert.False(exploration.IsEventRunning);

        exploration.TryInteract();
        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal("old.wav", voice.VoiceFile);
    }

    private static CinematicDefinition Voiced(string? track = null) => new()
    {
        Id = "voiced",
        MusicFile = track,
        Stills =
        [
            new CinematicStill { ImageFile = "a.png", Seconds = 4, VoiceFile = "one.wav" },
            new CinematicStill { ImageFile = "b.png", Seconds = 4 },
            new CinematicStill { ImageFile = "c.png", Seconds = 4, VoiceFile = "three.wav" },
        ],
    };

    private static CinematicDefinition Silent() => new()
    {
        Id = "silent",
        Stills =
        [
            new CinematicStill { ImageFile = "a.png", Seconds = 4 },
            new CinematicStill { ImageFile = "b.png", Seconds = 4 },
        ],
    };

    // The chest's event started with these steps, its first update taken.
    private static (ExplorationState Exploration, EventPresenter Presenter, VoiceChannel Voice) Start(
        CinematicDefinition cinematic,
        params EventStep[] steps)
    {
        var withCinematic = StartMapScenarios.BelowChest with
        {
            Cinematics = new Dictionary<string, CinematicDefinition> { [cinematic.Id] = cinematic },
        };
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.WithChestSaying(withCinematic, steps));
        exploration.TryInteract();
        var voice = new VoiceChannel();
        var presenter = EventPresenters.Over(exploration, voice: voice);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        return (exploration, presenter, voice);
    }

    // Loaded and past the black before the first still, which has just begun.
    private static (ExplorationState Exploration, EventPresenter Presenter, VoiceChannel Voice) StartPastTheLeadIn(
        CinematicDefinition cinematic,
        params EventStep[] steps)
    {
        var started = Start(cinematic, steps);
        started.Presenter.MarkStillsLoaded();
        Run(started.Presenter, CinematicRun.LeadIn + StartMapScenarios.OneUpdate);
        return started;
    }

    // At the game's fixed step, so no phase is jumped over.
    private static void Run(EventPresenter presenter, TimeSpan time)
    {
        for (var elapsed = TimeSpan.Zero; elapsed < time; elapsed += StartMapScenarios.OneUpdate)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }
    }
}
