using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// Where a damaging action's impact strip plays its lead while it winds up (decisions 656, 657): on Sarxian against the
// goblin trio, the skills built in the test so their effects are known.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionWindUpEffectTests
{
    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];
    private static readonly Combatant _secondGoblin = _battle.Combatants[2];

    [Fact]
    public void Of_Attack_AimsAtItsTarget()
    {
        Assert.Equal([_goblin], WindUpOf(_sarxian, new AttackCommand(_goblin)).AimedTargets);
    }

    [Fact]
    public void Of_DamagingSkill_AimsAtItsTargets()
    {
        var sweep = new SkillCommand(SoundingSkills.WithSound(""), [_goblin, _secondGoblin]);

        Assert.Equal([_goblin, _secondGoblin], WindUpOf(_sarxian, sweep).AimedTargets);
    }

    [Fact]
    public void Of_HealingSkill_AimsAtNoOne()
    {
        var mend = SoundingSkills.WithSound("") with { Effect = new Effect(EffectKind.RestoreHp, 5, Targeting.SingleAlly) };

        Assert.Empty(WindUpOf(_sarxian, new SkillCommand(mend, [_sarxian])).AimedTargets);
    }

    [Fact]
    public void Of_Defend_AimsAtNoOne()
    {
        Assert.Empty(WindUpOf(_sarxian, new DefendCommand()).AimedTargets);
    }

    [Fact]
    public void ImpactFile_Attack_IsTheAttackersStrip()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        Assert.Equal(_sarxian.Member!.Definition.AttackEffectFile, windUp.ImpactFile);
    }

    [Fact]
    public void ImpactFile_Defend_IsNone()
    {
        Assert.Null(WindUpOf(_sarxian, new DefendCommand()).ImpactFile);
    }

    [Fact]
    public void IsMirrored_PartyActor_IsTrue()
    {
        Assert.True(WindUpOf(_sarxian, new AttackCommand(_goblin)).IsMirrored);
    }

    [Fact]
    public void IsMirrored_EnemyActor_IsFalse()
    {
        Assert.False(WindUpOf(_goblin, new AttackCommand(_sarxian)).IsMirrored);
    }

    [Fact]
    public void SinceImpact_HalfwayThroughTheWindUp_IsNegative()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        windUp.Update(BattlePacing.WindUp / 2);

        Assert.Equal(-(BattlePacing.WindUp / 2), windUp.SinceImpact);
    }

    [Fact]
    public void SinceImpact_AtTheEnd_IsZero()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        windUp.Update(BattlePacing.WindUp);

        Assert.Equal(TimeSpan.Zero, windUp.SinceImpact);
    }

    private static ActionWindUp WindUpOf(Combatant actor, BattleCommand command) =>
        ActionWindUp.Of(actor, command, RepositoryScreenText.Text.Messages);
}
