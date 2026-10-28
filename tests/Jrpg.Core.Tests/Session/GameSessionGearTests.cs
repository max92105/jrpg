using Jrpg.Core.Items;
using Jrpg.Core.Loot;
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
            Assert.True(session.Equipment.TryEquip(session.Party.Leader, GearPiece.Plain(item), slot));
        }

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), catalog, new FixedSequenceRandom());

        var leader = restored.Party.Leader;
        Assert.Equal(
            (sword, _mail, helm, ring, ring),
            (leader.Weapon?.Base, leader.EquippedIn(EquipmentSlot.Chest)?.Base, leader.EquippedIn(EquipmentSlot.Head)?.Base, leader.EquippedIn(EquipmentSlot.FirstRing)?.Base, leader.EquippedIn(EquipmentSlot.SecondRing)?.Base));
        Assert.Equal(0, restored.Inventory.GearCountOf("power_ring"));
    }

    // Owned gear is saved as a count per base until saves keep pieces (S1a); it loads back plain.
    [Fact]
    public void CaptureThenRestore_OwnedGear_ComesBackAsPlainPieces()
    {
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
        var potion = new ItemDefinitionBuilder().WithId("potion").Build();
        var catalog = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(sword, potion).WithLeader("sarxian").Build();
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());
        session.Inventory.Add(sword, 2);
        session.Inventory.Add(potion, 3);

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), catalog, new FixedSequenceRandom());

        Assert.Equal([GearPiece.Plain(sword), GearPiece.Plain(sword)], restored.Inventory.Gear);
        Assert.Equal(3, restored.Inventory.QuantityOf("potion"));
    }

    // Each piece keeps its rarity and its affixes in roll order, worn or owned; each affix's stat comes from the pool.
    [Fact]
    public void CaptureThenRestore_RolledPieces_SameRollsAndOrder()
    {
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
        var ring = new ItemDefinitionBuilder().WithId("power_ring").AsWorn(EquipmentKind.Ring).Build();
        var catalog = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(sword, ring).WithAffixes(Affix("power", StatKind.Power), Affix("crit_chance", StatKind.CritChance)).WithLeader("sarxian").Build();
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());
        var rolledSword = GearPiece.Rolled(sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        GearPiece[] owned = [GearPiece.Plain(ring), GearPiece.Rolled(ring, Rarity.Rare, [new RolledAffix("crit_chance", StatKind.CritChance, 2), new RolledAffix("power", StatKind.Power, 3)]), GearPiece.Plain(sword)];
        foreach (var piece in owned.Prepend(rolledSword))
        {
            session.Inventory.AddGear(piece);
        }

        Assert.True(session.Equipment.TryEquip(session.Party.Leader, rolledSword));

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), catalog, new FixedSequenceRandom());

        Assert.Equal(rolledSword, restored.Party.Leader.Weapon);
        Assert.Equal(owned, restored.Inventory.Gear);
    }

    [Fact]
    public void Load_HpAboveClassMaxWithConstitutionArmor_KeepsHp()
    {
        var built = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(_mail).WithLeader("sarxian").Build();
        var catalog = built with { Rules = built.Rules with { Stats = TestStatRules.Neutral with { HpPercentPer100Constitution = 100 } } };
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());
        session.Inventory.Add(_mail, 1);
        Assert.True(session.Equipment.TryEquip(session.Party.Leader, GearPiece.Plain(_mail)));
        session.Party.Leader.Rest();

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), catalog, new FixedSequenceRandom());

        var leader = restored.Party.Leader;
        Assert.Equal((110, 110), (leader.Vitals.Hp, leader.Vitals.MaxHp));
    }

    private static AffixDefinition Affix(string id, StatKind stat) =>
        new() { Id = id, Stat = stat, Min = 1, Max = 5, Kinds = [EquipmentKind.Weapon, EquipmentKind.Ring], Weight = 10 };
}
