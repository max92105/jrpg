using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The shuffle (0) puts the goblin first; its 20-damage hit always lands on the hero, who wears a shield with a block
// affix. Where the goblin has a 50 % crit chance its crit draws before the block (decision 348): 999 doesn't crit.
public sealed class BattleBlockTests
{
    private static readonly SkillDefinition _mend = new()
    {
        Id = "mend",
        Name = "Mend",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.RestoreHp, 20, Targeting.Self),
    };

    [Fact]
    public void Execute_CertainBlock_HalvesHitAndReportsBlocked()
    {
        var (hero, battle) = Battle(blockPercent: 100, goblinCritChance: 0, 0);

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Equal(new HitBlocked(hero), events.OfType<HitBlocked>().Single());
        Assert.Equal(10, events.OfType<EffectApplied>().Single().Amount);
        Assert.IsType<HitBlocked>(events[events.ToList().FindIndex(battleEvent => battleEvent is EffectApplied) - 1]);
    }

    // The scripted random holds only the shuffle: a block roll would throw.
    [Fact]
    public void Execute_NoBlock_DrawsNothing()
    {
        var (hero, battle) = Battle(blockPercent: null, goblinCritChance: 0, 0);

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Empty(events.OfType<HitBlocked>());
        Assert.Equal(20, events.OfType<EffectApplied>().Single().Amount);
    }

    // 5 % is 50 permille: a block draw of 49 blocks.
    [Fact]
    public void Execute_DrawBelowBlockChance_Blocked()
    {
        var (hero, battle) = Battle(blockPercent: 5, goblinCritChance: 50, 0, 999, 49);

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Single(events.OfType<HitBlocked>());
        Assert.Equal(new EffectApplied(hero, EffectKind.Damage, 10), events.OfType<EffectApplied>().Single());
    }

    [Fact]
    public void Execute_DrawAtBlockChance_NotBlocked()
    {
        var (hero, battle) = Battle(blockPercent: 5, goblinCritChance: 50, 0, 999, 50);

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Empty(events.OfType<HitBlocked>());
        Assert.Equal(20, events.OfType<EffectApplied>().Single().Amount);
    }

    // Only a damaging hit is blocked: a certain blocker's own heal lands whole, with no cue. The shuffle (1) puts the
    // hero first.
    [Fact]
    public void Execute_HealOnCertainBlocker_NotBlocked()
    {
        var (hero, battle) = Battle(blockPercent: 100, goblinCritChance: 0, 1);
        hero.Vitals.TakeDamage(50);

        var events = battle.Execute(new SkillCommand(_mend, [hero]));

        Assert.Empty(events.OfType<HitBlocked>());
        Assert.Equal(new EffectApplied(hero, EffectKind.RestoreHp, 20), events.OfType<EffectApplied>().Single());
    }

    private static (Combatant Hero, BattleState Battle) Battle(int? blockPercent, int goblinCritChance, params int[] draws)
    {
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithSkills([_mend.Id]).Build(),
            new Dictionary<string, SkillDefinition> { [_mend.Id] = _mend },
            TestStatRules.Neutral);
        var shield = new ItemDefinitionBuilder().WithId("shield").AsWorn(EquipmentKind.OffHand).Build();
        var piece = blockPercent is { } percent
            ? GearPiece.Rolled(shield, Rarity.Magic, [new RolledAffix("block", StatKind.BlockChance, percent)])
            : GearPiece.Plain(shield);
        var inventory = new Inventory();
        inventory.AddGear(piece);
        Assert.True(new PartyEquipment(inventory).TryEquip(member, piece));
        var hero = Combatant.FromPartyMember(member);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).WithCritChance(goblinCritChance).Build());
        return (hero, new BattleState([hero, goblin], inventory, new FixedSequenceRandom(draws)));
    }
}
