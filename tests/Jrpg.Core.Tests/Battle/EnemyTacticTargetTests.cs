using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A rule with a target aims by it (decision 834) and is eligible only when the target finds a legal one (decision 836).
// The rule is the enemy's only action, so the pick draws nothing, and each sequence holds exactly the draws expected:
// an extra draw runs it out.
public sealed class EnemyTacticTargetTests
{
    private static readonly SkillDefinition _mend = Skill("mend", new Effect(EffectKind.RestoreHp, 25, Targeting.SingleAlly));
    private static readonly SkillDefinition _strike = Skill("strike", new Effect(EffectKind.Damage, 7, Targeting.SingleEnemy));
    private static readonly SkillDefinition _revive =
        Skill("revive", new Effect(EffectKind.Revive, 10, Targeting.SingleFallenAlly));
    private static readonly Dictionary<string, SkillDefinition> _skills =
        new[] { _mend, _strike, _revive }.ToDictionary(skill => skill.Id);

    private static readonly TacticCondition _allyBelowHalf = new(TacticConditionKind.AllyHpBelow, 50);
    private static readonly TacticCondition _allyFallen = new(TacticConditionKind.AllyFallen);
    private static readonly TacticCondition _foeBelowHalf = new(TacticConditionKind.FoeHpBelow, 50);

    // The wounded ally is lower, which the automatic heal would take.
    [Fact]
    public void ChooseCommand_SelfOnASingleAllyHeal_AimsAtItself()
    {
        var actor = Actor(Rule(_mend, TacticTarget.Self), hp: 80);

        var command = ChooseFor(actor, [Member("sarxian"), Ally("Wounded", hp: 10)], draws: []);

        Assert.Equal(actor, TargetOf(command));
    }

    // 30 % is the lower share although 20 is the lower number.
    [Fact]
    public void ChooseCommand_LowestHpFoe_AimsAtTheLowestShare()
    {
        var lowestShare = Member("sarxian", hp: 30, maxHp: 100);
        var actor = Actor(Rule(_strike, TacticTarget.LowestHpFoe));

        var command = ChooseFor(actor, [lowestShare, Member("jazog", hp: 20, maxHp: 40)], draws: []);

        Assert.Equal(lowestShare, TargetOf(command));
    }

    // Both at 50 %, listed against a sort by HP and by id.
    [Fact]
    public void ChooseCommand_LowestHpFoeTie_AimsAtTheFirstInBattleOrder()
    {
        var first = Member("teelle", hp: 20, maxHp: 40);
        var actor = Actor(Rule(_strike, TacticTarget.LowestHpFoe));

        var command = ChooseFor(actor, [first, Member("sarxian", hp: 10, maxHp: 20)], draws: []);

        Assert.Equal(first, TargetOf(command));
    }

    // 90 % is the highest share although 80 is the highest number.
    [Fact]
    public void ChooseCommand_HighestHpFoe_AimsAtTheHighestShare()
    {
        var highestShare = Member("jazog", hp: 45, maxHp: 50);
        var actor = Actor(Rule(_strike, TacticTarget.HighestHpFoe));

        var command = ChooseFor(actor, [Member("sarxian", hp: 60), highestShare, Member("teelle", hp: 80)], draws: []);

        Assert.Equal(highestShare, TargetOf(command));
    }

    // Both at 90 %, listed against a sort by HP and by id.
    [Fact]
    public void ChooseCommand_HighestHpFoeTie_AimsAtTheFirstInBattleOrder()
    {
        var first = Member("teelle", hp: 90);
        var actor = Actor(Rule(_strike, TacticTarget.HighestHpFoe));

        var command = ChooseFor(actor, [first, Member("sarxian", hp: 45, maxHp: 50)], draws: []);

        Assert.Equal(first, TargetOf(command));
    }

