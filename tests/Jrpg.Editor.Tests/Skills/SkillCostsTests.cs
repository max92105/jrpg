using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// A skill's costs as rows (decision 979): changes and needs as typed, added after the chosen row, the summary line, the
// first row's problem and the change event the skill relays to the list.
public sealed class SkillCostsTests
{
    private static readonly SkillDefinition _manaAndFocus = SkillFixtures.Bolt with
    {
        ResourceChanges = [new ResourceChange("mana", -2), new ResourceChange("focus", 1)],
        Requirements = [new ResourceRequirement("faith", 1), new ResourceRequirement("mana", 3)],
    };

    [Fact]
    public void From_SkillWithCostAndNeed_FillsBothLists()
    {
        var costs = SkillCosts.From(SkillFixtures.Bolt, "faith");

        Assert.Equal([("mana", "-2")], costs.Changes.Select(row => (row.ResourceId, row.AmountText)));
        Assert.Equal([("mana", "2")], costs.Needs.Select(row => (row.ResourceId, row.AtLeastText)));
    }

    [Fact]
    public void SummaryText_SpendAndGainAndNeed_ReadsAllThree()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");

        Assert.Equal("Spends 2 mana, gains 1 focus; needs 1 faith and 3 mana", costs.SummaryText);
    }

    [Fact]
    public void SummaryText_NoRow_IsFree()
    {
        var costs = SkillCosts.From(SkillFixtures.Bolt with { ResourceChanges = [], Requirements = [] }, "mana");

        Assert.Equal("Free", costs.SummaryText);
    }

    [Fact]
    public void SummaryText_AmountTyped_IsRaisedWithTheNewAmount()
    {
        var costs = SkillCosts.From(SkillFixtures.Bolt with { Requirements = [] }, "mana");
        var notified = new List<string?>();
        costs.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        costs.Changes[0].AmountText = "-5";

        Assert.Contains(nameof(SkillCosts.SummaryText), notified);
        Assert.Equal("Spends 5 mana", costs.SummaryText);
    }

    // The chosen row is the first, so an add put last would fail.
    [Fact]
    public void AddChange_ChosenRow_InsertsAfterItWithMinusOne()
    {
        var costs = SkillCosts.From(_manaAndFocus, "clarity");

        costs.AddChangeCommand.Execute(null);

        Assert.Equal(["mana", "clarity", "focus"], costs.Changes.Select(row => row.ResourceId));
        Assert.Equal("-1", costs.Changes[1].AmountText);
        Assert.Same(costs.Changes[1], costs.SelectedChange);
    }

    [Fact]
    public void AddNeed_ChosenRow_InsertsAfterItWithOne()
    {
        var costs = SkillCosts.From(_manaAndFocus, "clarity");

        costs.AddNeedCommand.Execute(null);

        var needs = costs.Needs.Select(row => (row.ResourceId, row.AtLeastText));
        Assert.Equal([("faith", "1"), ("clarity", "1"), ("mana", "3")], needs);
    }

    [Fact]
    public void RemoveChange_NoRow_Greyed()
    {
        var costs = SkillCosts.From(SkillFixtures.Bolt with { ResourceChanges = [] }, "mana");

        Assert.False(costs.RemoveChangeCommand.CanExecute(null));
        Assert.Empty(costs.Changes);
    }

    [Fact]
    public void RemoveNeed_NoRow_Greyed()
    {
        var costs = SkillCosts.From(SkillFixtures.Bolt with { Requirements = [] }, "mana");

        Assert.False(costs.RemoveNeedCommand.CanExecute(null));
        Assert.Empty(costs.Needs);
    }

    [Fact]
    public void SelectedChange_Chosen_RaisesRemoveState()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        var raised = 0;
        costs.RemoveChangeCommand.CanExecuteChanged += (_, _) => raised++;

        costs.SelectedChange = costs.Changes[1];

        Assert.Equal(1, raised);
    }

    [Fact]
    public void SelectedNeed_Chosen_RaisesRemoveState()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        var raised = 0;
        costs.RemoveNeedCommand.CanExecuteChanged += (_, _) => raised++;

        costs.SelectedNeed = costs.Needs[1];

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Problem_AmountNotANumber_NamesTheRow()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");

        costs.Changes[1].AmountText = "lots";

        Assert.Equal("Changes row 2: Amount must be a whole number (was 'lots').", costs.Problem);
    }

    [Fact]
    public void Problem_NoResourceOffered_AddedRowNamesIt()
    {
        var costs = SkillCosts.From(_manaAndFocus with { Requirements = [] }, "");

        costs.AddNeedCommand.Execute(null);

        Assert.Equal("Needs row 1: Resource must be picked.", costs.Problem);
    }

    [Fact]
    public void Problem_TypedRows_IsNull()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");

        Assert.Null(costs.Problem);
    }

    [Fact]
    public void ToChanges_TypedRows_InOrder()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");

        costs.Changes[0].ResourceId = "clarity";

        Assert.Equal([new ResourceChange("clarity", -2), new ResourceChange("focus", 1)], costs.ToChanges());
    }

    [Fact]
    public void ToRequirements_TypedRows_InOrder()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");

        costs.Needs[1].AtLeastText = " 4 ";

        Assert.Equal([new ResourceRequirement("faith", 1), new ResourceRequirement("mana", 4)], costs.ToRequirements());
    }

    [Fact]
    public void Changed_AmountTyped_Raised()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        var raised = 0;
        costs.Changed += (_, _) => raised++;

        costs.Changes[0].AmountText = "-4";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_NeedResourcePicked_Raised()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        var raised = 0;
        costs.Changed += (_, _) => raised++;

        costs.Needs[0].ResourceId = "clarity";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_RowRemoved_Raised()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        var raised = 0;
        costs.Changed += (_, _) => raised++;

        costs.RemoveNeedCommand.Execute(null);

        Assert.Equal(1, raised);
        Assert.Equal(["mana"], costs.Needs.Select(row => row.ResourceId));
    }

    // An added row is watched: typing in it after the add tells the skill.
    [Fact]
    public void Changed_AddedRowTyped_Raised()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        costs.AddChangeCommand.Execute(null);
        var raised = 0;
        costs.Changed += (_, _) => raised++;

        costs.SelectedChange!.AmountText = "-2";

        Assert.Equal(1, raised);
    }

    // A removed row is no longer watched, so typing in a row the skill no longer holds tells no one.
    [Fact]
    public void Changed_RemovedRowTyped_NotRaised()
    {
        var costs = SkillCosts.From(_manaAndFocus, "mana");
        var removed = costs.Changes[0];
        costs.RemoveChangeCommand.Execute(null);
        var raised = 0;
        costs.Changed += (_, _) => raised++;

        removed.AmountText = "-9";

        Assert.Equal(0, raised);
    }
}
