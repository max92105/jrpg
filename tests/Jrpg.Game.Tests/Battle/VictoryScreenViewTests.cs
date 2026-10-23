using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Game.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

public sealed class VictoryScreenViewTests
{
    [Fact]
    public void RewardRows_ExperienceOnly_IsOneRow()
    {
        var rows = VictoryScreenView.RewardRows(new VictoryRewards(120, []));

        Assert.Equal([new VictoryRewardRow(MessageIds.StatusExp, 120)], rows);
    }

    // Decision 149: a full party of five takes a third row of cards, which must still clear the footer's line.
    [Fact]
    public void CardTopLeft_LastCardOfAFullParty_EndsAboveTheFooterLine()
    {
        var top = VictoryScreenView.CardTopLeft(PartyRules.MostActiveMembers - 1).Y;

        Assert.True(top + VictoryScreenView.CardHeight <= VictoryScreenView.FooterLineTop, $"The last card ends at {top + VictoryScreenView.CardHeight}.");
    }
}
