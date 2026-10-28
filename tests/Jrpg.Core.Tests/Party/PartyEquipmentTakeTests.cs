using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

/// <summary>
/// Taking equipment from another member (decision 137). Sarxian, Jazog and Teelle are fighters, Zaruk is a mage; Teelle
/// waits in the reserve.
/// </summary>
public sealed class PartyEquipmentTakeTests
{
    private readonly Inventory _inventory = new();
    private readonly PartyEquipment _equipment;
    private readonly PartyMember _sarxian = new PartyMemberBuilder().WithId("sarxian").WithClass("fighter").Build();
    private readonly PartyMember _jazog = new PartyMemberBuilder().WithId("jazog").WithClass("fighter").Build();
    private readonly PartyMember _zaruk = new PartyMemberBuilder().WithId("zaruk").WithClass("mage").Build();
    private readonly PartyMember _teelle = new PartyMemberBuilder().WithId("teelle").WithClass("fighter").Build();
    private readonly ItemDefinition _bronzeSword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
    private readonly ItemDefinition _ironSword = new ItemDefinitionBuilder().WithId("iron_sword").AsWeapon(8).Build();
    private readonly ItemDefinition _staff = new ItemDefinitionBuilder().WithId("magic_staff").AsWeapon(6).Build();
    private readonly ItemDefinition _chainMail = new ItemDefinitionBuilder().WithId("chain_mail").AsChest().Build();
    private readonly PartyState _party;

    public PartyEquipmentTakeTests()
    {
        _equipment = new PartyEquipment(_inventory);
        _party = new PartyState([_sarxian, _teelle, _jazog, _zaruk], [_sarxian, _jazog, _zaruk], new PartyRules(MaxActiveMembers: 4));
    }

    [Fact]
    public void TakeableFor_TwoOthersWearIt_ListsOneEntryPerWearer()
    {
        Dress(_jazog, _ironSword);
        Dress(_teelle, _ironSword);

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.Weapon, _party);

        Assert.Equal([new WornItem(_jazog, GearPiece.Plain(_ironSword)), new WornItem(_teelle, GearPiece.Plain(_ironSword))], takeable);
    }

    [Fact]
    public void TakeableFor_ReserveMemberWearsIt_ListsThem()
    {
        Dress(_teelle, _ironSword);

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.Weapon, _party);

        Assert.Equal([new WornItem(_teelle, GearPiece.Plain(_ironSword))], takeable);
    }

    [Fact]
    public void TakeableFor_AnyClass_OffersIt()
    {
        Dress(_zaruk, _staff);

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.Weapon, _party);

        Assert.Equal(new WornItem(_zaruk, GearPiece.Plain(_staff)), Assert.Single(takeable));
    }

    [Fact]
    public void TakeableFor_MemberAlreadyWearsIt_LeavesItOut()
    {
        Dress(_sarxian, _ironSword);
        Dress(_jazog, _ironSword);

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.Weapon, _party);

        Assert.Empty(takeable);
    }

    [Fact]
    public void TakeableFor_OtherSlot_LeavesItOut()
    {
        Dress(_jazog, _chainMail);

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.Weapon, _party);

        Assert.Empty(takeable);
    }

    [Fact]
    public void TakeableFor_InPartyOrder_ActiveThenReserve()
    {
        Dress(_teelle, _bronzeSword);
        Dress(_jazog, _ironSword);

        var takeable = PartyEquipment.TakeableFor(_sarxian, EquipmentSlot.Weapon, _party);

        Assert.Equal([_jazog, _teelle], takeable.Select(worn => worn.Wearer));
    }

    [Fact]
    public void TryTakeFrom_GiverWearsIt_MovesItAndEmptiesTheGiversSlot()
    {
        Dress(_jazog, _ironSword);

        var taken = _equipment.TryTakeFrom(_jazog, _sarxian, EquipmentSlot.Weapon);

        Assert.True(taken);
        Assert.Same(_ironSword, _sarxian.Weapon?.Base);
        Assert.Null(_jazog.Weapon);
        Assert.Equal(0, _inventory.GearCountOf("iron_sword"));
    }

    [Fact]
    public void TryTakeFrom_ReceiverWearsSomething_PutsItInTheBag()
    {
        Dress(_jazog, _ironSword);
        Dress(_sarxian, _bronzeSword);

        _equipment.TryTakeFrom(_jazog, _sarxian, EquipmentSlot.Weapon);

        Assert.Same(_ironSword, _sarxian.Weapon?.Base);
        Assert.Equal(1, _inventory.GearCountOf("bronze_sword"));
    }

    [Fact]
    public void TryTakeFrom_ReceiverWearsTheSameItem_ChangesNothing()
    {
        Dress(_jazog, _ironSword);
        Dress(_sarxian, _ironSword);

        var taken = _equipment.TryTakeFrom(_jazog, _sarxian, EquipmentSlot.Weapon);

        Assert.False(taken);
        Assert.Same(_ironSword, _jazog.Weapon?.Base);
        Assert.Equal(0, _inventory.GearCountOf("iron_sword"));
    }

    [Fact]
    public void TryTakeFrom_GiverSlotEmpty_ReturnsFalse()
    {
        var taken = _equipment.TryTakeFrom(_jazog, _sarxian, EquipmentSlot.Weapon);

        Assert.False(taken);
    }

    [Fact]
    public void TryTakeFrom_SameMember_ReturnsFalse()
    {
        Dress(_sarxian, _ironSword);

        var taken = _equipment.TryTakeFrom(_sarxian, _sarxian, EquipmentSlot.Weapon);

        Assert.False(taken);
        Assert.Same(_ironSword, _sarxian.Weapon?.Base);
    }

    // Puts the item on through the bag, as the game does.
    private void Dress(PartyMember member, ItemDefinition item)
    {
        _inventory.Add(item, 1);
        Assert.True(_equipment.TryEquip(member, item));
    }
}
