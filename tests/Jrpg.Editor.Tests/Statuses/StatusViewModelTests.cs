using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Statuses;

// One status as the Statuses page edits it (decision 984): its boxes, its part rows in order, the skills granting it,
// the id fixed once saved, and the change event the list's problem line listens to.
public sealed class StatusViewModelTests
{
    // Every field off its record default, so a box left unread fails a test.
    private static readonly StatusDefinition _burning = new()
    {
        Id = "burning_1",
        Name = "Burning",
        Description = "Loses HP as its turn starts.",
        IsHarmful = true,
        ActsAt = StatusTiming.StartOfTurn,
        Family = "burning",
        MarkLetter = "B",
        Parts =
        [
            new StatusPart(StatusPartKind.DamageEachTurn, 6, DamageType.Fire),
            new StatusPart(StatusPartKind.StatChange, -10, Stat: StatKind.Accuracy),
        ],
    };

    [Fact]
    public void From_Status_FillsEveryBoxAndPart()
    {
        var status = StatusViewModel.From(_burning, []);

        Assert.Equal(
            ("burning_1", "Burning", "Loses HP as its turn starts.", true, "burning", StatusTiming.StartOfTurn, "B"),
            (status.IdText, status.Name, status.Description, status.IsHarmful, status.FamilyText, status.ActsAt,
                status.MarkLetterText));
        var parts = status.Parts.Select(part => (part.Kind, part.AmountText, part.DamageType, part.Stat));
        Assert.Equal(
            [
                (StatusPartKind.DamageEachTurn, "6", (DamageType?)DamageType.Fire, (StatKind?)null),
                (StatusPartKind.StatChange, "-10", null, StatKind.Accuracy),
            ],
            parts);
        Assert.False(status.IsNew);
    }

    [Fact]
    public void ToStatus_Untouched_EqualsTheRecord()
    {
        var status = StatusViewModel.From(_burning, []);

        Assert.Equal(_burning, status.ToStatus());
    }

    [Fact]
    public void ToStatus_EmptyFamilyAndLetter_WritesNulls()
    {
        var status = StatusViewModel.From(_burning, []);

        status.FamilyText = "";
        status.MarkLetterText = " ";

        Assert.Equal(_burning with { Family = null, MarkLetter = null }, status.ToStatus());
    }

    [Fact]
    public void ToStatus_StartOfTurn_Kept()
    {
        var status = StatusViewModel.From(SkillFixtures.Poison1, []);

        status.ActsAt = StatusTiming.StartOfTurn;

        Assert.Equal(StatusTiming.StartOfTurn, status.ToStatus().ActsAt);
    }

    [Fact]
    public void AddPart_ChosenFirst_InsertsSecond()
    {
        var status = StatusViewModel.From(_burning, []);

        status.AddPartCommand.Execute(null);

        Assert.Equal(
            [StatusPartKind.DamageEachTurn, StatusPartKind.DamageEachTurn, StatusPartKind.StatChange],
            status.Parts.Select(part => part.Kind));
        Assert.Equal(new StatusPart(StatusPartKind.DamageEachTurn, 1), status.Parts[1].ToPart());
        Assert.Same(status.Parts[1], status.SelectedPart);
    }

    [Fact]
    public void MovePartDown_FirstPart_ToStatusReadsTheNewOrder()
    {
        var status = StatusViewModel.From(_burning, []);

        status.MovePartDownCommand.Execute(null);

        Assert.Equal([.. _burning.Parts.Reverse()], status.ToStatus().Parts);
    }

    [Fact]
    public void MovePartUp_SecondPart_SwapsTheOrder()
    {
        var status = StatusViewModel.From(_burning, []);
        status.SelectedPart = status.Parts[1];

        status.MovePartUpCommand.Execute(null);

        Assert.Equal([.. _burning.Parts.Reverse()], status.ToStatus().Parts);
    }

    [Fact]
    public void RemovePart_Chosen_ChoosesTheNext()
    {
        var status = StatusViewModel.From(_burning, []);

        status.RemovePartCommand.Execute(null);

        Assert.Equal([_burning.Parts[1]], status.ToStatus().Parts);
        Assert.Same(status.Parts[0], status.SelectedPart);
    }

    [Fact]
    public void RemovePart_NoPart_Greyed()
    {
        var status = StatusViewModel.From(_burning with { Parts = [] }, []);

        Assert.False(status.RemovePartCommand.CanExecute(null));
        Assert.False(status.MovePartUpCommand.CanExecute(null));
        Assert.False(status.MovePartDownCommand.CanExecute(null));
    }

