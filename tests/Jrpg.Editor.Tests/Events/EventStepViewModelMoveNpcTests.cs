using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// A move step (ADR 0044) picks its NPC and builds its path with the direction buttons (decision 579).
public sealed class EventStepViewModelMoveNpcTests
{
    [Fact]
    public void From_MoveNpcStep_ReadsUidAndPath()
    {
        var step = EventStepViewModel.From(new MoveNpcStep("npc_3", [Facing.Left, Facing.Left, Facing.Down]));

        Assert.True(step.IsMoveNpc);
        Assert.Equal("npc_3", step.MoveNpcUid);
        Assert.Equal([Facing.Left, Facing.Left, Facing.Down], step.MovePath);
        Assert.Equal("Move npc_3: left, left, down", step.Label);
        Assert.Null(step.Problem);
    }

    [Fact]
    public void AddLeft_Twice_PathTextReadsLeftLeft()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);

        step.AddLeftCommand.Execute(null);
        step.AddLeftCommand.Execute(null);

        Assert.Equal("left, left", step.MovePathText);
    }

    [Fact]
    public void RemoveLastMoveStep_EmptyPath_IsGreyed()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);

        Assert.False(step.RemoveLastMoveStepCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveLastMoveStep_TwoSteps_DropsTheLast()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        step.AddUpCommand.Execute(null);
        step.AddRightCommand.Execute(null);

        step.RemoveLastMoveStepCommand.Execute(null);

        Assert.Equal([Facing.Up], step.MovePath);
    }

    // The content's bound (decision 578): the 65th step is greyed, the 64th is not.
    [Fact]
    public void AddUp_AtMaxSteps_IsGreyed()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        for (var count = 0; count < MoveNpcStep.MaxSteps - 1; count++)
        {
            step.AddDownCommand.Execute(null);
        }

        Assert.True(step.AddUpCommand.CanExecute(null));
        step.AddUpCommand.Execute(null);

        Assert.False(step.AddUpCommand.CanExecute(null));
        Assert.Equal(MoveNpcStep.MaxSteps, step.MovePath.Count);
    }

    [Fact]
    public void Problem_NoNpcChosen_AsksForOne()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        step.AddUpCommand.Execute(null);

        Assert.Equal("Choose the NPC to move.", step.Problem);
    }

    [Fact]
    public void Problem_EmptyPath_AsksForAStep()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        step.MoveNpcUid = "npc_3";

        Assert.Equal("Add at least one step to the path.", step.Problem);
        Assert.Throws<InvalidOperationException>(step.ToStep);
    }

    [Fact]
    public void ToStep_NpcAndPath_GivesTheMoveStep()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        step.MoveNpcUid = "npc_3";
        step.AddLeftCommand.Execute(null);
        step.AddDownCommand.Execute(null);

        Assert.Equal(new MoveNpcStep("npc_3", [Facing.Left, Facing.Down]), step.ToStep());
    }

    // The steps list and the problem line read the path through these, so a button press must raise them.
    [Fact]
    public void AddRight_PathChanged_RaisesLabelPathTextAndProblem()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        var raised = new List<string?>();
        step.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        step.AddRightCommand.Execute(null);

        Assert.Contains(nameof(EventStepViewModel.Label), raised);
        Assert.Contains(nameof(EventStepViewModel.MovePathText), raised);
        Assert.Contains(nameof(EventStepViewModel.Problem), raised);
    }

    [Fact]
    public void AddUp_FirstStep_LightsRemoveLast()
    {
        var step = EventStepViewModel.New(EventStepKind.MoveNpc);
        var lit = false;
        step.RemoveLastMoveStepCommand.CanExecuteChanged += (_, _) => lit = step.RemoveLastMoveStepCommand.CanExecute(null);

        step.AddUpCommand.Execute(null);

        Assert.True(lit);
    }
}
