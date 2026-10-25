using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// Ragnir, a fighter, acts first against goblins A, B and C. Shield Bash on C stuns it for 1 turn, and it stays stunned
// while A and B act before it.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatusTagsTextTests
{
    private readonly BattleState _battle =
        BattleScenarios.MemberAgainstGoblinTrio("ragnir", new Inventory(), new SeededRandom(BattleScenarios.SarxianFirstSeed));

    [Fact]
    public void Refresh_StunWithOneTurn_ShowsNameAndTurns()
    {
        var tags = new StatusTagsText(GoblinC.Statuses, RepositoryScreenText.Text.Messages);
        BashGoblinC();

        tags.Refresh();

        Assert.Equal("STUN 1", tags.Line);
    }

    [Fact]
    public void Refresh_StatusEnded_RemovesTag()
    {
        var tags = new StatusTagsText(GoblinC.Statuses, RepositoryScreenText.Text.Messages);
        BashGoblinC();
        tags.Refresh();
        _battle.Execute(new DefendCommand());
        _battle.Execute(new DefendCommand());

        tags.Refresh();

        Assert.Empty(tags.Line);
    }

    [Fact]
    public void Refresh_Unchanged_KeepsSameString()
    {
        var tags = new StatusTagsText(GoblinC.Statuses, RepositoryScreenText.Text.Messages);
        tags.Refresh();
        BashGoblinC();
        tags.Refresh();
        var stunned = tags.Line;

        tags.Refresh();

        Assert.Equal("STUN 1", stunned);
        Assert.Same(stunned, tags.Line);
    }

    private Combatant GoblinC => _battle.Combatants[3];

    private void BashGoblinC() =>
        _battle.Execute(new SkillCommand(BattleScenarios.Catalog.Skills["shield_bash"], [GoblinC]));
}
