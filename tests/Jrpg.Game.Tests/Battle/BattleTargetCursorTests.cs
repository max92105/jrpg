using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattleTargetCursorTests
{
    private readonly BattleState _battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    [Fact]
    public void Start_SingleTargetChoice_SelectsLastLivingEnemy()
    {
        var cursor = new BattleTargetCursor(_battle.TargetChoiceForAttack(), _battle.CurrentActor);

        Assert.Equal([_battle.Combatants[^1]], cursor.Selected);
    }

    [Fact]
    public void MoveNext_OnLastCandidate_WrapsToFirst()
    {
        var cursor = new BattleTargetCursor(_battle.TargetChoiceForAttack(), _battle.CurrentActor);

        cursor.MoveNext();

        Assert.Equal([_battle.Combatants[1]], cursor.Selected);
    }

    [Fact]
    public void TakesAll_SelectsEveryCandidate()
    {
        var enemies = _battle.LivingOn(BattleSide.Enemies);

        var cursor = new BattleTargetCursor(new TargetChoice<Combatant>(enemies, TakesAll: true), _battle.CurrentActor);

        Assert.Equal(enemies, cursor.Selected);
    }

    [Fact]
    public void Start_ActorIsCandidate_SelectsActor()
    {
        var cursor = new BattleTargetCursor(new TargetChoice<Combatant>(_battle.Combatants, TakesAll: false), _battle.CurrentActor);

        Assert.Equal([_battle.CurrentActor], cursor.Selected);
    }

    [Fact]
    public void Start_AllyListedAfterEnemies_SelectsLastEnemy()
    {
        var (sarxian, goblinA, goblinB, goblinC) = (_battle.Combatants[0], _battle.Combatants[1], _battle.Combatants[2], _battle.Combatants[3]);

        var cursor = new BattleTargetCursor(new TargetChoice<Combatant>([goblinA, goblinB, sarxian], TakesAll: false), goblinC);

        Assert.Equal([goblinB], cursor.Selected);
    }

    [Fact]
    public void MovePrevious_OnFirstCandidate_WrapsToLast()
    {
        var cursor = new BattleTargetCursor(_battle.TargetChoiceForAttack(), _battle.CurrentActor);
        cursor.MoveNext();

        cursor.MovePrevious();

        Assert.Equal([_battle.Combatants[^1]], cursor.Selected);
    }
}
