using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// An equipment's form as typed (decision 880): its kind, and a weapon's damage and type, hidden, never read and written
// empty off a weapon; a kind changed keeps the bonuses, whose rows ItemEquipmentViewModelBonusTests covers.
public sealed class ItemEquipmentViewModelTests
{
    private static readonly EquipmentStats _axe = ItemFixtures.CopperAxe.Equipment!;

    // The vest's bonuses on a ring: no damage and no type, as Data requires off a weapon.
    private static readonly EquipmentStats _ring = ItemFixtures.CopperVest.Equipment! with { Kind = EquipmentKind.Ring };

    // Each typed box's edit, a value away from the axe's.
    private static readonly Dictionary<string, Action<ItemEquipmentViewModel>> _typings = new()
    {
        [nameof(ItemEquipmentViewModel.Kind)] = equipment => equipment.Kind = EquipmentKind.Hands,
        [nameof(ItemEquipmentViewModel.DamageText)] = equipment => equipment.DamageText = "12",
        [nameof(ItemEquipmentViewModel.DamageType)] = equipment => equipment.DamageType = DamageType.Fire,
    };

    [Fact]
    public void From_Weapon_ShowsItsKindDamageAndType()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);

        var expected = (EquipmentKind.Weapon, "9", (DamageType?)DamageType.Slashing);
        Assert.Equal(expected, (equipment.Kind, equipment.DamageText, equipment.DamageType));
        Assert.True(equipment.IsWeapon);
    }

    [Fact]
    public void From_Ring_StartsTheDamageBoxesEmpty()
    {
        var equipment = ItemEquipmentViewModel.From(_ring);

        Assert.Equal(("", (DamageType?)null), (equipment.DamageText, equipment.DamageType));
        Assert.False(equipment.IsWeapon);
    }

    [Fact]
    public void KindChoices_ListEveryEquipmentKind()
    {
        var equipment = ItemEquipmentViewModel.From(_ring);

        Assert.Equal(Enum.GetValues<EquipmentKind>(), equipment.KindChoices);
    }

    [Fact]
    public void DamageTypeChoices_ListEveryDamageType()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);

        Assert.Equal(Enum.GetValues<DamageType>(), equipment.DamageTypeChoices);
    }

    [Fact]
    public void ToEquipment_Untouched_EqualsTheSource()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);

        Assert.Equal(_axe, equipment.ToEquipment());
    }

    [Fact]
    public void ToEquipment_WeaponTurnedRing_DropsDamageAndType()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);

        equipment.Kind = EquipmentKind.Ring;

        Assert.Equal(_axe with { Kind = EquipmentKind.Ring, Damage = 0, DamageType = null }, equipment.ToEquipment());
    }

    [Fact]
    public void ToEquipment_RingTurnedWeapon_TakesTheTypedDamageAndType()
    {
        var equipment = ItemEquipmentViewModel.From(_ring);

        equipment.Kind = EquipmentKind.Weapon;
        equipment.DamageText = "14";
        equipment.DamageType = DamageType.Fire;

        var expected = _ring with { Kind = EquipmentKind.Weapon, Damage = 14, DamageType = DamageType.Fire };
        Assert.Equal(expected, equipment.ToEquipment());
    }

    [Fact]
    public void ToEquipment_KindChanged_CarriesTheBonuses()
    {
        var equipment = ItemEquipmentViewModel.From(_ring);

        equipment.Kind = EquipmentKind.Head;

        Assert.Equal(_ring.Bonuses, equipment.ToEquipment().Bonuses);
    }

    [Fact]
    public void ToEquipment_WithAProblem_Throws()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);

        equipment.DamageText = "big";

        Assert.Throws<InvalidOperationException>(equipment.ToEquipment);
    }

    [Fact]
    public void Problem_WeaponDamageNotANumber_NamesTheBox()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);

        equipment.DamageText = "big";

        Assert.Equal("Damage must be a whole number (was 'big').", equipment.Problem);
    }

    [Fact]
    public void Problem_RingTurnedWeapon_AsksForTheDamage()
    {
        var equipment = ItemEquipmentViewModel.From(_ring);

        equipment.Kind = EquipmentKind.Weapon;

        Assert.Equal("Damage must be a whole number (was '').", equipment.Problem);
    }

    // The box is hidden off a weapon, so what it still holds is never read.
    [Fact]
    public void Problem_RingWithDamageBoxGarbage_IsNull()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);
        equipment.DamageText = "big";

        equipment.Kind = EquipmentKind.Ring;

        Assert.Null(equipment.Problem);
    }

    [Fact]
    public void CopyOf_HalfTypedDamage_CopiesTheBoxesAsTyped()
    {
        var equipment = ItemEquipmentViewModel.From(_ring);
        equipment.Kind = EquipmentKind.Weapon;
        equipment.DamageText = "1x";
        equipment.DamageType = DamageType.Ice;

        var copy = ItemEquipmentViewModel.CopyOf(equipment);

        Assert.Equal((EquipmentKind.Weapon, "1x", (DamageType?)DamageType.Ice), (copy.Kind, copy.DamageText, copy.DamageType));
    }

    [Fact]
    public void PropertyChanged_KindChosen_RaisesIsWeaponAndProblem()
    {
        var equipment = ItemEquipmentViewModel.From(_axe);
        var raised = new List<string?>();
        equipment.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        equipment.Kind = EquipmentKind.Ring;

        Assert.Contains(nameof(ItemEquipmentViewModel.IsWeapon), raised);
        Assert.Contains(nameof(ItemEquipmentViewModel.Problem), raised);
    }

    [Theory]
    [InlineData(nameof(ItemEquipmentViewModel.Kind))]
    [InlineData(nameof(ItemEquipmentViewModel.DamageText))]
    [InlineData(nameof(ItemEquipmentViewModel.DamageType))]
    public void Changed_TypedBoxEdited_IsRaised(string box)
    {
        var equipment = ItemEquipmentViewModel.From(_axe);
        var raised = 0;
        equipment.Changed += (_, _) => raised++;

        _typings[box](equipment);

        Assert.Equal(1, raised);
    }
}
