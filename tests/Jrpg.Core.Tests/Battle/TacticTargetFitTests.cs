using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Tests.Battle;

// Which target fits which aim after which trigger (decision 840, design.md section 3.3); a null aim is the attack.
public sealed class TacticTargetFitTests
{
    [Theory]
    [InlineData(TacticTarget.Self, null, Targeting.Self, true)]
    [InlineData(TacticTarget.Self, null, Targeting.SingleEnemy, false)]
    [InlineData(TacticTarget.LowestHpFoe, null, null, true)]
    [InlineData(TacticTarget.LowestHpFoe, null, Targeting.SingleAlly, false)]
    [InlineData(TacticTarget.HighestHpFoe, null, Targeting.AllEnemies, false)]
    [InlineData(TacticTarget.LowestHpOnItsSide, null, Targeting.SingleAlly, true)]
    [InlineData(null, null, Targeting.AllEnemies, true)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.AllyHpBelow, Targeting.SingleAlly, true)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.AllyHpBelow, Targeting.SingleEnemy, false)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.AllyFallen, Targeting.SingleFallenAlly, true)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.OwnHpBelow, Targeting.Self, true)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.FoeHpBelow, Targeting.SingleEnemy, true)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.FoeHpBelow, Targeting.SingleAlly, false)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.EveryNthTurn, Targeting.Self, true)]
    [InlineData(TacticTarget.Subject, TacticConditionKind.EveryNthTurn, Targeting.SingleEnemy, false)]
    public void Fits_TargetTriggerAndAim_MatchesTheTable(
        TacticTarget? target, TacticConditionKind? kind, Targeting? aim, bool expected)
    {
        var condition = kind is { } named ? new TacticCondition(named, 50) : null;
        var action = new EnemyAction { Weight = 1, When = condition, On = target };

        var fits = TacticTargetFit.Fits(action, aim is { } aimed ? Skill(aimed) : null);

        Assert.Equal(expected, fits);
    }

    // "That one" without a trigger names nobody, whatever the skill aims at.
    [Fact]
    public void Fits_SubjectWithoutCondition_IsFalse()
    {
        var action = new EnemyAction { Weight = 1, On = TacticTarget.Subject };
        var skills = Enum.GetValues<Targeting>().Select(aim => (SkillDefinition?)Skill(aim)).Append(null);

        Assert.All(skills, skill => Assert.False(TacticTargetFit.Fits(action, skill)));
    }

    // The fit switches over the targets and the triggers' subjects, so a member without an arm would throw here.
    [Fact]
    public void Fits_EveryTargetAfterEveryKind_Answers()
    {
        var actions = Enum.GetValues<TacticTarget>().SelectMany(target => Enum.GetValues<TacticConditionKind>()
            .Select(kind => new EnemyAction { Weight = 1, When = new TacticCondition(kind, 50), On = target }));

        Assert.All(actions, action => TacticTargetFit.Fits(action, null));
    }

    private static SkillDefinition Skill(Targeting aim) => new()
    {
        Id = "skill",
        Name = "Skill",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 5, aim),
    };
}
