using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

public sealed class AttackEffectTests
{
    [Fact]
    public void Of_MemberWithSword_IsSlashing()
    {
        var member = new PartyMemberBuilder().Build();
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).WithDamageType(DamageType.Slashing).Build();
        var inventory = new Inventory();
        inventory.Add(sword, 1);
        new PartyEquipment(inventory).TryEquip(member, sword);

        var effect = AttackEffect.Of(Combatant.FromPartyMember(member));

        Assert.Equal(DamageType.Slashing, effect.DamageType);
    }

    [Fact]
    public void Of_UnarmedMember_IsRulesType()
    {
        var rules = TestStatRules.Neutral with { UnarmedDamageType = DamageType.Dark };
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), new ClassBuilder().Build(), new Dictionary<string, SkillDefinition>(), rules);

        var effect = AttackEffect.Of(Combatant.FromPartyMember(member));

        Assert.Equal(DamageType.Dark, effect.DamageType);
    }

    [Fact]
    public void Of_Enemy_IsItsDamageType()
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithDamageType(DamageType.Fire).Build());

        var effect = AttackEffect.Of(goblin);

        Assert.Equal(DamageType.Fire, effect.DamageType);
    }
}