    // The taunter's share is the highest; the roll of 500 fails the enemy's 50 % shrug-off.
    [Fact]
    public void ChooseCommand_LowestHpFoeWhileAMemberTaunts_AimsAtTheTaunter()
    {
        var taunter = Taunting(Member("jazog", hp: 90));
        var actor = Actor(Rule(_strike, TacticTarget.LowestHpFoe), crowdControlResistance: 50);

        var command = ChooseFor(actor, [Member("sarxian", hp: 30), taunter], draws: [500]);

        Assert.Equal(taunter, TargetOf(command));
    }

    // The roll of 499 shrugs the taunt off, so the lowest share stands.
    [Fact]
    public void ChooseCommand_LowestHpFoeTauntShruggedOff_AimsAtTheLowestShare()
    {
        var lowestShare = Member("sarxian", hp: 30);
        var actor = Actor(Rule(_strike, TacticTarget.LowestHpFoe), crowdControlResistance: 50);

        var command = ChooseFor(actor, [lowestShare, Taunting(Member("jazog", hp: 90))], draws: [499]);

        Assert.Equal(lowestShare, TargetOf(command));
    }

    // The ally below half met the condition, which the automatic heal would take; the actor is lower still.
    [Fact]
    public void ChooseCommand_LowestHpOnItsSide_AimsAtItselfWhenLowest()
    {
        var actor = Actor(Rule(_mend, TacticTarget.LowestHpOnItsSide, _allyBelowHalf), hp: 20);

        var command = ChooseFor(actor, [Member("sarxian", hp: 5), Ally("Ally", hp: 40)], draws: []);

        Assert.Equal(actor, TargetOf(command));
    }

    // The actor is the lowest of its side, but only the ally at 40 % met the condition.
    [Fact]
    public void ChooseCommand_SubjectOfAllyHpBelow_AimsAtThatAlly()
    {
        var metIt = Ally("Met", hp: 40);
        var actor = Actor(Rule(_mend, TacticTarget.Subject, _allyBelowHalf), hp: 10);

        var command = ChooseFor(actor, [Member("sarxian"), Ally("Above", hp: 60), metIt], draws: []);

        Assert.Equal(metIt, TargetOf(command));
    }

    [Fact]
    public void ChooseCommand_SubjectOfAllyFallen_AimsAtTheFallenAlly()
    {
        var fallen = Ally("Fallen", hp: 0);
        var actor = Actor(Rule(_revive, TacticTarget.Subject, _allyFallen));

        var command = ChooseFor(actor, [Member("sarxian"), Ally("Standing", hp: 50), fallen], draws: []);

        Assert.Equal(fallen, TargetOf(command));
    }

    // Only the member at 40 % met the condition; "that one" picks it without the draw an automatic strike makes.
    [Fact]
    public void ChooseCommand_SubjectOfFoeHpBelow_AimsAtTheHurtFoe()
    {
        var hurt = Member("jazog", hp: 40);
        var actor = Actor(Rule(_strike, TacticTarget.Subject, _foeBelowHalf));

        var command = ChooseFor(actor, [Member("sarxian", hp: 60), hurt], draws: []);

        Assert.Equal(hurt, TargetOf(command));
    }

    // Only the member at 40 % met the condition; the roll of 500 fails the enemy's 50 % shrug-off (decision 838).
    [Fact]
    public void ChooseCommand_SubjectOfFoeHpBelowWhileAnotherMemberTaunts_AimsAtTheTaunter()
    {
        var taunter = Taunting(Member("jazog", hp: 90));
        var actor = Actor(Rule(_strike, TacticTarget.Subject, _foeBelowHalf), crowdControlResistance: 50);

        var command = ChooseFor(actor, [Member("sarxian", hp: 40), taunter], draws: [500]);

        Assert.Equal(taunter, TargetOf(command));
    }

