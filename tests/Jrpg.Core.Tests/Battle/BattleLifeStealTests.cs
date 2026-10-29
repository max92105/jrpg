using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The hero wields a 37-damage sword with 10 % lifesteal against a 100 HP goblin: every hit lands for certain and never
// crits under the neutral rules, so the only draw is the shuffle (1), which keeps the hero first. Each landed hit
// steals 37 × 10 % = 3 HP, rounded down (decision 346).
public sealed class BattleLifeStealTests
{
    private const int LifeStealPercent = 10;

    private static readonly SkillDefinition _mend = new()
    {
        Id = "mend",
        Name = "Mend",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.RestoreHp, 20, Targeting.Self),
    };

    [Fact]
    public void Execute_AttackWithLifeSteal_HealsAttacker()
    {
        var (hero, goblin, battle) = Battle(TestStatRules.Neutral, new Inventory());
        hero.Vitals.TakeDamage(20);
        var hpBefore = hero.Vitals.Hp;

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal([new LifeStolen(hero, 3)], events.OfType<LifeStolen>());
        Assert.Equal(hpBefore + 3, hero.Vitals.Hp);
    }

    // Two HP missing take two of the three the hit would steal; what is reported is what was restored.
    [Fact]
    public void Execute_StealsMoreThanMissing_ReportsWhatWasRestored()
    {
        var (hero, goblin, battle) = Battle(TestStatRules.Neutral, new Inventory());
        hero.Vitals.TakeDamage(2);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal([new LifeStolen(hero, 2)], events.OfType<LifeStolen>());
        Assert.Equal(hero.Vitals.MaxHp, hero.Vitals.Hp);
    }

    [Fact]
    public void Execute_AtFullHp_ReportsNothing()
    {
        var (_, goblin, battle) = Battle(TestStatRules.Neutral, new Inventory());

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Empty(events.OfType<LifeStolen>());
    }

    // At 50 % accuracy the hit draws to land: 999 misses.
    [Fact]
    public void Execute_MissedHit_StealsNothing()
    {
        var (hero, goblin, battle) = Battle(TestStatRules.Neutral with { BaseAccuracy = 50 }, new Inventory(), 1, 999);
        hero.Vitals.TakeDamage(20);
        var hpBefore = hero.Vitals.Hp;

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Empty(events.OfType<LifeStolen>());
        Assert.Equal(hpBefore, hero.Vitals.Hp);
    }

    // A goblin left at 10 HP loses only 10 to the 37-damage hit: 10 % of what it really took is 1 (decision 365).
    [Fact]
    public void Execute_Overkill_StealsFromWhatTheHitTook()
    {
        var (hero, goblin, battle) = Battle(TestStatRules.Neutral, new Inventory());
        hero.Vitals.TakeDamage(20);
        goblin.Vitals.TakeDamage(90);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal([new LifeStolen(hero, 1)], events.OfType<LifeStolen>());
    }

    // Only a damaging hit steals.
    [Fact]
    public void Execute_HealSkill_StealsNothing()
    {
        var (hero, _, battle) = Battle(TestStatRules.Neutral, new Inventory());
        hero.Vitals.TakeDamage(50);

        var events = battle.Execute(new SkillCommand(_mend, [hero]));

        Assert.Empty(events.OfType<LifeStolen>());
    }

    // An item's hits carry none of the thrower's lifesteal (decision 321).
    [Fact]
    public void Execute_BombItem_StealsNothing()
    {
        var bomb = new ItemDefinitionBuilder().WithId("bomb").WithEffect(new Effect(EffectKind.Damage, 40, Targeting.SingleEnemy)).Build();
        var inventory = new Inventory();
        inventory.Add(bomb, 1);
        var (hero, goblin, battle) = Battle(TestStatRules.Neutral, inventory);
        hero.Vitals.TakeDamage(20);

        var events = battle.Execute(new ItemCommand(bomb, [goblin]));

        Assert.Equal(40, events.OfType<EffectApplied>().Single().Amount);
        Assert.Empty(events.OfType<LifeStolen>());
    }

    private static (Combatant Hero, Combatant Goblin, BattleState Battle) Battle(StatRules rules, Inventory inventory, params int[] draws)
    {
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithSkills([_mend.Id]).Build(),
            new Dictionary<string, SkillDefinition> { [_mend.Id] = _mend },
            rules);
        var sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(37).Build();
        var piece = GearPiece.Rolled(sword, Rarity.Magic, [new RolledAffix("life_steal", StatKind.LifeSteal, LifeStealPercent)]);
        inventory.AddGear(piece);
        Assert.True(new PartyEquipment(inventory).TryEquip(member, piece));
        var hero = Combatant.FromPartyMember(member);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 20).Build());
        return (hero, goblin, new BattleState([hero, goblin], inventory, new FixedSequenceRandom(draws.Length == 0 ? [1] : draws)));
    }
}
