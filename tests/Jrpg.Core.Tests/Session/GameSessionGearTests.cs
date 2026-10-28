using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// The default fighter has 100 HP; at +1 % HP per Constitution point, chain mail's +10 Constitution makes it 110.
public sealed class GameSessionGearTests
{
    private static readonly ItemDefinition _mail = new ItemDefinitionBuilder().WithId("chain_mail").AsChest()
        .WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Constitution] = 10 }))
        .Build();

    [Fact]
    public void CaptureThenRestore_NineSlots_WearsTheSame()
    {
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
        var ring = new ItemDefinitionBuilder().WithId("power_ring").AsWorn(EquipmentKind.Ring).Build();
        var helm = new ItemDefinitionBuilder().WithId("leather_cap").AsWorn(EquipmentKind.Head).Build();
        var catalog = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(_mail, sword, ring, helm).WithLeader("sarxian").Build();
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());
        foreach (var (item, slot) in new[] { (sword, EquipmentSlot.Weapon), (_mail, EquipmentSlot.Chest), (helm, EquipmentSlot.Head), (ring, EquipmentSlot.FirstRing), (ring, EquipmentSlot.SecondRing) })
        {
            session.Inventory.Add(item, 1);
            Assert.True(session.Equipment.TryEquip(session.Party.Leader, item, slot));
        }

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), catalog, new FixedSequenceRandom());

        var leader = restored.Party.Leader;
        Assert.Equal(
            (sword, _mail, helm, ring, ring),
            (leader.Weapon?.Base, leader.EquippedIn(EquipmentSlot.Chest)?.Base, leader.EquippedIn(EquipmentSlot.Head)?.Base, leader.EquippedIn(EquipmentSlot.FirstRing)?.Base, leader.EquippedIn(EquipmentSlot.SecondRing)?.Base));
        Assert.Equal(0, restored.Inventory.QuantityOf("power_ring"));
    }

    [Fact]
    public void Load_HpAboveClassMaxWithConstitutionArmor_KeepsHp()
    {
        var built = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(_mail).WithLeader("sarxian").Build();
        var catalog = built with { Rules = built.Rules with { Stats = TestStatRules.Neutral with { HpPercentPer100Constitution = 100 } } };
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());
        session.Inventory.Add(_mail, 1);
        Assert.True(session.Equipment.TryEquip(session.Party.Leader, _mail));
        session.Party.Leader.Rest();

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), catalog, new FixedSequenceRandom());

        var leader = restored.Party.Leader;
        Assert.Equal((110, 110), (leader.Vitals.Hp, leader.Vitals.MaxHp));
    }
}
