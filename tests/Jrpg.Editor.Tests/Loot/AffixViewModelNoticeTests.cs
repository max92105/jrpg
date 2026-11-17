using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// What an affix tells the page and the list as it is typed: its label, the notices the boxes read, and the Changed
// event the list's problem line listens to.
public sealed class AffixViewModelNoticeTests
{
    // Each typed field's edit, a value away from the fierce affix's.
    private static readonly Dictionary<string, Action<AffixViewModel>> _typings = new()
    {
        [nameof(AffixViewModel.Id)] = affix => affix.Id = "brutal",
        [nameof(AffixViewModel.Stat)] = affix => affix.Stat = StatKind.Power,
        [nameof(AffixViewModel.MinText)] = affix => affix.MinText = "2",
        [nameof(AffixViewModel.MaxText)] = affix => affix.MaxText = "9",
        [nameof(AffixViewModel.WeightText)] = affix => affix.WeightText = "8",
        [nameof(AffixViewModel.GoldValueText)] = affix => affix.GoldValueText = "40",
    };

    [Fact]
    public void Label_SavedAffix_NamesItsIdStatAndRange()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        Assert.Equal("fierce: CritDamage 3..7", affix.Label);
    }

    [Fact]
    public void Label_NewWithoutId_SaysNew()
    {
        var affix = AffixViewModel.New();

        Assert.Equal("(new affix)", affix.Label);
    }

    [Fact]
    public void Label_KindToggle_IsTheContentSpelling()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);

        Assert.Equal("offHand", AffixFixtures.Toggle(affix, EquipmentKind.OffHand).Label);
    }

    [Theory]
    [InlineData(nameof(AffixViewModel.Id), "brutal: CritDamage 3..7")]
    [InlineData(nameof(AffixViewModel.Stat), "fierce: Power 3..7")]
    [InlineData(nameof(AffixViewModel.MinText), "fierce: CritDamage 2..7")]
    [InlineData(nameof(AffixViewModel.MaxText), "fierce: CritDamage 3..9")]
    public void PropertyChanged_LabelFieldTyped_RaisesLabel(string field, string expected)
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);
        var raised = new List<string?>();
        affix.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _typings[field](affix);

        Assert.Contains(nameof(AffixViewModel.Label), raised);
        Assert.Equal(expected, affix.Label);
    }

    [Theory]
    [InlineData(nameof(AffixViewModel.Id))]
    [InlineData(nameof(AffixViewModel.MinText))]
    [InlineData(nameof(AffixViewModel.MaxText))]
    [InlineData(nameof(AffixViewModel.WeightText))]
    [InlineData(nameof(AffixViewModel.GoldValueText))]
    public void PropertyChanged_ProblemFieldTyped_RaisesProblem(string field)
    {
        var affix = AffixViewModel.New();
        var raised = new List<string?>();
        affix.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _typings[field](affix);

        Assert.Contains(nameof(AffixViewModel.Problem), raised);
    }

    [Theory]
    [InlineData(nameof(AffixViewModel.Id))]
    [InlineData(nameof(AffixViewModel.Stat))]
    [InlineData(nameof(AffixViewModel.MinText))]
    [InlineData(nameof(AffixViewModel.MaxText))]
    [InlineData(nameof(AffixViewModel.WeightText))]
    [InlineData(nameof(AffixViewModel.GoldValueText))]
    public void Changed_TypedFieldEdited_IsRaised(string field)
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);
        var raised = 0;
        affix.Changed += (_, _) => raised++;

        _typings[field](affix);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_KindTicked_IsRaised()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);
        var raised = 0;
        affix.Changed += (_, _) => raised++;

        AffixFixtures.Toggle(affix, EquipmentKind.Feet).IsChosen = true;

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_KindUnticked_IsRaised()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);
        var raised = 0;
        affix.Changed += (_, _) => raised++;

        AffixFixtures.Toggle(affix, EquipmentKind.Ring).IsChosen = false;

        Assert.Equal(1, raised);
    }

    // The box's typing also raises the label's and the problem's notices, which must not be relayed a second time.
    [Fact]
    public void Changed_ProblemNotice_IsNotRaised()
    {
        var affix = AffixViewModel.From(AffixFixtures.Fierce);
        var notices = new List<string?>();
        affix.PropertyChanged += (_, e) => notices.Add(e.PropertyName);
        var raised = 0;
        affix.Changed += (_, _) => raised++;

        affix.MinText = "x";

        Assert.Contains(nameof(AffixViewModel.Problem), notices);
        Assert.Equal(1, raised);
    }
}
