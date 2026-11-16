using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A foe without the skill's statuses (decision 834): a random legal foe holding no family of the skill's harmful
// grants, one draw among them after the taunt's roll (decision 838). The rule is the enemy's only action, so choosing
// it draws nothing, and each sequence holds exactly the draws expected: an extra draw runs it out. The member the rule
// passes over is listed first, where a draw of 0 would take it.
public sealed class EnemyUnafflictedFoeTests
{
    private static readonly SkillDefinition _poisonDart =
        Skill("poison_dart", TestStatuses.Granting(Dart, TestStatuses.PoisonTier(1, turns: 3)));

    private static readonly SkillDefinition _wardingDart = Skill(
        "warding_dart", Dart with { Statuses = [TestStatuses.PoisonTier(1, turns: 3), TestStatuses.Guard(10, turns: 3)] });

    private static readonly Dictionary<string, SkillDefinition> _skills =
        new[] { _poisonDart, _wardingDart }.ToDictionary(skill => skill.Id);

    private static Effect Dart => new(EffectKind.Damage, 3, Targeting.SingleEnemy);

    [Fact]
    public void ChooseCommand_UnafflictedFoe_SkipsTheMemberAlreadyPoisoned()
    {
        var unpoisoned = Member("jazog");
        var actor = Actor(_poisonDart);

        var command = ChooseFor(actor, [Poisoned("sarxian"), unpoisoned], draws: [0]);

        Assert.Equal(unpoisoned, TargetOf(command));
    }

    // Nobody is left to spread it to, so the rule isn't eligible and the attack draws its target.
    [Fact]
    public void ChooseCommand_UnafflictedFoeAllAfflicted_AttacksInstead()
    {
        var jazog = Poisoned("jazog");
        var actor = Actor(_poisonDart);

        var command = ChooseFor(actor, [Poisoned("sarxian"), jazog], draws: [1]);

        Assert.Equal(new AttackCommand(jazog), command);
    }

    // The skill grants poison_1; a holder keeps one status per family (decision 800), so poison_2 counts.
    [Fact]
    public void ChooseCommand_UnafflictedFoeOtherTierOfTheFamily_CountsAsAfflicted()
    {
        var unpoisoned = Member("jazog");
        var actor = Actor(_poisonDart);

        var command = ChooseFor(actor, [Poisoned("sarxian", tier: 2), unpoisoned], draws: [0]);

        Assert.Equal(unpoisoned, TargetOf(command));
    }

    // The guard the skill also grants is a boon, so holding it leaves the member without the skill's statuses.
    [Fact]
    public void ChooseCommand_UnafflictedFoeHoldingTheSkillsBoon_CountsAsUnafflicted()
    {
        var guarded = Holding(Member("sarxian"), TestStatuses.Guard(10, turns: 2));
        var actor = Actor(_wardingDart);

        var command = ChooseFor(actor, [Poisoned("jazog"), guarded], draws: [0]);

        Assert.Equal(guarded, TargetOf(command));
    }

    // The roll of 500 fails the enemy's 50 % shrug-off, so the poisoned taunter is the only one left to draw.
    [Fact]
    public void ChooseCommand_UnafflictedFoeWhileAnAfflictedMemberTaunts_AimsAtTheTaunter()
    {
        var taunter = Holding(Poisoned("jazog"), TestStatuses.Taunt(2));
        var actor = Actor(_poisonDart, crowdControlResistance: 50);

        var command = ChooseFor(actor, [Member("sarxian"), taunter], draws: [500, 0]);

        Assert.Equal(taunter, TargetOf(command));
    }

    private static BattleCommand ChooseFor(Combatant actor, Combatant[] others, int[] draws)
    {
        var battle = new BattleState([.. others, actor], new Inventory(), new FixedSequenceRandom(ActorFirst(others.Length + 1)));
        return new EnemyTactics(new FixedSequenceRandom(draws), _skills).ChooseCommand(battle);
    }

    // The shuffle swaps the last combatant with the first, then leaves every other in place, so the actor acts first.
    private static int[] ActorFirst(int count) => [0, .. Enumerable.Range(1, count - 2).Reverse()];

    private static Combatant TargetOf(BattleCommand command) => Assert.Single(Assert.IsType<SkillCommand>(command).Targets);

    private static Combatant Actor(SkillDefinition skill, int crowdControlResistance = 0) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Actor").WithStats(maxHp: 100, damage: 5)
            .WithCrowdControlResistance(crowdControlResistance)
            .WithTactics(new EnemyAction { SkillId = skill.Id, Weight = 1, On = TacticTarget.UnafflictedFoe }).Build());

    private static Combatant Member(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());

    private static Combatant Poisoned(string id, int tier = 1) => Holding(Member(id), TestStatuses.PoisonTier(tier, turns: 2));

    private static Combatant Holding(Combatant member, StatusGrant grant)
    {
        member.Statuses.Apply(grant);
        return member;
    }

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
