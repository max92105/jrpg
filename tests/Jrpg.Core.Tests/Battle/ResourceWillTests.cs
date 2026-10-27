using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A warlock with 30 Will, three points at 10 per point, whose Instability (max 10, from 0, draining 1 a turn) loses
// them on every gain (decision 319). Bolt builds 5, Spark 2. The shuffle (1) keeps the warlock first.
public sealed class ResourceWillTests
{
    private static readonly ResourceDefinition _instability = new("instability", "Instability", Max: 10, StartsAt: 0)
    {
        Gains = new ResourceGains { PerTurn = -1 },
        WillScaling = new WillScaling(WillPerPoint: 10, WillEffect.LowerGain),
    };

    private static readonly SkillDefinition _bolt = Spell("bolt", builds: 5);
    private static readonly SkillDefinition _spark = Spell("spark", builds: 2);

    [Fact]
    public void Execute_LowerGainThreePoints_AddsTheRest()
    {
        var (warlock, goblin, battle) = Battle();

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Contains(new ResourceChanged(warlock, "instability", 2), events);
    }

    [Fact]
    public void Execute_LowerGainBeyondTheGain_AddsOne()
    {
        var (warlock, goblin, battle) = Battle();

        var events = battle.Execute(new SkillCommand(_spark, [goblin]));

        Assert.Contains(new ResourceChanged(warlock, "instability", 1), events);
    }

    // The turn's drain comes after the skill in the same Execute, and loses nothing to Will.
    [Fact]
    public void Execute_LowerGainOnADrain_DrainsInFull()
    {
        var (warlock, goblin, battle) = Battle();

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Contains(new ResourceChanged(warlock, "instability", -1), events);
        Assert.Equal(1, warlock.Resources.AmountOf("instability"));
    }

    private static (Combatant Warlock, Combatant Goblin, BattleState Battle) Battle()
    {
        var warlock = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithPrimaries(PrimaryStats.None with { Will = 30 }, PrimaryStats.None).WithResources(_instability).WithSkills("bolt", "spark").Build(),
            new Dictionary<string, SkillDefinition> { ["bolt"] = _bolt, ["spark"] = _spark },
            TestStatRules.Neutral));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 1).Build());
        return (warlock, goblin, new BattleState([warlock, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    private static SkillDefinition Spell(string id, int builds) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy, DamageType: DamageType.Dark),
        ResourceChanges = [new ResourceChange("instability", builds)],
    };
}
