using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// An action is eligible only while its condition holds (decision 402). The enemy's one action here is Cleave, which
// draws nothing, so a command is the Cleave when the condition holds, and otherwise the attack (decision 403) with its
// one draw.
public sealed class EnemyTacticConditionTests
{
    private static readonly SkillDefinition _cleave = new()
    {
        Id = "cleave",
        Name = "Cleave",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 10, Targeting.AllEnemies),
    };

    private readonly Combatant _ally = Enemy("Ally");
    private readonly Combatant _member = Member("sarxian");
    private readonly Combatant _otherMember = Member("jazog");

    [Fact]
    public void ChooseCommand_OwnHpBelowAndHurt_ActionIsEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.OwnHpBelow, 50));
        actor.Vitals.TakeDamage(51);

        Assert.IsType<SkillCommand>(ChooseFor(actor, draws: []));
    }

    [Fact]
    public void ChooseCommand_OwnHpAtTheThreshold_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.OwnHpBelow, 50));
        actor.Vitals.TakeDamage(50);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    [Fact]
    public void ChooseCommand_AllyHpBelowAndALivingAllyHurt_ActionIsEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyHpBelow, 50));
        _ally.Vitals.TakeDamage(51);

        Assert.IsType<SkillCommand>(ChooseFor(actor, draws: []));
    }

    [Fact]
    public void ChooseCommand_AllyHpAtTheThreshold_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyHpBelow, 50));
        _ally.Vitals.TakeDamage(50);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    [Fact]
    public void ChooseCommand_AllyHpBelowOnlyItselfHurt_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyHpBelow, 50));
        actor.Vitals.TakeDamage(99);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    [Fact]
    public void ChooseCommand_AllyHpBelowOnlyAFallenAlly_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyHpBelow, 50));
        _ally.Vitals.TakeDamage(100);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    [Fact]
    public void ChooseCommand_AllyFallen_ActionIsEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyFallen));
        _ally.Vitals.TakeDamage(100);

        Assert.IsType<SkillCommand>(ChooseFor(actor, draws: []));
    }

    [Fact]
    public void ChooseCommand_NoAllyFallen_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyFallen));
        _ally.Vitals.TakeDamage(99);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    [Fact]
    public void ChooseCommand_AllyHpBelowOnlyAMemberHurt_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyHpBelow, 50));
        _member.Vitals.TakeDamage(_member.Vitals.MaxHp - 1);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    [Fact]
    public void ChooseCommand_OnlyAMemberFallen_ActionIsNotEligible()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.AllyFallen));
        _member.Vitals.TakeDamage(_member.Vitals.MaxHp);

        Assert.IsType<AttackCommand>(ChooseFor(actor, draws: [0]));
    }

    // Shuffle values (0, 2, 1) swap the last combatant to the front, so the actor is first.
    private BattleCommand ChooseFor(Combatant actor, int[] draws)
    {
        var battle = new BattleState([_member, _otherMember, _ally, actor], new Inventory(), new FixedSequenceRandom(0, 2, 1));
        var skills = new Dictionary<string, SkillDefinition> { [_cleave.Id] = _cleave };
        return new EnemyTactics(new FixedSequenceRandom(draws), skills).ChooseCommand(battle);
    }

    private static Combatant Actor(TacticCondition condition) =>
        Combatant.FromEnemy(EnemyBuilder("Actor").WithTactics(new EnemyAction { SkillId = _cleave.Id, Weight = 1, When = condition }).Build());

    private static Combatant Member(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());

    private static Combatant Enemy(string name) => Combatant.FromEnemy(EnemyBuilder(name).Build());

    private static EnemyDefinitionBuilder EnemyBuilder(string name) => new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp: 100, damage: 5);
}