    // Without a condition "that one" names nobody: the attack holds the whole weight; the one value is its target.
    [Fact]
    public void ChooseCommand_SubjectWithoutCondition_AttacksInstead()
    {
        var jazog = Member("jazog");
        var actor = Actor(Rule(_strike, TacticTarget.Subject));

        var command = ChooseFor(actor, [Member("sarxian"), jazog], draws: [1]);

        Assert.Equal(new AttackCommand(jazog), command);
    }

    // The condition holds and the heal has a living ally to aim at, but the fallen it names aren't legal for it.
    [Fact]
    public void ChooseCommand_TargetFindsNoLegalOne_AttacksInstead()
    {
        var sarxian = Member("sarxian");
        var actor = Actor(Rule(_mend, TacticTarget.Subject, _allyFallen));

        var command = ChooseFor(actor, [sarxian, Ally("Standing", hp: 50), Ally("Fallen", hp: 0)], draws: [0]);

        Assert.Equal(new AttackCommand(sarxian), command);
    }

    // A target keeps only whom it names among the skill's legal targets, here none, so the attack draws its target.
    [Theory]
    [InlineData(TacticTarget.Self, "strike")]
    [InlineData(TacticTarget.LowestHpFoe, "mend")]
    [InlineData(TacticTarget.LowestHpOnItsSide, "strike")]
    [InlineData(TacticTarget.LowestHpOnItsSide, "revive")]
    public void ChooseCommand_TargetNamingNoneOfTheSkillsLegalOnes_AttacksInstead(TacticTarget target, string skillId)
    {
        var sarxian = Member("sarxian");
        var actor = Actor(Rule(_skills[skillId], target));

        var command = ChooseFor(actor, [sarxian, Ally("Standing", hp: 50), Ally("Fallen", hp: 0)], draws: [0]);

        Assert.Equal(new AttackCommand(sarxian), command);
    }

    [Fact]
    public void ChooseCommand_AttackOnLowestHpFoe_AttacksThatFoe()
    {
        var lowestShare = Member("jazog", hp: 30);
        var actor = Actor(new EnemyAction { Weight = 1, On = TacticTarget.LowestHpFoe });

        var command = ChooseFor(actor, [Member("sarxian", hp: 60), lowestShare], draws: []);

        Assert.Equal(new AttackCommand(lowestShare), command);
    }

    private static BattleCommand ChooseFor(Combatant actor, Combatant[] others, int[] draws)
    {
        var battle = new BattleState([.. others, actor], new Inventory(), new FixedSequenceRandom(ActorFirst(others.Length + 1)));
        return new EnemyTactics(new FixedSequenceRandom(draws), _skills).ChooseCommand(battle);
    }

    // The shuffle swaps the last combatant with the first, then leaves every other in place, so the actor acts first.
    private static int[] ActorFirst(int count) => [0, .. Enumerable.Range(1, count - 2).Reverse()];

    private static Combatant TargetOf(BattleCommand command) => Assert.Single(Assert.IsType<SkillCommand>(command).Targets);

    private static EnemyAction Rule(SkillDefinition skill, TacticTarget target, TacticCondition? when = null) =>
        new() { SkillId = skill.Id, Weight = 1, On = target, When = when };

    private static Combatant Actor(EnemyAction rule, int hp = 100, int crowdControlResistance = 0)
    {
        var actor = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Actor").WithStats(maxHp: 100, damage: 5)
            .WithCrowdControlResistance(crowdControlResistance).WithTactics(rule).Build());
        actor.Vitals.TakeDamage(100 - hp);
        return actor;
    }

    private static Combatant Ally(string name, int hp)
    {
        var ally = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp: 100, damage: 5).Build());
        ally.Vitals.TakeDamage(100 - hp);
        return ally;
    }

    private static Combatant Member(string id, int hp = 100, int maxHp = 100) =>
        Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).WithVitals(hp, maxHp).Build());

    private static Combatant Taunting(Combatant member)
    {
        member.Statuses.Apply(TestStatuses.Taunt(2));
        return member;
    }

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
