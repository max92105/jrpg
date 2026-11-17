using System.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

// The two rules naming another resource of the class as typed (decision 951), strain and dominance: on or off as the
// record has them, the blank an off rule shows (decision 1016), the other resource's id written as typed, the number
// box's problem, the Changed event for every typed field, and the copy a duplicated resource makes.
public sealed class ResourceRivalRuleTests
{
    // Every number differs from the blank's, and each id from the blank's none.
    private static readonly StrainRule _strain = new("mana", 3);
    private static readonly DominanceRule _dominance = new("decay", 50);

    [Fact]
    public void From_ResourceWithoutStrain_ShowsItOff() => Assert.False(StrainBoxes.From(null).IsOn);

    [Fact]
    public void From_Strain_ShowsItOn()
    {
        var strain = StrainBoxes.From(_strain);

        Assert.True(strain.IsOn);
        string[] shown = [strain.LowersStartOf, strain.PerMissingPointText];
        Assert.Equal(["mana", "3"], shown);
    }

    [Fact]
    public void From_Dominance_ShowsItOn()
    {
        var dominance = DominanceBoxes.From(_dominance);

        Assert.True(dominance.IsOn);
        string[] shown = [dominance.Over, dominance.BonusPercentText];
        Assert.Equal(["decay", "50"], shown);
    }

    [Fact]
    public void ToRule_UntouchedStrain_EqualsTheRecord() => Assert.Equal(_strain, StrainBoxes.From(_strain).ToRule());

    [Fact]
    public void ToRule_UntouchedDominance_EqualsTheRecord() =>
        Assert.Equal(_dominance, DominanceBoxes.From(_dominance).ToRule());

    // Ticked on a resource without it: the lowest amount, naming no resource yet (decision 1016).
    [Fact]
    public void ToRule_StrainTickedOn_WritesTheBlank()
    {
        var strain = StrainBoxes.From(null);

        strain.IsOn = true;

        Assert.Equal(new StrainRule("", 1), strain.ToRule());
    }

    [Fact]
    public void ToRule_DominanceTickedOn_WritesTheBlank()
    {
        var dominance = DominanceBoxes.From(null);

        dominance.IsOn = true;

        Assert.Equal(new DominanceRule("", 0), dominance.ToRule());
    }

    [Fact]
    public void ToRule_StrainOn_WritesTargetAndAmount()
    {
        var strain = StrainBoxes.From(_strain);

        strain.LowersStartOf = "focus";
        strain.PerMissingPointText = "2";

        Assert.Equal(new StrainRule("focus", 2), strain.ToRule());
    }

    [Fact]
    public void ToRule_DominanceOn_WritesRivalAndBonus()
    {
        var dominance = DominanceBoxes.From(_dominance);

        dominance.Over = "growth";
        dominance.BonusPercentText = "25";

        Assert.Equal(new DominanceRule("growth", 25), dominance.ToRule());
    }

    [Fact]
    public void Problem_PerMissingPointNotANumber_NamesIt()
    {
        var strain = StrainBoxes.From(_strain);

        strain.PerMissingPointText = "x";

        Assert.Equal("Strain per missing point must be a whole number (was 'x').", strain.Problem);
    }

    [Fact]
    public void Problem_BonusNotANumber_NamesIt()
    {
        var dominance = DominanceBoxes.From(_dominance);

        dominance.BonusPercentText = "half";

        Assert.Equal("Dominance bonus must be a whole number (was 'half').", dominance.Problem);
    }

    [Fact]
    public void Problem_PerMissingPointTyped_IsRaised()
    {
        var strain = StrainBoxes.From(null);

        Assert.Contains(nameof(StrainBoxes.Problem), NoticesOf(strain, () => strain.PerMissingPointText = "2"));
    }

    [Fact]
    public void Problem_BonusTyped_IsRaised()
    {
        var dominance = DominanceBoxes.From(null);

        Assert.Contains(nameof(DominanceBoxes.Problem), NoticesOf(dominance, () => dominance.BonusPercentText = "2"));
    }

    // The ids have no problem notice of their own: Data checks them.
    [Fact]
    public void Changed_RivalTyped_IsRaisedOnce()
    {
        var dominance = DominanceBoxes.From(_dominance);

        Assert.Equal(1, ChangesOf(dominance, () => dominance.Over = "growth"));
    }

    [Fact]
    public void Changed_StrainTargetTyped_IsRaisedOnce()
    {
        var strain = StrainBoxes.From(_strain);

        Assert.Equal(1, ChangesOf(strain, () => strain.LowersStartOf = "focus"));
    }

    [Fact]
    public void Changed_PerMissingPointTyped_IsRaisedOnce()
    {
        var strain = StrainBoxes.From(_strain);

        Assert.Equal(1, ChangesOf(strain, () => strain.PerMissingPointText = "4"));
    }

    [Fact]
    public void Changed_BonusTyped_IsRaisedOnce()
    {
        var dominance = DominanceBoxes.From(_dominance);

        Assert.Equal(1, ChangesOf(dominance, () => dominance.BonusPercentText = "40"));
    }

    [Fact]
    public void CopyFrom_HalfTypedStrain_CopiesIt()
    {
        var chosen = StrainBoxes.From(null);
        chosen.IsOn = true;
        chosen.LowersStartOf = "focus";
        chosen.PerMissingPointText = "1x";
        var copy = StrainBoxes.From(_strain);

        copy.CopyFrom(chosen);

        Assert.True(copy.IsOn);
        string[] copied = [copy.LowersStartOf, copy.PerMissingPointText];
        Assert.Equal(["focus", "1x"], copied);
    }

    [Fact]
    public void CopyFrom_OffDominance_CopiesItOff()
    {
        var chosen = DominanceBoxes.From(null);
        chosen.Over = "growth";
        chosen.BonusPercentText = "2x";
        var copy = DominanceBoxes.From(_dominance);

        copy.CopyFrom(chosen);

        Assert.False(copy.IsOn);
        string[] copied = [copy.Over, copy.BonusPercentText];
        Assert.Equal(["growth", "2x"], copied);
    }

    private static List<string?> NoticesOf(INotifyPropertyChanged boxes, Action type)
    {
        var raised = new List<string?>();
        boxes.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        type();
        return raised;
    }

    private static int ChangesOf<TRule>(ResourceRuleBoxes<TRule> boxes, Action type)
        where TRule : class
    {
        var raised = 0;
        boxes.Changed += (_, _) => raised++;
        type();
        return raised;
    }
}
