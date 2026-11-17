using System.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

// A resource's optional rules as typed (decision 951), through Will scaling and backfire: on or off as the record has
// them, the blank an off rule shows (decision 1016), nothing written while off, the boxes' problem only while on, the
// Changed event a resource listens to, and the copy a duplicated resource makes.
public sealed class ResourceRuleTests
{
    // Every number differs from the blank's, and the effect from the blank's raised max.
    private static readonly BackfireRule _backfire = new(5, 15);
    private static readonly WillScaling _willScaling = new(4, WillEffect.LowerGain);

    [Fact]
    public void From_Backfire_ShowsItOn()
    {
        var backfire = BackfireBoxes.From(_backfire);

        Assert.True(backfire.IsOn);
        string[] shown = [backfire.ChancePerPointText, backfire.DamagePercentText];
        Assert.Equal(["5", "15"], shown);
    }

    [Fact]
    public void From_WillScaling_ShowsItOn()
    {
        var willScaling = WillScalingBoxes.From(_willScaling);

        Assert.True(willScaling.IsOn);
        Assert.Equal("4", willScaling.WillPerPointText);
        Assert.Equal(WillEffect.LowerGain, willScaling.Effect);
    }

    [Fact]
    public void From_NoWillScaling_ShowsItOff() => Assert.False(WillScalingBoxes.From(null).IsOn);

    [Fact]
    public void ToRule_Off_IsNull()
    {
        var backfire = BackfireBoxes.From(_backfire);

        backfire.IsOn = false;

        Assert.Null(backfire.ToRule());
    }

    [Fact]
    public void ToRule_UntouchedBackfire_EqualsTheRecord() => Assert.Equal(_backfire, BackfireBoxes.From(_backfire).ToRule());

    [Fact]
    public void ToRule_UntouchedWillScaling_EqualsTheRecord() =>
        Assert.Equal(_willScaling, WillScalingBoxes.From(_willScaling).ToRule());

    // Ticked on a resource without it: the lowest Will per point, raising the max (decision 1016).
    [Fact]
    public void ToRule_WillScalingTickedOn_WritesTheBlank()
    {
        var willScaling = WillScalingBoxes.From(null);

        willScaling.IsOn = true;

        Assert.Equal(new WillScaling(1, WillEffect.RaiseMax), willScaling.ToRule());
    }

    [Fact]
    public void ToRule_BackfireTickedOn_WritesTheBlank()
    {
        var backfire = BackfireBoxes.From(null);

        backfire.IsOn = true;

        Assert.Equal(new BackfireRule(1, 1), backfire.ToRule());
    }

    [Fact]
    public void ToRule_WillScalingOn_WritesPerPointAndEffect()
    {
        var willScaling = WillScalingBoxes.From(new WillScaling(4, WillEffect.RaiseStart));

        willScaling.WillPerPointText = "7";
        willScaling.Effect = WillEffect.LowerGain;

        Assert.Equal(new WillScaling(7, WillEffect.LowerGain), willScaling.ToRule());
    }

    [Fact]
    public void ToRule_BackfireOn_WritesBothPercents()
    {
        var backfire = BackfireBoxes.From(null);

        backfire.IsOn = true;
        backfire.ChancePerPointText = "10";
        backfire.DamagePercentText = " 20 ";

        Assert.Equal(new BackfireRule(10, 20), backfire.ToRule());
    }

    [Fact]
    public void ToRule_OnWithBadBox_Throws()
    {
        var backfire = BackfireBoxes.From(_backfire);

        backfire.DamagePercentText = "";

        Assert.Throws<InvalidOperationException>(() => backfire.ToRule());
    }

    [Fact]
    public void Problem_OffRuleWithBadBox_IsNull()
    {
        var backfire = BackfireBoxes.From(null);

        backfire.ChancePerPointText = "x";

        Assert.Null(backfire.Problem);
    }

    [Fact]
    public void Problem_OnRuleWithBadBox_NamesIt()
    {
        var backfire = BackfireBoxes.From(_backfire);

        backfire.ChancePerPointText = "x";

        Assert.Equal("Backfire chance per point must be a whole number (was 'x').", backfire.Problem);
    }

