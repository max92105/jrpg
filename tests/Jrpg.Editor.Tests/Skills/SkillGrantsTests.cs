using Jrpg.Core.Statuses;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// The statuses a skill's effect grants as rows in order (decision 981): added after the chosen row on the first status
// by id, moved, removed, the first row's problem, the records the rows resolve to and the change event the skill passes
// on to the list.
public sealed class SkillGrantsTests
{
    private static readonly StatusDefinition _stun = new()
    {
        Id = "stun",
        Name = "Stun",
        Description = "Loses a turn.",
        IsHarmful = true,
        Parts = [new StatusPart(StatusPartKind.SkipTurn, 0)],
    };

    // Stun before poison, against the box's id order, so an added row reading the grants instead fails.
    private static readonly IReadOnlyList<StatusGrant> _stunThenPoison =
        [new StatusGrant(_stun, 1) { Chance = 60 }, new StatusGrant(SkillFixtures.Poison1, 3)];

    private static readonly SkillChoices _poisonAndStun = SkillFixtures.PoisonOnlyChoices with
    {
        Statuses = [new StatusChoice("poison_1", "poison_1: Poison"), new StatusChoice("stun", "stun: Stun")],
        StatusRecords = new Dictionary<string, StatusDefinition> { ["poison_1"] = SkillFixtures.Poison1, ["stun"] = _stun },
    };

    [Fact]
    public void From_SkillWithTwoGrants_RowsInOrder()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        var rows = grants.Rows.Select(row => (row.StatusId, row.TurnsText, row.ChanceText));
        Assert.Equal([("stun", "1", "60"), ("poison_1", "3", "100")], rows);
    }

    [Fact]
    public void AddRow_ChosenFirst_InsertsSecondWithOneTurnAndFullChance()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        grants.AddRowCommand.Execute(null);

        var rows = grants.Rows.Select(row => (row.StatusId, row.TurnsText, row.ChanceText));
        Assert.Equal([("stun", "1", "60"), ("poison_1", "1", "100"), ("poison_1", "3", "100")], rows);
        Assert.Same(grants.Rows[1], grants.SelectedRow);
    }

    [Fact]
    public void AddRow_NoStatusInTheDraft_Greyed()
    {
        var noStatus = _poisonAndStun with { Statuses = [], StatusRecords = new Dictionary<string, StatusDefinition>() };
        var grants = SkillGrants.From([], noStatus);

        Assert.False(grants.AddRowCommand.CanExecute(null));
        Assert.Empty(grants.Rows);
    }

    [Fact]
    public void RemoveRow_NoRow_Greyed()
    {
        var grants = SkillGrants.From([], _poisonAndStun);

        Assert.False(grants.RemoveRowCommand.CanExecute(null));
        Assert.False(grants.MoveUpCommand.CanExecute(null));
        Assert.False(grants.MoveDownCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveRow_Chosen_ChoosesTheNext()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        grants.RemoveRowCommand.Execute(null);

        Assert.Equal(["poison_1"], grants.Rows.Select(row => row.StatusId));
        Assert.Same(grants.Rows[0], grants.SelectedRow);
    }

    [Fact]
    public void MoveDown_FirstRow_SwapsTheOrder()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        grants.MoveDownCommand.Execute(null);

        Assert.Equal(["poison_1", "stun"], grants.ToGrants().Select(grant => grant.Status.Id));
    }

    [Fact]
    public void MoveUp_SecondRow_SwapsTheOrder()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        grants.SelectedRow = grants.Rows[1];

        grants.MoveUpCommand.Execute(null);

        Assert.Equal(["poison_1", "stun"], grants.Rows.Select(row => row.StatusId));
    }

    [Fact]
    public void SelectedRow_Chosen_RaisesRemoveAndMoveStates()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        var raised = new List<string>();
        grants.RemoveRowCommand.CanExecuteChanged += (_, _) => raised.Add("remove");
        grants.MoveUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        grants.MoveDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        grants.SelectedRow = grants.Rows[1];

        Assert.Equal(["remove", "up", "down"], raised);
    }

    [Fact]
    public void Problem_ChanceNotANumber_NamesTheRow()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        grants.Rows[1].ChanceText = "most";

        Assert.Equal("Statuses row 2: Chance must be a whole number (was 'most').", grants.Problem);
    }

    [Fact]
    public void Problem_TurnsNotANumber_NamesTheRow()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        grants.Rows[0].TurnsText = "";

        Assert.Equal("Statuses row 1: Turns must be a whole number (was '').", grants.Problem);
    }

    [Fact]
    public void Problem_TypedRows_IsNull()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        Assert.Null(grants.Problem);
    }

    // A copy equal by value but not the same instance: only a lookup in the draft's records gives back the draft's.
    [Fact]
    public void ToGrants_RowNamingPoison_ResolvesTheDraftRecord()
    {
        var grants = SkillGrants.From([new StatusGrant(SkillFixtures.Poison1 with { }, 2) { Chance = 40 }], _poisonAndStun);

        var grant = Assert.Single(grants.ToGrants());

        Assert.Same(SkillFixtures.Poison1, grant.Status);
        Assert.Equal((2, 40), (grant.Turns, grant.Chance));
    }

    [Fact]
    public void ToGrants_StatusPicked_ResolvesThePick()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);

        grants.Rows[0].StatusId = "poison_1";

        Assert.Same(SkillFixtures.Poison1, grants.ToGrants()[0].Status);
    }

    // Copied from the boxes: reading the half-typed turns would throw.
    [Fact]
    public void CopyOf_TurnsHalfTyped_CopiesTheTypedRows()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        grants.Rows[0].TurnsText = "-";

        var copy = SkillGrants.CopyOf(grants);

        Assert.Equal(("stun", "-", "60"), (copy.Rows[0].StatusId, copy.Rows[0].TurnsText, copy.Rows[0].ChanceText));
        Assert.NotSame(grants.Rows[0], copy.Rows[0]);
    }

    [Fact]
    public void CopyOf_NoStatusInTheDraft_AddStaysGreyed()
    {
        var noStatus = _poisonAndStun with { Statuses = [], StatusRecords = new Dictionary<string, StatusDefinition>() };

        var copy = SkillGrants.CopyOf(SkillGrants.From([], noStatus));

        Assert.False(copy.AddRowCommand.CanExecute(null));
    }

    [Fact]
    public void Changed_TurnsTyped_Raised()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        grants.Rows[0].TurnsText = "2";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_ChanceTyped_Raised()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        grants.Rows[1].ChanceText = "75";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_StatusPicked_Raised()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        grants.Rows[1].StatusId = "stun";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_RowMoved_Raised()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        grants.MoveDownCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    // A moved row is listed as both taken out and put in: it stays watched once, so typing in it tells the skill once.
    [Fact]
    public void Changed_MovedRowTyped_RaisedOnce()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        grants.MoveDownCommand.Execute(null);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        grants.Rows[1].TurnsText = "4";

        Assert.Equal(1, raised);
    }

    // A removed row is no longer watched, so typing in a row the skill no longer holds tells no one.
    [Fact]
    public void Changed_RemovedRowTyped_NotRaised()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        var removed = grants.Rows[0];
        grants.RemoveRowCommand.Execute(null);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        removed.TurnsText = "9";

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Changed_AddedRowTyped_Raised()
    {
        var grants = SkillGrants.From(_stunThenPoison, _poisonAndStun);
        grants.AddRowCommand.Execute(null);
        var raised = 0;
        grants.Changed += (_, _) => raised++;

        grants.SelectedRow!.ChanceText = "50";

        Assert.Equal(1, raised);
    }
}
