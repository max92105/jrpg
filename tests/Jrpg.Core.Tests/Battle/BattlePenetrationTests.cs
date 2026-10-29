using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The hero swings a 40-damage slashing sword at a goblin resisting Slashing 25 %: every hit lands and never crits under
// the neutral rules, so the only draw is the shuffle (1), which keeps the hero first. Penetration takes its points off
// the resistance (decision 364).
public sealed class BattlePenetrationTests
{
    private static readonly Resistances _slashingQuarter = Resistances.None with { Slashing = 250 };

    // 40 × 75 % is 30; with 10 points off, 40 × 85 % is 34.
    [Fact]
    public void Execute_PenetrationAgainstResistance_DealsMore()
    {
        var (_, goblin, battle) = Battle(penetrationPercent: 10);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(34, events.OfType<EffectApplied>().Single().Amount);
    }

    [Fact]
    public void Execute_NoPenetration_ResistanceTakesItsShare()
    {
        var (_, goblin, battle) = Battle(penetrationPercent: null);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(new EffectApplied(goblin, EffectKind.Damage, 30, HitAffinity.Resisted), events.OfType<EffectApplied>().Single());
    }

    // 25 % less 5 points is 20 %, under the "Resisted" threshold (decision 323): the cue reads the penetrated value.
    [Fact]
    public void Execute_PenetratedBelowThreshold_NotResisted()
    {
        var (_, goblin, battle) = Battle(penetrationPercent: 5);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(HitAffinity.Neutral, events.OfType<EffectApplied>().Single().Affinity);
    }

    // An item's hits carry none of the thrower's penetration (decision 321): 40 × 75 % is 30, as without it.
    [Fact]
    public void Execute_SlashingItemWithPenetrationWorn_ResistanceTakesItsShare()
    {
        var blade = new ItemDefinitionBuilder()
            .WithId("throwing_blade")
            .WithEffect(new Effect(EffectKind.Damage, 40, Targeting.SingleEnemy) { DamageType = DamageType.Slashing })
            .Build();
        var (_, goblin, battle) = Battle(penetrationPercent: 10, blade);

        var events = battle.Execute(new ItemCommand(blade, [goblin]));

        Assert.Equal(30, events.OfType<EffectApplied>().Single().Amount);
    }

    private static (Combatant Hero, Combatant Goblin, BattleState Battle) Battle(int? penetrationPercent, ItemDefinition? carried = null)
    {
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral);
        var sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(40).WithDamageType(DamageType.Slashing).Build();
        var piece = penetrationPercent is { } percent
            ? GearPiece.Rolled(sword, Rarity.Magic, [new RolledAffix("armor_penetration", StatKind.ArmorPenetration, percent)])
            : GearPiece.Plain(sword);
        var inventory = new Inventory();
        inventory.AddGear(piece);
        if (carried is not null)
        {
            inventory.Add(carried, 1);
        }

        Assert.True(new PartyEquipment(inventory).TryEquip(member, piece));
        var hero = Combatant.FromPartyMember(member);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).WithResistances(_slashingQuarter).Build());
        return (hero, goblin, new BattleState([hero, goblin], inventory, new FixedSequenceRandom(1)));
    }
}