    [Fact]
    public void SelectedPart_Chosen_RaisesRemoveAndMoveStates()
    {
        var status = StatusViewModel.From(_burning, []);
        var raised = new List<string>();
        status.RemovePartCommand.CanExecuteChanged += (_, _) => raised.Add("remove");
        status.MovePartUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        status.MovePartDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        status.SelectedPart = status.Parts[1];

        Assert.Equal(["remove", "up", "down"], raised);
    }

    // Set up against id order, with a skill granting another status between them.
    [Fact]
    public void UsedByText_SkillGrantsIt_NamesIt()
    {
        var zap = SkillFixtures.Bolt with { Id = "zap" };
        var flame = SkillFixtures.Bolt with
        {
            Id = "flame",
            Effect = SkillFixtures.Bolt.Effect with { Statuses = [new StatusGrant(_burning, 2)] },
        };

        var status = StatusViewModel.From(SkillFixtures.Poison1, [zap, flame, SkillFixtures.Bolt]);

        Assert.Equal("Used by: bolt, zap", status.UsedByText);
    }

    [Fact]
    public void UsedByText_NoSkill_SaysSo()
    {
        var status = StatusViewModel.From(_burning, [SkillFixtures.Bolt]);

        Assert.Equal("Used by no skill", status.UsedByText);
    }

    [Fact]
    public void Problem_NewWithATakenId_NamesIt()
    {
        var status = StatusViewModel.New(["poison_1"]);

        status.IdText = "poison_1";

        Assert.Equal("The id poison_1 is taken.", status.Problem);
    }

    [Fact]
    public void Problem_SecondPartAmountNotANumber_NamesTheRow()
    {
        var status = StatusViewModel.From(_burning, []);

        status.Parts[1].AmountText = "x";

        Assert.Equal("Parts row 2: Amount must be a whole number (was 'x').", status.Problem);
    }

    // A harmful damage tick of 1 HP at the end of the turn: only the id and name are left to type.
    [Fact]
    public void New_IdAndNameTyped_IsAHarmfulTickAtTheTurnsEnd()
    {
        var status = StatusViewModel.New([]);

        status.IdText = "bleed_1";
        status.Name = "Bleed";

        var expected = new StatusDefinition
        {
            Id = "bleed_1",
            Name = "Bleed",
            Description = "",
            IsHarmful = true,
            Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 1)],
        };
        Assert.Equal(expected, status.ToStatus());
        Assert.True(status.IsNew);
    }

    // Copied from the boxes: reading the half-typed amount would throw.
    [Fact]
    public void DuplicateOf_HalfTypedAmount_CopiesTheBoxesUnderTheNextId()
    {
        var chosen = StatusViewModel.From(_burning, [SkillFixtures.Bolt]);
        chosen.Parts[0].AmountText = "-";

        var copy = StatusViewModel.DuplicateOf(chosen, "burning_2", ["burning_1"]);

        Assert.Equal(
            ("burning_2", "Burning", "burning", StatusTiming.StartOfTurn, "B"),
            (copy.IdText, copy.Name, copy.FamilyText, copy.ActsAt, copy.MarkLetterText));
        Assert.Equal(("-", (DamageType?)DamageType.Fire), (copy.Parts[0].AmountText, copy.Parts[0].DamageType));
        Assert.NotSame(chosen.Parts[0], copy.Parts[0]);
        Assert.True(copy.IsNew);
    }

    [Fact]
    public void Changed_AmountTyped_Raised()
    {
        var status = StatusViewModel.From(_burning, []);
        var raised = 0;
        status.Changed += (_, _) => raised++;

        status.Parts[0].AmountText = "9";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_PartAdded_Raised()
    {
        var status = StatusViewModel.From(_burning, []);
        var raised = 0;
        status.Changed += (_, _) => raised++;

        status.AddPartCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_RemovedPartTyped_NotRaised()
    {
        var status = StatusViewModel.From(_burning, []);
        var removed = status.Parts[0];
        status.RemovePartCommand.Execute(null);
        var raised = 0;
        status.Changed += (_, _) => raised++;

        removed.AmountText = "9";

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Changed_FamilyTyped_Raised()
    {
        var status = StatusViewModel.From(_burning, []);
        var raised = 0;
        status.Changed += (_, _) => raised++;

        status.FamilyText = "fire";

        Assert.Equal(1, raised);
    }
}
