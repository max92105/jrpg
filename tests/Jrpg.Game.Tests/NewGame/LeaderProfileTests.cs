using Jrpg.Core.Content;
using Jrpg.Game.NewGame;
using Jrpg.Game.Tests.Battle;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.NewGame;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class LeaderProfileTests
{
    [Fact]
    public void Of_Character_ListsLevelHpSpeedAndPrimaries()
    {
        var messages = RepositoryScreenText.Text.Messages;

        var profile = LeaderProfile.Of(new LeaderChoice { MemberId = "omer" }, BattleScenarios.Catalog, messages);

        string[] ids =
        [
            MessageIds.StatusLevel, MessageIds.StatHp, MessageIds.StatusSpeed,
            MessageIds.StatPower, MessageIds.StatAgility, MessageIds.StatIntelligence, MessageIds.StatConstitution, MessageIds.StatWill,
        ];
        string[] expected = [.. ids.Select(messages.Text)];
        Assert.Equal(expected, profile.Stats.Select(line => line.Label));
    }
}
