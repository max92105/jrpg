using Jrpg.Core.Battle;
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
}
