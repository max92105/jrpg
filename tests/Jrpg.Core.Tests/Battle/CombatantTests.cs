using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
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
        var member = new PartyMemberBuilder().WithClass("fighter").WithDefense(5).Build();
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(15).Build();
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
        var goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20).Build();

        var combatant = Combatant.FromEnemy(goblin);

        Assert.Equal(40, combatant.Vitals.Hp);
        Assert.Equal(40, combatant.Vitals.MaxHp);
    }

    [Fact]
    public void CurrentStats_NoStatus_EqualsStats()
    {
        var combatant = Combatant.FromPartyMember(new PartyMemberBuilder().WithAttack(10).Build());

        var current = combatant.CurrentStats;

        Assert.Equal(combatant.Stats, current);
    }

    [Fact]
    public void FromPartyMember_SpeedScaledResource_MaxAndStartFromSpeed()
    {
        var combatant = Combatant.FromPartyMember(Rogue(level: 1));

        Assert.Equal((3, 3), (combatant.Resources.MaxOf("readiness"), combatant.Resources.AmountOf("readiness")));
    }

    // Speed 30 at level 1 and +5 a level: 40 at level 3, so 4 charges.
    [Fact]
    public void FromPartyMember_LevelUpRaisesSpeed_MoreCharges()
    {
        var combatant = Combatant.FromPartyMember(Rogue(level: 3));

        Assert.Equal(4, combatant.Resources.MaxOf("readiness"));
    }

    private static PartyMember Rogue(int level)
    {
        var readiness = new ResourceDefinition("readiness", "Readiness") { MaxFromSpeed = new SpeedScaling(SpeedPerCharge: 10) };
        var rogue = new ClassBuilder()
            .WithId("rogue")
            .WithBase(new ClassStats(MaxHp: 100, Defense: 5, Speed: 30))
            .WithGrowth(new ClassStats(MaxHp: 0, Defense: 0, Speed: 5))
            .WithResources(readiness)
            .Build();
        var progress = new PartyMemberProgress("lazarus", level, 0, Hp: 100, WeaponId: null, ArmorId: null, LastingAmounts: new Dictionary<string, int>());
        var member = PartyMember.Recruit(new PartyMemberBuilder().WithId("lazarus").BuildDefinition(), rogue, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral);
        member.RestoreProgress(progress);
        return member;
    }

    [Fact]
    public void FromEnemy_Always_HasNoResources()
    {
        var goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20).Build();

        var combatant = Combatant.FromEnemy(goblin);

        Assert.Empty(combatant.Resources.Definitions);
    }

    [Fact]
    public void FromEnemy_Definition_UsesItsStatsOnEnemySide()
    {
        var goblin = new EnemyDefinitionBuilder().WithName("Goblin").WithStats(maxHp: 40, attack: 20).WithDefense(2).Build();

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

    [Fact]
    public void FromPartyMember_LastingResource_StartsAtMembersAmount()
    {
        var clarity = new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 3) { IsLasting = true };
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().WithId("omer").BuildDefinition(),
            new ClassBuilder().WithId("mage").WithResources(clarity).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral);
        member.KeepLastingAmount("clarity", 1);

        var combatant = Combatant.FromPartyMember(member);

        Assert.Equal(1, combatant.Resources.AmountOf("clarity"));
    }

    [Fact]
    public void FromPartyMember_TwoClarityMissing_ManaStartsTwoLower()
    {
        var mage = StrainedMage(clarityLeft: 1);

        var combatant = Combatant.FromPartyMember(mage);

        Assert.Equal(1, combatant.Resources.AmountOf("mana"));
    }

    [Fact]
    public void FromPartyMember_StrainBeyondStart_StartsAtZero()
    {
        var mage = StrainedMage(clarityLeft: 0, perMissingPoint: 2);

        var combatant = Combatant.FromPartyMember(mage);

        Assert.Equal(0, combatant.Resources.AmountOf("mana"));
    }

    // Mana starts at 3; Clarity (max 3) lowers it by perMissingPoint for each point below its max.
    private static PartyMember StrainedMage(int clarityLeft, int perMissingPoint = 1)
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3);
        var clarity = new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 3)
        {
            IsLasting = true,
            Strain = new StrainRule("mana", perMissingPoint),
        };
        var mage = PartyMember.Recruit(
            new PartyMemberBuilder().WithId("omer").BuildDefinition(),
            new ClassBuilder().WithId("mage").WithResources(mana, clarity).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral);
        mage.KeepLastingAmount("clarity", clarityLeft);
        return mage;
    }
}
