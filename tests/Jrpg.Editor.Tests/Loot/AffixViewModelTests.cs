using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// One affix as the Affixes page types it, written over the record it came from; the kinds compared as a list, since the
// record holds them in one and compares it by reference.
public sealed class AffixViewModelTests
{
    [Fact]
    public void From_Affix_TicksItsKindsOnly()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        var ticked = affix.Kinds.Where(toggle => toggle.IsChosen).Select(toggle => toggle.Kind);
        Assert.Equal([EquipmentKind.Weapon, EquipmentKind.Ring], ticked);
    }

    [Fact]
    public void Kinds_Constructed_AreEveryKindInEnumOrder()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        Assert.Equal(Enum.GetValues<EquipmentKind>(), affix.Kinds.Select(toggle => toggle.Kind));
    }

    [Fact]
    public void StatChoices_Constructed_AreEveryStatTheAffixOnlyOnesIncluded()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        Assert.Equal(Enum.GetValues<StatKind>(), affix.StatChoices);
    }

    [Fact]
    public void ToAffix_Untouched_EqualsTheSource()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        AssertSameContent(AffixFixtures.Fierce, affix.ToAffix());
    }

    [Fact]
    public void ToAffix_RangeAndWeightTyped_TakesThem()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.Stat = StatKind.LifeSteal;
        affix.MinText = " 2 ";
        affix.MaxText = "9";
        affix.WeightText = "8";
        affix.GoldValueText = "40";

        var expected = AffixFixtures.Fierce with { Stat = StatKind.LifeSteal, Min = 2, Max = 9, Weight = 8, GoldValue = 40 };
        AssertSameContent(expected, affix.ToAffix());
    }

    [Fact]
    public void ToAffix_KindsTickedOutOfOrder_AreInEnumOrder()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fireproof);

        AffixFixtures.Toggle(affix, EquipmentKind.Feet).IsChosen = true;
        AffixFixtures.Toggle(affix, EquipmentKind.Weapon).IsChosen = true;

        Assert.Equal([EquipmentKind.Weapon, EquipmentKind.Chest, EquipmentKind.Feet], affix.ToAffix().Kinds);
    }

    [Fact]
    public void ToAffix_KindUnticked_LeavesItOut()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        AffixFixtures.Toggle(affix, EquipmentKind.Weapon).IsChosen = false;

        Assert.Equal([EquipmentKind.Ring], affix.ToAffix().Kinds);
    }

    [Fact]
    public void ToAffix_WithAProblem_Throws()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.WeightText = "often";

        Assert.Throws<InvalidOperationException>(affix.ToAffix);
    }

    [Fact]
    public void Problem_MinNotANumber_NamesTheBox()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.MinText = "-";

        Assert.Equal("Min must be a whole number (was '-').", affix.Problem);
    }

    [Fact]
    public void Problem_MaxNotANumber_NamesTheBox()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.MaxText = "high";

        Assert.Equal("Max must be a whole number (was 'high').", affix.Problem);
    }

    [Fact]
    public void Problem_WeightNotANumber_NamesTheBox()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.WeightText = "often";

        Assert.Equal("Weight must be a whole number (was 'often').", affix.Problem);
    }

    [Fact]
    public void Problem_GoldValueNotANumber_NamesTheBox()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.GoldValueText = "lots";

        Assert.Equal("Gold value must be a whole number (was 'lots').", affix.Problem);
    }

    // A negative number is read: which side of 0 a range may take is Data's sign rule, shown by the list.
    [Fact]
    public void Problem_NegativeRange_IsNull()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        affix.MinText = "-7";
        affix.MaxText = "-3";

        Assert.Null(affix.Problem);
    }

    [Fact]
    public void Problem_NewWithoutId_NamesTheId()
    {
        var affix = AffixViewModel.New();

        Assert.Equal("The id must not be empty.", affix.Problem);
    }

    [Fact]
    public void Problem_SavedAffix_IgnoresTheIdRule()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce with { Id = "Old_Fierce" });

        Assert.Null(affix.Problem);
    }

    [Fact]
    public void New_StartsFromTheTunables()
    {
        var affix = AffixViewModel.New();
        affix.Id = "ward";

        var expected = new AffixDefinition
        {
            Id = "ward",
            Stat = StatKind.Power,
            Min = 1,
            Max = 1,
            Kinds = [],
            Weight = 10,
            GoldValue = 20,
        };
        Assert.True(affix.IsNew);
        AssertSameContent(expected, affix.ToAffix());
    }

    [Fact]
    public void Duplicate_Affix_IsNewWithEveryFieldButTheId()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        var copy = affix.Duplicate();
        var idWhileCopied = copy.Id;
        copy.Id = "fierce_2";

        Assert.True(copy.IsNew);
        Assert.Equal("", idWhileCopied);
        AssertSameContent(AffixFixtures.Fierce with { Id = "fierce_2" }, copy.ToAffix());
    }

    [Fact]
    public void Duplicate_TypedBoxes_CopiesThemAsTyped()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);
        affix.Stat = StatKind.DropLuck;
        affix.MaxText = "8x";
        AffixFixtures.Toggle(affix, EquipmentKind.Ring).IsChosen = false;

        var copy = affix.Duplicate();

        Assert.Equal((StatKind.DropLuck, "8x"), (copy.Stat, copy.MaxText));
        Assert.False(AffixFixtures.Toggle(copy, EquipmentKind.Ring).IsChosen);
    }

    // The record compares its kinds' list by reference, so the rest is compared whole with the kinds set alike, and the
    // kinds in order apart.
    private static void AssertSameContent(AffixDefinition expected, AffixDefinition actual)
    {
        Assert.Equal(expected, actual with { Kinds = expected.Kinds });
        Assert.Equal(expected.Kinds, actual.Kinds);
    }
}
