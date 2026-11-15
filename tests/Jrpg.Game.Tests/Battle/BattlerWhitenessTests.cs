using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// How white a battler draws (decisions 648, 649), on Sarxian attacking the goblin trio, stepped to his first blink.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class BattlerWhitenessTests
{
    private const float Strength = 0.6f;

    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];

    [Fact]
    public void Of_FallenBattler_IsTheFallensWhite()
    {
        var fallen = Combatant.FromEnemy(StartMapScenarios.TestWorld.Enemies["goblin"]);
        fallen.Vitals.TakeDamage(9999);

        Assert.Equal(Strength, BattlerWhiteness.Of(fallen, AttackAtTheFirstBlink()));
    }

    [Fact]
    public void Of_ActorMidBlink_IsTheBlinksWhite()
    {
        Assert.Equal(Strength, BattlerWhiteness.Of(_sarxian, AttackAtTheFirstBlink()));
    }

    [Fact]
    public void Of_Bystander_IsNone()
    {
        Assert.Equal(0, BattlerWhiteness.Of(_goblin, AttackAtTheFirstBlink()));
    }

    [Fact]
    public void Of_NoWindUp_IsNone()
    {
        Assert.Equal(0, BattlerWhiteness.Of(_sarxian, null));
    }

    private static ActionWindUp AttackAtTheFirstBlink()
    {
        var windUp = ActionWindUp.Of(
            _sarxian, new AttackCommand(_goblin),
            RepositoryScreenText.Text.Messages,
            BattleScenarios.NoEffectStrips);
        windUp.Update(TimeSpan.FromSeconds(0.10));
        return windUp;
    }
}
