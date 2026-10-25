using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

public sealed class CombatantTests
{
    [Fact]
    public void FromPartyMember_DamageInBattle_PersistsOnMember()
    {
        var member = new PartyMemberBuilder().WithVitals(hp: 100, maxHp: 100).Build();
        var combatant = Combatant.FromPartyMember(member);

        combatant.Vitals.TakeDamage(30);

        Assert.Equal(70, member.Vitals.Hp);
    }

    [Fact]
    public void FromPartyMember_EquippedMember_UsesTotalStatsOnPartySide()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").WithStats(attack: 10, defense: 5).Build();
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
        var inventory = new Inventory();
        inventory.Add(sword, 1);
        new PartyEquipment(inventory).TryEquip(member, sword);

        var combatant = Combatant.FromPartyMember(member);

        Assert.Equal(BattleSide.Party, combatant.Side);
        Assert.Equal(new CombatStats(Attack: 15, Defense: 5), combatant.Stats);
    }

    [Fact]
    public void FromPartyMember_MemberWithSkills_CarriesThem()
    {
        var heal = new SkillDefinition
        {
            Id = "heal",
            Name = "Heal",
            Description = "",
            IconFile = "Skill.png",
            Effect = new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly),
        };
        var member = new PartyMemberBuilder().WithSkills(heal).Build();

        var combatant = Combatant.FromPartyMember(member);

        Assert.Equal([heal], combatant.Skills);
    }

    [Fact]
    public void FromEnemy_StartsAtFullHp()
    {
        var goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build();

        var combatant = Combatant.FromEnemy(goblin);

        Assert.Equal(40, combatant.Vitals.Hp);
        Assert.Equal(40, combatant.Vitals.MaxHp);
    }

    [Fact]
    public void FromEnemy_Always_HasNoResources()
    {
        var goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build();

        var combatant = Combatant.FromEnemy(goblin);

        Assert.Empty(combatant.Resources.Definitions);
    }

    [Fact]
    public void FromEnemy_Definition_UsesItsStatsOnEnemySide()
    {
        var goblin = new EnemyDefinitionBuilder().WithName("Goblin").WithStats(maxHp: 40, attack: 20, defense: 2).Build();

        var combatant = Combatant.FromEnemy(goblin);

        Assert.Equal("Goblin", combatant.Name);
        Assert.Equal(BattleSide.Enemies, combatant.Side);
        Assert.Equal(new CombatStats(Attack: 20, Defense: 2), combatant.Stats);
    }

    [Fact]
    public void FromEnemy_SameDefinitionTwice_GivesSeparateVitals()
    {
        var goblin = new EnemyDefinitionBuilder().Build();
        var first = Combatant.FromEnemy(goblin);
        var second = Combatant.FromEnemy(goblin);

        first.Vitals.TakeDamage(10);

        Assert.Equal(second.Vitals.MaxHp, second.Vitals.Hp);
    }
}
