using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Events;

/// <summary>
/// A move step's NPC, picked among the map's placed NPCs, and its path, built one cell at a time with the direction
/// buttons and shown as text (decision 579).
/// </summary>
internal sealed partial class EventStepViewModel
{
    /// <summary>The placed NPC the step walks, by uid; empty until picked.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string MoveNpcUid { get; set; } = "";

    /// <summary>The path, one facing per cell walked, in order.</summary>
    public ObservableCollection<Facing> MovePath { get; } = [];

    /// <summary>The path as the content spells it: "left, left, down".</summary>
    public string MovePathText => string.Join(", ", MovePath.Select(facing => facing.ToString().ToLowerInvariant()));

    [RelayCommand(CanExecute = nameof(CanAddMoveStep))]
    private void AddUp() => MovePath.Add(Facing.Up);

    [RelayCommand(CanExecute = nameof(CanAddMoveStep))]
    private void AddDown() => MovePath.Add(Facing.Down);

    [RelayCommand(CanExecute = nameof(CanAddMoveStep))]
    private void AddLeft() => MovePath.Add(Facing.Left);

    [RelayCommand(CanExecute = nameof(CanAddMoveStep))]
    private void AddRight() => MovePath.Add(Facing.Right);

    [RelayCommand(CanExecute = nameof(HasMoveSteps))]
    private void RemoveLastMoveStep() => MovePath.RemoveAt(MovePath.Count - 1);

    // The content's bound (decision 578), greyed here rather than refused on save.
    private bool CanAddMoveStep() => MovePath.Count < MoveNpcStep.MaxSteps;

    private bool HasMoveSteps() => MovePath.Count > 0;

    private string MoveNpcLabel() => $"Move {MoveNpcUid}: {MovePathText}";

    private string? MoveNpcProblem =>
        string.IsNullOrEmpty(MoveNpcUid) ? "Choose the NPC to move."
        : MovePath.Count == 0 ? "Add at least one step to the path."
        : null;

    private MoveNpcStep ToMoveNpcStep() =>
        MoveNpcProblem is null ? new MoveNpcStep(MoveNpcUid, [.. MovePath]) : throw InvalidFields();

    private static EventStepViewModel FromMove(MoveNpcStep move)
    {
        var step = new EventStepViewModel(EventStepKind.MoveNpc) { MoveNpcUid = move.ObjectUid };
        foreach (var facing in move.Path)
        {
            step.MovePath.Add(facing);
        }

        return step;
    }

    // The path's changes show in the line and the problem, and grey the buttons at the bounds.
    private void WatchMovePath() => MovePath.CollectionChanged += (_, _) =>
    {
        OnPropertyChanged(nameof(MovePathText));
        OnPropertyChanged(nameof(Label));
        OnPropertyChanged(nameof(Problem));
        AddUpCommand.NotifyCanExecuteChanged();
        AddDownCommand.NotifyCanExecuteChanged();
        AddLeftCommand.NotifyCanExecuteChanged();
        AddRightCommand.NotifyCanExecuteChanged();
        RemoveLastMoveStepCommand.NotifyCanExecuteChanged();
    };
}
