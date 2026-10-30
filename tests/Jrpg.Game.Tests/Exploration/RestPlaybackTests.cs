using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// Which line the rest's page says is the playback's choice: the party rests once paid, the refusal when Yes came short
// of gold (decision 382).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class RestPlaybackTests
{
    [Fact]
    public void Answer_YesWithTheGold_FadesAndSaysThePartyRests()
    {
        var (playback, _) = AskingAtTheInn(gold: 20);

        playback.Answer(Pressing(GameButton.Up));
        playback.Answer(Pressing(GameButton.Confirm));

        Assert.Null(playback.Question);
        Assert.True(playback.IsFading);
        Assert.Equal(MessageIds.PartyRests, playback.LineId);
    }

    [Fact]
    public void Answer_YesShortOfGold_SaysTheRefusalWithoutFading()
    {
        var (playback, stay) = AskingAtTheInn(gold: 19);

        playback.Answer(Pressing(GameButton.Up));
        playback.Answer(Pressing(GameButton.Confirm));

        Assert.Null(playback.Question);
        Assert.False(playback.IsFading);
        Assert.False(playback.IsDeclined);
        Assert.Equal(MessageIds.InnShortOfGold, playback.LineId);
        Assert.False(stay.CanPay);
    }

    [Fact]
    public void Answer_No_Declines()
    {
        var (playback, _) = AskingAtTheInn(gold: 20);

        playback.Answer(Pressing(GameButton.Confirm));

        Assert.Null(playback.Question);
        Assert.True(playback.IsDeclined);
        Assert.False(playback.IsFading);
    }

    // The inn chest's stay (20 gold) reached through its event, the purse holding the given gold.
    private static (RestPlayback Playback, InnStay Stay) AskingAtTheInn(int gold)
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowInnChest);
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));
        session.Party.Purse.Add(gold);
        session.Exploration.TryInteract();
        var stay = session.Exploration.CurrentInnStay!;
        var playback = new RestPlayback(RepositoryScreenText.Text.Messages);
        playback.Start(stay);
        Assert.NotNull(playback.Question);
        return (playback, stay);
    }
}
