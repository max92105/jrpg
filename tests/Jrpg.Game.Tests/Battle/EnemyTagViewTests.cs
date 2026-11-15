using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// Where an enemy's name tag lies, read by the numbers above it (decision 651); on Sarxian against the goblin trio.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EnemyTagViewTests
{
    private readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    [Fact]
    public void TagAreaOf_Enemy_EndsBelowItsSpot()
    {
        var area = Tags().TagAreaOf(_battle.Combatants[1]);

        Assert.Equal(BattleLayout.EnemySpot(0).Bottom + 38, area?.Bottom);
    }

    [Fact]
    public void TagAreaOf_PartyMember_IsNone()
    {
        Assert.Null(Tags().TagAreaOf(_battle.Combatants[0]));
    }

    private EnemyTagView Tags()
    {
        var tactics = new EnemyTactics(new SeededRandom(1), new Dictionary<string, SkillDefinition>());
        var sequence = new BattleTurnSequence(_battle, tactics, RepositoryScreenText.Text.Messages);
        return new EnemyTagView(_battle.Combatants, sequence, RepositoryScreenText.Text, new UiTextures());
    }
}
