using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Quests;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Quests;

/// <summary>
/// One quest as the Quests window edits it (decision 394): its id (typed while the quest is new, fixed once the draft
/// holds it), title, description, kind and its stages in order. A stage is added after the chosen one; only a stage
/// added this session can be removed, since a saved one may be named by an event or a save. <see cref="Problem"/> names
/// the first thing the game would refuse.
/// </summary>
internal sealed partial class QuestViewModel : ObservableObject
{
    private QuestViewModel(string id, IEnumerable<QuestStageViewModel> stages)
    {
        Id = id;
        Stages = [.. stages];
        foreach (var stage in Stages)
        {
            stage.PropertyChanged += OnStageChanged;
        }

        Stages.CollectionChanged += OnStagesChanged;
        SelectedStage = Stages.FirstOrDefault();
    }

    public static QuestViewModel From(QuestDefinition quest) =>
        new(quest.Id, quest.Stages.Select(QuestStageViewModel.From)) { Title = quest.Title, Description = quest.Description, Kind = quest.Kind };

    /// <summary>A quest with nothing typed yet; a side quest until the kind is picked (placeholder default).</summary>
    public static QuestViewModel New() => new("", []) { IsNew = true, Kind = QuestKind.Side };

    /// <summary>True while the quest was added this session: the only time its id can be typed.</summary>
    public bool IsNew { get; private init; }

    public ObservableCollection<QuestStageViewModel> Stages { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial QuestKind Kind { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveStageCommand), nameof(MoveStageUpCommand), nameof(MoveStageDownCommand))]
    public partial QuestStageViewModel? SelectedStage { get; set; }

    /// <summary>
    /// The list's line: "placeholder_errand: Placeholder errand", or a placeholder while a new quest has no id.
    /// </summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Title}" : "(new quest)";

    /// <summary>
    /// Why the game would refuse the quest as typed, a stage's problem naming the stage by position; null when it
    /// wouldn't. Whether the id is another quest's is the list's problem, which sees them all.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, []) : null)
        ?? (string.IsNullOrWhiteSpace(Title) ? "Write the quest's title." : null)
        ?? (string.IsNullOrWhiteSpace(Description) ? "Write the quest's description." : null)
        ?? (Stages.Count == 0 ? "Add at least one stage." : null)
        ?? FirstStageProblem();

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the window applies only without one.
    /// </exception>
    public QuestDefinition ToQuest() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new QuestDefinition(Id, Title.Trim(), Description.Trim(), Kind, [.. Stages.Select(stage => stage.ToStage())]);

    [RelayCommand]
    private void AddStage()
    {
        var stage = QuestStageViewModel.New();
        Stages.Insert(SelectedStage is null ? Stages.Count : Stages.IndexOf(SelectedStage) + 1, stage);
        SelectedStage = stage;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveStage))]
    private void RemoveStage() => SelectedStage = ListOrder.Remove(Stages, SelectedStage!);

    private bool CanRemoveStage() => SelectedStage is { IsNew: true };

    [RelayCommand(CanExecute = nameof(HasSelectedStage))]
    private void MoveStageUp() => ListOrder.MoveBy(Stages, SelectedStage!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedStage))]
    private void MoveStageDown() => ListOrder.MoveBy(Stages, SelectedStage!, 1);

    private bool HasSelectedStage() => SelectedStage is not null;

    // A stage added or removed changes the problem and, from then on, is watched or not for its own problems.
    private void OnStagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var stage in e.OldItems?.OfType<QuestStageViewModel>() ?? [])
        {
            stage.PropertyChanged -= OnStageChanged;
        }

        foreach (var stage in e.NewItems?.OfType<QuestStageViewModel>() ?? [])
        {
            stage.PropertyChanged += OnStageChanged;
        }

        OnPropertyChanged(nameof(Problem));
    }

    private void OnStageChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Problem));

    // A stage repeating an earlier stage's id is reported on the later one, so the first stays as it was.
    private string? FirstStageProblem()
    {
        for (var index = 0; index < Stages.Count; index++)
        {
            var stage = Stages[index];
            if (Stages.Take(index).Any(earlier => earlier.Id == stage.Id))
            {
                return $"Stage {index + 1} repeats the id {stage.Id}.";
            }

            if (stage.Problem is { } problem)
            {
                return $"Stage {index + 1}: {problem}";
            }
        }

        return null;
    }
}
