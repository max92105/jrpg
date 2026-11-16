using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A member against one goblin; the shuffle (1) keeps the member first. Rot poisons for 10 over 3 turns; Bash deals 13
// and stuns with a 60 % chance; Brace stuns its user for certain.
public sealed class BattleStatusScalingTests
{
    private static readonly SkillDefinition _rot =
        Skill("rot", TestStatuses.Granting(
            new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy), TestStatuses.Poison(10, 3)));

    private static readonly SkillDefinition _bash =
        Skill("bash", TestStatuses.Granting(new Effect(EffectKind.Damage, 13, Targeting.SingleEnemy), TestStatuses.Stun(60, 1)));

    private static readonly SkillDefinition _brace =
        Skill("brace", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.Self), TestStatuses.Stun(100, 1)));

    // 20 Intelligence at 100 % per 100 points: the rot's 10 becomes 12.
    [Fact]
    public void Execute_PoisonFromCleverMember_TicksHarder()
    {
        var druid = Member(PrimaryStats.None with { Intelligence = 20 }, TestStatRules.Neutral with { StatusPercentPer100Intelligence = 100 }, _rot);
        var goblin = Goblin(crowdControlResistance: 0);
        var battle = new BattleState([druid, goblin], new Inventory(), new FixedSequenceRandom(1));

        battle.Execute(new SkillCommand(_rot, [goblin]));

        Assert.Equal(12, TestStatuses.AmountOf(goblin.Statuses.Active.Single()));
    }

    // 25 Intelligence at one extra turn per 25: the rot lasts 4 turns.
    [Fact]
    public void Execute_PoisonFromCleverMember_LastsLonger()
    {
        var druid = Member(PrimaryStats.None with { Intelligence = 25 }, TestStatRules.Neutral with { IntelligencePerExtraStatusTurn = 25 }, _rot);
        var goblin = Goblin(crowdControlResistance: 0);
        var battle = new BattleState([druid, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new SkillCommand(_rot, [goblin]));

        Assert.Contains(
            events.OfType<StatusApplied>(),
            applied => (applied.Target, TestStatuses.IdOf(applied.Status), applied.Turns) == (goblin, TestStatuses.PoisonId, 4));
    }

    // The same Intelligence adds nothing to a stun's turn (decision 317); at 200 % the chance passes 100, so it draws
    // nothing.
    [Fact]
    public void Execute_StunFromCleverMember_KeepsItsOneTurn()
    {
        var fighter = Member(PrimaryStats.None with { Intelligence = 25 }, TestStatRules.Neutral with { IntelligencePerExtraStatusTurn = 25, StatusPercentPer100Intelligence = 400 }, _bash);
        var goblin = Goblin(crowdControlResistance: 0);
        var battle = new BattleState([fighter, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Contains(
            events.OfType<StatusApplied>(),
            applied => (applied.Target, TestStatuses.IdOf(applied.Status), applied.Turns) == (goblin, TestStatuses.StunId, 1));
    }

    // 60 % less the goblin's 30 % is a 30 % chance: a draw of 999 loses it.
    [Fact]
    public void Execute_StunRollFailsAgainstEnemyResistance_ReportsResisted()
    {
        var fighter = Member(PrimaryStats.None, TestStatRules.Neutral, _bash);
        var goblin = Goblin(crowdControlResistance: 30);
        var battle = new BattleState([fighter, goblin], new Inventory(), new FixedSequenceRandom(1, 999));

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Contains(
            events.OfType<StatusResisted>(),
            resisted => (resisted.Target, TestStatuses.IdOf(resisted.Status)) == (goblin, TestStatuses.StunId));
        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusApplied);
        Assert.False(goblin.Statuses.Has(TestStatuses.StunId));
    }

    [Fact]
    public void Execute_StunRollSucceedsAgainstEnemyResistance_Stuns()
    {
        var fighter = Member(PrimaryStats.None, TestStatRules.Neutral, _bash);
        var goblin = Goblin(crowdControlResistance: 30);
        var battle = new BattleState([fighter, goblin], new Inventory(), new FixedSequenceRandom(1, 299));

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Contains(
            events.OfType<StatusApplied>(),
            applied => (applied.Target, TestStatuses.IdOf(applied.Status), applied.Turns) == (goblin, TestStatuses.StunId, 1));
    }

    // A member's own crowd control resistance counts too: at 100 % the brace's sure stun has no chance, and no draw.
    [Fact]
    public void Execute_SelfStunAgainstOwnFullResistance_IsResisted()
    {
        var monk = Member(PrimaryStats.None, TestStatRules.Neutral with { BaseCrowdControlResistance = 100 }, _brace);
        var goblin = Goblin(crowdControlResistance: 0);
        var battle = new BattleState([monk, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new SkillCommand(_brace, [monk]));

        Assert.Contains(
            events.OfType<StatusResisted>(),
            resisted => (resisted.Target, TestStatuses.IdOf(resisted.Status)) == (monk, TestStatuses.StunId));
    }

    private static Combatant Member(PrimaryStats primaries, StatRules rules, SkillDefinition skill) =>
        Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithPrimaries(primaries, PrimaryStats.None).WithSkills([skill.Id]).Build(),
            new Dictionary<string, SkillDefinition> { [skill.Id] = skill },
            rules));

    private static Combatant Goblin(int crowdControlResistance) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithCrowdControlResistance(crowdControlResistance).Build());

    private static SkillDefinition Skill(string id, Effect effect) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = effect,
    };
}
