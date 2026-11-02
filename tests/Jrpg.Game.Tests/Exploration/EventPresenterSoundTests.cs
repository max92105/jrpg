using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// A page typing out ticks (decision 422): none at Instant, none under a voice, none when Confirm shows the rest.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterSoundTests
{
    private readonly VoiceChannel _voice = new();

    [Fact]
    public void Update_DialogueTypingAtNormalSpeed_AsksTheTypingSound()
    {
        var presenter = Reading(StartMapScenarios.BelowTalkingChest, TextSpeed.Normal);

        var asked = SoundsOverAQuarterSecond(presenter);

        Assert.Contains(UiSoundFiles.Effects.TextTyping, asked);
    }

    [Fact]
    public void Update_DialogueAtInstantSpeed_AsksNothing()
    {
        var presenter = Reading(StartMapScenarios.BelowTalkingChest, TextSpeed.Instant);

        Assert.Empty(SoundsOverAQuarterSecond(presenter));
    }

    [Fact]
    public void Update_DialogueWhileAVoicePlays_AsksNothing()
    {
        var voiced = StartMapScenarios.WithChestSaying(
            StartMapScenarios.BelowChest,
            new VoiceStep("Placeholder.wav"),
            new DialogueStep(new Speech(["A voice from the chest."])));
        var presenter = Reading(voiced, TextSpeed.Normal);

        Assert.Equal("Placeholder.wav", _voice.VoiceFile);
        Assert.Empty(SoundsOverAQuarterSecond(presenter));
    }

    // About a character per update at Fast, so every second update crosses a multiple: the gap spaces the ticks
    // (decision 422).
    [Fact]
    public void Update_DialogueAtFastSpeed_SpacesItsTicksByTheGap()
    {
        var presenter = Reading(StartMapScenarios.BelowTalkingChest, TextSpeed.Fast);
        var ticks = new List<int>();
        for (var update = 0; update < 20; update++)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
            if (presenter.Sounds.Contains(UiSoundFiles.Effects.TextTyping))
            {
                ticks.Add(update);
            }
        }

        Assert.True(ticks.Count > 1, $"Only {ticks.Count} tick(s) in 20 updates.");
        Assert.All(ticks.Zip(ticks.Skip(1)), pair => Assert.True(pair.Second - pair.First >= 4, $"Ticks at updates {pair.First} and {pair.Second}."));
    }

    // Confirm shows the rest of the page at once, with no burst of ticks.
    [Fact]
    public void Update_ConfirmMidPage_AsksNothing()
    {
        var presenter = Reading(StartMapScenarios.BelowTalkingChest, TextSpeed.Normal);

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));

        Assert.Empty(presenter.Sounds);
    }

    private static List<string> SoundsOverAQuarterSecond(EventPresenter presenter)
    {
        var asked = new List<string>();
        for (var update = 0; update < 15; update++)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
            asked.AddRange(presenter.Sounds);
        }

        return asked;
    }

    // The first update starts the event's first steps, so the page is on screen and typing from the next.
    private EventPresenter Reading(Core.Content.ContentCatalog catalog, TextSpeed speed)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        Assert.True(exploration.TryInteract());
        var look = new EventLook(RepositoryScreenText.Text, new UiTextures());
        var presenter = new EventPresenter(exploration, look, new PlayerSettings { TextSpeed = speed }, _voice);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        Assert.True(presenter.IsShowingMessage);
        return presenter;
    }
}
