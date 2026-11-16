using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// An effect grants every status of its list, in order, on each target still standing after it landed (decision 809);
// skills and statuses built here.
public sealed class BattleStatusListTests
{
    private const int MissingDraw = 999;

    private static readonly SkillDefinition _hex = Skill(
        "hex",
        new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
        {
            Statuses = [TestStatuses.Exposed(20, 2), TestStatuses.Poison(8, 3)],
        });

    private static readonly SkillDefinition _venomStrike = Skill(
        "venom_strike",
        new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy)
        {
            Statuses = [TestStatuses.Exposed(20, 2), TestStatuses.Poison(8, 3)],
        });

    [Fact]
    public void Execute_EffectWithTwoStatuses_GrantsBothInListOrder()
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).Build());
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_hex).Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new SkillCommand(_hex, [goblin]));

        Assert.Equal(
            [TestStatuses.ExposedId, TestStatuses.PoisonId],
            events.OfType<StatusApplied>().Select(applied => TestStatuses.IdOf(applied.Status)));
    }

    // A goblin with 50 % dodge and a draw of 999: the strike misses, so neither status lands (decision 326).
    [Fact]
    public void Execute_DamageThatMissedEveryHit_GrantsNoneOfItsStatuses()
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithDodge(50).Build());
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_venomStrike).Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, MissingDraw));

        var events = battle.Execute(new SkillCommand(_venomStrike, [goblin]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusApplied);
    }

    // Decay dominant: +50 % on both grants, Exposed 20 to 30 and Poison 8 to 12.
    [Fact]
    public void Execute_StatusOnlySkillWithDominantForce_RaisesEveryGrant()
    {
        var hex = _hex with { ResourceChanges = [new ResourceChange("decay", 1)] };
        var forces = new[]
        {
            new ResourceDefinition("growth", "Growth", Max: 10) { Dominance = new DominanceRule("decay", 50) },
            new ResourceDefinition("decay", "Decay", Max: 10) { Dominance = new DominanceRule("growth", 50) },
        };
        var druid = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithResources(forces).WithSkills([hex.Id]).Build(),
            new Dictionary<string, SkillDefinition> { [hex.Id] = hex },
            TestStatRules.Neutral));
        druid.ChangeResource("decay", 2, []);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).Build());
        var battle = new BattleState([druid, goblin], new Inventory(), new FixedSequenceRandom(1));

        battle.Execute(new SkillCommand(hex, [goblin]));

        Assert.Equal([30, 12], goblin.Statuses.Active.Select(TestStatuses.AmountOf));
    }

    // Decision 801: every grant rolls its chance; at 60 % a draw of 700 misses and the status is resisted.
    [Fact]
    public void Execute_PoisonAtSixtyPercent_RollsAndCanMiss()
    {
        var (goblin, events) = CastSickness(chanceDraw: 700);

        Assert.Contains(events.OfType<StatusResisted>(), resisted => resisted.Target == goblin);
        Assert.Empty(goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_PoisonAtSixtyPercent_LandsOnALowDraw()
    {
        var (goblin, _) = CastSickness(chanceDraw: 100);

        Assert.Equal([TestStatuses.PoisonId], goblin.Statuses.Active.Select(TestStatuses.IdOf));
    }

    // Crowd control resistance cuts only a stun (decision 324): an enemy with 50 % taunts itself for sure.
    [Fact]
    public void Execute_SelfTauntWithCrowdControlResistance_AlwaysLands()
    {
        var taunt = Skill(
            "bellow", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.Self), TestStatuses.Taunt(2)));
        var chief = Combatant.FromEnemy(new EnemyDefinitionBuilder()
            .WithCrowdControlResistance(50)
            .WithTactics(new EnemyAction { SkillId = taunt.Id, Weight = 1 })
            .Build());
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var battle = new BattleState([hero, chief], new Inventory(), new FixedSequenceRandom(0));

        battle.Execute(new SkillCommand(taunt, [chief]));

        Assert.True(chief.Statuses.Has(TestStatuses.TauntId));
    }

    // Decision 800's named change: Burning and Poison are two families, so on one goblin both tick at the end of its
    // turn (one Poison refreshed the other before records).
    [Fact]
    public void Execute_BurningThenPoison_BothTickAtTheEnd()
    {
        var burning = new StatusDefinition
        {
            Id = "burning_1",
            Name = "Burning",
            Description = "",
            Family = "burning",
            Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 5)],
        };
        var blaze = Skill("blaze", new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
        {
            Statuses = [new StatusGrant(burning, Turns: 2), TestStatuses.Poison(8, 3)],
        });
        var (goblin, battle) = HeroFirstAgainstGoblin(blaze);
        battle.Execute(new SkillCommand(blaze, [goblin]));

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(
            [("burning_1", 5), (TestStatuses.PoisonId, 8)],
            events.OfType<StatusTicked>().Select(ticked => (TestStatuses.IdOf(ticked.Status), ticked.Amount)));
    }

    // Decision 800: Exposed 50 then Exposed 20 is one Exposed keeping 50, never 70; the goblin defends in between.
    [Fact]
    public void Execute_ExposedFiftyThenTwenty_KeepsFifty()
    {
        var hex = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy);
        var expose = Skill("expose", TestStatuses.Granting(hex, TestStatuses.Exposed(50, 3)));
        var bolt = Skill("bolt", TestStatuses.Granting(hex, TestStatuses.Exposed(20, 2)));
        var (goblin, battle) = HeroFirstAgainstGoblin(expose, bolt);
        battle.Execute(new SkillCommand(expose, [goblin]));
        battle.Execute(new DefendCommand());

        battle.Execute(new SkillCommand(bolt, [goblin]));

        Assert.Equal(50, TestStatuses.AmountOf(Assert.Single(goblin.Statuses.Active)));
    }

    // The shuffle (1) puts the hero first.
    private static (Combatant Goblin, BattleState Battle) HeroFirstAgainstGoblin(params SkillDefinition[] skills)
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).Build());
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(skills).Build());
        return (goblin, new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    // The shuffle (1) puts the hero first; the next draw is the poison's roll.
    private static (Combatant Goblin, IReadOnlyList<BattleEvent> Events) CastSickness(int chanceDraw)
    {
        var sickness = Skill(
            "sickness",
            TestStatuses.Granting(
                new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy), TestStatuses.Poison(8, 3) with { Chance = 60 }));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).Build());
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(sickness).Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, chanceDraw));
        return (goblin, battle.Execute(new SkillCommand(sickness, [goblin])));
    }

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
