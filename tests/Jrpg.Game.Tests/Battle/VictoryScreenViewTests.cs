using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

public sealed class VictoryScreenViewTests
{
    [Fact]
    public void RewardRows_NoGold_IsExperienceOnly()
    {
        var rows = VictoryScreenView.RewardRows(new VictoryScreen(new BattleSpoils(new VictoryRewards(120, []), Drops.None, Gold: 0)));

        Assert.Equal([new VictoryRewardRow(MessageIds.StatusExp, 120, IsGold: false)], rows);
    }

    [Fact]
    public void RewardRows_WithGold_ListsExperienceThenGold()
    {
        var rows = VictoryScreenView.RewardRows(new VictoryScreen(new BattleSpoils(new VictoryRewards(120, []), Drops.None, Gold: 27)));

        Assert.Equal([new VictoryRewardRow(MessageIds.StatusExp, 120, IsGold: false), new VictoryRewardRow(MessageIds.BattleVictoryGold, 27, IsGold: true)], rows);
    }

    // Decision 149: a full party of five takes a third row of cards, which must still clear the footer's line.
    [Fact]
    public void CardTopLeft_LastCardOfAFullParty_EndsAboveTheFooterLine()
    {
        var top = VictoryScreenView.CardTopLeft(PartyRules.MostActiveMembers - 1).Y;

        Assert.True(top + VictoryScreenView.CardHeight <= VictoryScreenView.FooterLineTop, $"The last card ends at {top + VictoryScreenView.CardHeight}.");
    }
}
