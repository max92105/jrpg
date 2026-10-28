using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// Equipping names the slot (ADR 0028): an item goes only where its kind fits, and a ring fits both ring slots.
public sealed class PartyEquipmentSlotTests
{
    private readonly Inventory _inventory = new();
    private readonly PartyEquipment _equipment;
    private readonly PartyMember _sarxian = new PartyMemberBuilder().WithId("sarxian").Build();
    private readonly PartyMember _jazog = new PartyMemberBuilder().WithId("jazog").Build();
    private readonly ItemDefinition _ring = new ItemDefinitionBuilder()
        .WithId("power_ring")
        .AsWorn(EquipmentKind.Ring)
        .WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Power] = 5 }))
        .Build();
    private readonly ItemDefinition _helm = new ItemDefinitionBuilder().WithId("leather_cap").AsWorn(EquipmentKind.Head).Build();

    public PartyEquipmentSlotTests()
    {
        _equipment = new PartyEquipment(_inventory);
    }

    [Fact]
    public void TryEquip_RingInSecondRing_WearsIt()
    {
        _inventory.Add(_ring, 1);

        Assert.True(_equipment.TryEquip(_sarxian, _ring, EquipmentSlot.SecondRing));

        Assert.Same(_ring, _sarxian.EquippedIn(EquipmentSlot.SecondRing)?.Base);
        Assert.Null(_sarxian.EquippedIn(EquipmentSlot.FirstRing));
    }

    // Decision 329: two copies of one ring, one in each ring slot, and its +5 Power counted twice.
    [Fact]
    public void TryEquip_SameRingTwice_WearsBothAndAddsItsBonusTwice()
    {
        var powerBefore = _sarxian.Sheet.Primaries.Power;
        _inventory.Add(_ring, 2);

        Assert.True(_equipment.TryEquip(_sarxian, _ring, EquipmentSlot.FirstRing));
        Assert.True(_equipment.TryEquip(_sarxian, _ring, EquipmentSlot.SecondRing));

        Assert.Equal((_ring, _ring), (_sarxian.EquippedIn(EquipmentSlot.FirstRing)?.Base, _sarxian.EquippedIn(EquipmentSlot.SecondRing)?.Base));
        Assert.Equal(powerBefore + 10, _sarxian.Sheet.Primaries.Power);
        Assert.Equal(0, _inventory.QuantityOf(_ring.Id));
    }

    [Fact]
    public void TryEquip_WithoutASlot_UsesTheFirstTheKindFits()
    {
        _inventory.Add(_ring, 1);

        Assert.True(_equipment.TryEquip(_sarxian, _ring));

        Assert.Same(_ring, _sarxian.EquippedIn(EquipmentSlot.FirstRing)?.Base);
    }

    [Fact]
    public void TryEquip_HelmInChestSlot_RefusesAndChangesNothing()
    {
        _inventory.Add(_helm, 1);

        Assert.False(_equipment.TryEquip(_sarxian, _helm, EquipmentSlot.Chest));

        Assert.Null(_sarxian.EquippedIn(EquipmentSlot.Chest));
        Assert.Equal(1, _inventory.QuantityOf(_helm.Id));
    }

    [Fact]
    public void TakeableFor_SecondRing_ListsOtherMembersRings()
    {
        _inventory.Add(_ring, 1);
        Assert.True(_equipment.TryEquip(_jazog, _ring, EquipmentSlot.SecondRing));
        var party = new PartyState([_sarxian, _jazog], [_sarxian, _jazog], new PartyRules(MaxActiveMembers: 4));

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.SecondRing, party);

        Assert.Equal([new WornItem(_jazog, GearPiece.Plain(_ring))], takeable);
    }
}