    [Fact]
    public void Problem_DamageNotANumber_NamesIt()
    {
        var backfire = BackfireBoxes.From(_backfire);

        backfire.DamagePercentText = "1.5";

        Assert.Equal("Backfire damage must be a whole number (was '1.5').", backfire.Problem);
    }

    [Fact]
    public void Problem_WillPerPointNotANumber_NamesIt()
    {
        var willScaling = WillScalingBoxes.From(_willScaling);

        willScaling.WillPerPointText = "ten";

        Assert.Equal("Will per point must be a whole number (was 'ten').", willScaling.Problem);
    }

    [Fact]
    public void Problem_Ticked_IsRaised()
    {
        var backfire = BackfireBoxes.From(null);

        Assert.Contains(nameof(BackfireBoxes.Problem), NoticesOf(backfire, () => backfire.IsOn = true));
    }

    [Fact]
    public void Problem_ChanceTyped_IsRaised()
    {
        var backfire = BackfireBoxes.From(null);

        Assert.Contains(nameof(BackfireBoxes.Problem), NoticesOf(backfire, () => backfire.ChancePerPointText = "2"));
    }

    [Fact]
    public void Problem_DamageTyped_IsRaised()
    {
        var backfire = BackfireBoxes.From(null);

        Assert.Contains(nameof(BackfireBoxes.Problem), NoticesOf(backfire, () => backfire.DamagePercentText = "2"));
    }

    [Fact]
    public void Problem_WillPerPointTyped_IsRaised()
    {
        var willScaling = WillScalingBoxes.From(null);

        Assert.Contains(nameof(WillScalingBoxes.Problem), NoticesOf(willScaling, () => willScaling.WillPerPointText = "2"));
    }

    // A typed box raises its own notice and the problem's: the event stands for the one typed change.
    [Fact]
    public void Changed_BoxTyped_IsRaisedOnce()
    {
        var backfire = BackfireBoxes.From(_backfire);

        Assert.Equal(1, ChangesOf(backfire, () => backfire.DamagePercentText = "30"));
    }

    [Fact]
    public void Changed_Ticked_IsRaisedOnce()
    {
        var backfire = BackfireBoxes.From(_backfire);

        Assert.Equal(1, ChangesOf(backfire, () => backfire.IsOn = false));
    }

    [Fact]
    public void Changed_EffectChosen_IsRaisedOnce()
    {
        var willScaling = WillScalingBoxes.From(_willScaling);

        Assert.Equal(1, ChangesOf(willScaling, () => willScaling.Effect = WillEffect.RaiseMax));
    }

    [Fact]
    public void Changed_WillPerPointTyped_IsRaisedOnce()
    {
        var willScaling = WillScalingBoxes.From(_willScaling);

        Assert.Equal(1, ChangesOf(willScaling, () => willScaling.WillPerPointText = "6"));
    }

    [Fact]
    public void Changed_ChanceTyped_IsRaisedOnce()
    {
        var backfire = BackfireBoxes.From(_backfire);

        Assert.Equal(1, ChangesOf(backfire, () => backfire.ChancePerPointText = "8"));
    }

    [Fact]
    public void CopyFrom_HalfTypedBackfire_CopiesIt()
    {
        var chosen = BackfireBoxes.From(null);
        chosen.IsOn = true;
        chosen.ChancePerPointText = "1x";
        chosen.DamagePercentText = "25";
        var copy = BackfireBoxes.From(_backfire);

        copy.CopyFrom(chosen);

        Assert.True(copy.IsOn);
        string[] copied = [copy.ChancePerPointText, copy.DamagePercentText];
        Assert.Equal(["1x", "25"], copied);
    }

    [Fact]
    public void CopyFrom_OffWillScaling_CopiesItOff()
    {
        var chosen = WillScalingBoxes.From(null);
        chosen.WillPerPointText = "2x";
        chosen.Effect = WillEffect.RaiseStart;
        var copy = WillScalingBoxes.From(_willScaling);

        copy.CopyFrom(chosen);

        Assert.False(copy.IsOn);
        Assert.Equal("2x", copy.WillPerPointText);
        Assert.Equal(WillEffect.RaiseStart, copy.Effect);
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
