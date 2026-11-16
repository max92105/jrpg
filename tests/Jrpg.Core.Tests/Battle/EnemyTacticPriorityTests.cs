using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// An eligible priority rule is chosen before any draw, the first in the list among several (decision 841). The skills
// hit every foe, so aiming draws nothing, and each sequence holds exactly the draws expected: a pick that draws when it
// shouldn't runs it out.
public sealed class EnemyTacticPriorityTests
{
    private static readonly SkillDefinition _cleave = Skill("cleave", new Effect(EffectKind.Damage, 10, Targeting.AllEnemies));
    private static readonly SkillDefinition _crush = Skill("crush", new Effect(EffectKind.Damage, 12, Targeting.AllEnemies));
    private static readonly SkillDefinition _strike = Skill("strike", new Effect(EffectKind.Damage, 7, Targeting.SingleEnemy));
    private static readonly Dictionary<string, SkillDefinition> _skills =
        new[] { _cleave, _crush, _strike }.ToDictionary(skill => skill.Id);

    // Listed after a heavier rule, so a first-eligible pick or a weighted one fails.
    [Fact]
    public void ChooseCommand_EligiblePriorityRule_ChosenWithoutADraw()
    {
        var battle = EnemyFirst(Enemy().WithTactics(Rule(_cleave, weight: 3), Priority(_crush, weight: 1)));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal(_crush, SkillOf(command));
    }

    [Fact]
    public void ChooseCommand_TwoEligiblePriorityRules_TheFirstInTheListWins()
    {
        var battle = EnemyFirst(Enemy().WithTactics(Priority(_cleave, weight: 1), Priority(_crush, weight: 9)));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal(_cleave, SkillOf(command));
    }

    // At full HP the priority rule's condition fails; the draw of 1 passes the attack's share, landing in cleave's.
    [Fact]
    public void ChooseCommand_PriorityRuleNotEligible_WeightedDrawAmongTheRest()
    {
        var belowHalf = Priority(_crush, weight: 1) with { When = new TacticCondition(TacticConditionKind.OwnHpBelow, 50) };
        var battle = EnemyFirst(Enemy().WithTactics(belowHalf, new EnemyAction { Weight = 1 }, Rule(_cleave, weight: 3)));

        var command = Tactics(1).ChooseCommand(battle);

        Assert.Equal(_cleave, SkillOf(command));
    }

    // The member's hit takes the enemy below half, into the phase; the list from the start holds a priority rule too.
    [Fact]
    public void ChooseCommand_PriorityRuleInAPhase_ReadFromThePhase()
    {
        var phase = new EnemyPhase { BelowHpPercent = 50, Tactics = [Rule(_cleave, weight: 9), Priority(_crush, weight: 1)] };
        var enemy = Combatant.FromEnemy(Enemy().WithTactics(Priority(_strike, weight: 1)).WithPhases(phase).Build());
        var battle = new BattleState([Member(attack: 60), enemy], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new AttackCommand(enemy));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal(_crush, SkillOf(command));
    }

    // Without a condition "that one" names nobody, so the priority rule isn't eligible; cleave holds the whole weight.
    [Fact]
    public void ChooseCommand_PriorityRuleWithItsTargetMissing_NotChosen()
    {
        var onThatOne = Priority(_strike, weight: 1) with { On = TacticTarget.Subject };
        var battle = EnemyFirst(Enemy().WithTactics(onThatOne, Rule(_cleave, weight: 1)));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal(_cleave, SkillOf(command));
    }

    private static EnemyTactics Tactics(params int[] draws) => new(new FixedSequenceRandom(draws), _skills);

    // With two combatants the shuffle's 0 swaps them, so the enemy acts first.
    private static BattleState EnemyFirst(EnemyDefinitionBuilder enemy) =>
        new([Member(attack: 10), Combatant.FromEnemy(enemy.Build())], new Inventory(), new FixedSequenceRandom(0));

    private static EnemyDefinitionBuilder Enemy() =>
        new EnemyDefinitionBuilder().WithName("Chief").WithStats(maxHp: 100, damage: 5);

    private static Combatant Member(int attack) => Combatant.FromPartyMember(new PartyMemberBuilder().WithAttack(attack).Build());

    private static SkillDefinition SkillOf(BattleCommand command) => Assert.IsType<SkillCommand>(command).Skill;

    private static EnemyAction Rule(SkillDefinition skill, int weight) => new() { SkillId = skill.Id, Weight = weight };

    private static EnemyAction Priority(SkillDefinition skill, int weight) => Rule(skill, weight) with { IsPriority = true };

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
