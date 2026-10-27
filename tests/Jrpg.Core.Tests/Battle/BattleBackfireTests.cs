using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A warlock (100 max HP) whose Instability backfires 5 % per point for 10 % of max HP, against one goblin. The first
// scripted value is the shuffle's (1 keeps the warlock first); the next is the backfire roll, out of 100.
public sealed class BattleBackfireTests
{
    private static readonly ResourceDefinition _instability = new("instability", "Instability", Max: 10, StartsAt: 2)
    {
        Backfire = new BackfireRule(ChancePerPointPercent: 5, DamagePercentOfMaxHp: 10),
    };

    private static readonly ResourceDefinition _mana = new("mana", "Mana", Max: 10, StartsAt: 5);

    private static readonly SkillDefinition _bolt = Spell("bolt", new ResourceChange("instability", 2));
    private static readonly SkillDefinition _calm = Spell("calm", new ResourceChange("instability", -2));
    private static readonly SkillDefinition _spark = Spell("spark", new ResourceChange("mana", -1));

    [Fact]
    public void Execute_RollBelowChance_CasterLosesHpAndReportsIt()
    {
        var (warlock, goblin, battle) = Battle(rolls: [1, 19]);

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Contains(new Backfired(warlock, "instability", 10), events);
        Assert.Equal(90, warlock.Vitals.Hp);
    }

    [Fact]
    public void Execute_RollAtChance_NoBackfire()
    {
        var (warlock, goblin, battle) = Battle(rolls: [1, 20]);

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is Backfired);
        Assert.Equal(100, warlock.Vitals.Hp);
    }

    // The script holds only the shuffle's value, so a roll would throw.
    [Fact]
    public void Execute_ZeroInstabilityAfterCast_DrawsNoRandom()
    {
        var (_, goblin, battle) = Battle(rolls: [1]);

        var events = battle.Execute(new SkillCommand(_calm, [goblin]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is Backfired);
    }

    [Fact]
    public void Execute_BackfireFellsCaster_ReportsDefeat()
    {
        var (warlock, goblin, battle) = Battle(rolls: [1, 0]);
        warlock.Vitals.TakeDamage(95);

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Contains(new Backfired(warlock, "instability", 5), events);
        Assert.Contains(new CombatantDefeated(warlock), events);
        Assert.Equal(BattlePhase.Defeat, battle.Phase);
    }

    // The bolt fells the last goblin, so it draws no roll (one would throw): a backfire can't turn a win into a loss.
    [Fact]
    public void Execute_SpellFellsLastEnemy_DrawsNoRandom()
    {
        var (_, goblin, battle) = Battle(rolls: [1]);
        goblin.Vitals.TakeDamage(95);

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is Backfired);
        Assert.Equal(BattlePhase.Victory, battle.Phase);
    }

    [Fact]
    public void Execute_SkillWithoutBackfireRule_DrawsNoRandom()
    {
        var (_, goblin, battle) = Battle(rolls: [1]);

        var events = battle.Execute(new SkillCommand(_spark, [goblin]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is Backfired);
    }

    private static (Combatant Warlock, Combatant Goblin, BattleState Battle) Battle(int[] rolls)
    {
        var warlock = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("sarxian").BuildDefinition(),
            new ClassBuilder().WithId("warlock").WithResources(_instability, _mana).WithSkills("bolt", "calm", "spark").Build(),
            new Dictionary<string, SkillDefinition> { ["bolt"] = _bolt, ["calm"] = _calm, ["spark"] = _spark },
            TestStatRules.Neutral));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, attack: 20).Build());
        return (warlock, goblin, new BattleState([warlock, goblin], new Inventory(), new FixedSequenceRandom(rolls)));
    }

    private static SkillDefinition Spell(string id, ResourceChange change) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        ResourceChanges = [change],
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
