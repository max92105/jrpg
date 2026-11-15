using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Events;

namespace Jrpg.Editor.Events;

/// <summary>
/// One event in the event dialog: when it runs (its activation, the object, leader and quest gates), and its steps in
/// order. Its id never changes.
/// </summary>
internal sealed partial class EventViewModel : ObservableObject
{
    /// <param name="mapEvent">The event as it is.</param>
    /// <param name="choices">The quests and stages the quest gate can pick from.</param>
    public EventViewModel(MapEvent mapEvent, StepChoices choices)
    {
        Id = mapEvent.Id;
        Activation = mapEvent.Activation;
        Prompt = mapEvent.Prompt ?? "";
        IsOnce = mapEvent.IsOnce;
        OnlyWhenObjectId = mapEvent.OnlyWhenObjectId ?? "";
        OnlyWhenLeaderId = mapEvent.OnlyWhenLeaderId ?? "";
        QuestGate = new QuestGateViewModel(mapEvent.OnlyWhenQuest, choices.QuestStages);
        QuestGate.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(ConditionSummary));
        };
        Steps = [.. mapEvent.Steps.Select(EventStepViewModel.From)];
        Steps.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Label));
        AddStepChoices = StepKindMenu.Choices(AddStepCommand);
    }

    public string Id { get; }

    /// <summary>
    /// False for a cell's event, which has no prompt and no object to wait for, and can't swap an object (the game
    /// refuses it): the dialogs' shared view hides those rows on it, and Swap object can't be added.
    /// </summary>
    public bool IsObjectEvent { get; init; } = true;

    public ObservableCollection<EventStepViewModel> Steps { get; }

    /// <summary>The Add step drop-down's entries: every step kind under its group's title (decision 729).</summary>
    public IReadOnlyList<DropDownChoice> AddStepChoices { get; }

    /// <summary>The quest the event waits on, if any (ADR 0033).</summary>
    public QuestGateViewModel QuestGate { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(ConditionSummary), nameof(CanHavePrompt))]
    public partial EventActivation Activation { get; set; }

    /// <summary>The hint shown when the player can interact, such as "Open".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(ConditionSummary))]
    public partial string Prompt { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(ConditionSummary))]
    public partial bool IsOnce { get; set; }

    /// <summary>The object the event's object must be for it to run, such as a closed door; empty for always.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(ConditionSummary))]
    public partial string OnlyWhenObjectId { get; set; }

    /// <summary>The member who must lead for the event to run, such as an intro's character; empty for anyone.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(ConditionSummary))]
    public partial string OnlyWhenLeaderId { get; set; }

    /// <summary>False for an arrival event: nothing is pressed to start it, so the game refuses a prompt.</summary>
    public bool CanHavePrompt => Activation != EventActivation.Arrive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveStepCommand), nameof(MoveStepUpCommand), nameof(MoveStepDownCommand))]
    public partial EventStepViewModel? SelectedStep { get; set; }

    /// <summary>The one-line summary, such as <c>Press "Open" if door_close: 1 step(s), once</c> or <c>On arrival while omer leads: 4 step(s), once</c>.</summary>
    public string Label => $"{ConditionWords()}: {Steps.Count} step(s){OnceWords}";

    /// <summary>
    /// The folded conditions' line (decision 732): <see cref="Label"/> without the step count, such as <c>Press "Open"
    /// if door_close when errand is active, once</c>.
    /// </summary>
    public string ConditionSummary => $"{ConditionWords()}{OnceWords}";

    private string OnceWords => IsOnce ? ", once" : "";

    public MapEvent ToEvent() => new()
    {
        Id = Id,
        Activation = Activation,
        Prompt = !CanHavePrompt || string.IsNullOrWhiteSpace(Prompt) ? null : Prompt.Trim(),
        IsOnce = IsOnce,
        OnlyWhenObjectId = OnlyWhenObjectId.Length > 0 ? OnlyWhenObjectId : null,
        OnlyWhenLeaderId = OnlyWhenLeaderId.Length > 0 ? OnlyWhenLeaderId : null,
        OnlyWhenQuest = QuestGate.ToCondition(),
        Steps = [.. Steps.Select(step => step.ToStep())],
    };

    /// <summary>Why the gate or a step can't be kept as typed, naming the step; null when all can.</summary>
    public string? Problem =>
        QuestGate.Problem ?? Steps.Select((step, index) => step.Problem is { } problem ? $"step {index + 1}: {problem}" : null).FirstOrDefault(problem => problem is not null);

    // What starts the event, then the object, leader and quest it waits for.
    private string ConditionWords()
    {
        var when = Activation switch
        {
            EventActivation.Touch => "Walk into",
            EventActivation.Arrive => "On arrival",
            _ => $"Press \"{Prompt}\"",
        };
        var objectCondition = OnlyWhenObjectId.Length > 0 ? $" if {OnlyWhenObjectId}" : "";
        var leaderCondition = OnlyWhenLeaderId.Length > 0 ? $" while {OnlyWhenLeaderId} leads" : "";
        return $"{when}{objectCondition}{leaderCondition}{QuestGate.Label}";
    }

    // After the chosen step, or last when none is chosen.
    [RelayCommand(CanExecute = nameof(CanAddStep))]
    private void AddStep(EventStepKind kind)
    {
        var step = EventStepViewModel.New(kind);
        Steps.Insert(SelectedStep is null ? Steps.Count : Steps.IndexOf(SelectedStep) + 1, step);
        SelectedStep = step;
    }

    // The game only swaps an object from that object's own event (decision 728).
    private bool CanAddStep(EventStepKind kind) => kind != EventStepKind.SwapObject || IsObjectEvent;

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void RemoveStep() => SelectedStep = ListOrder.Remove(Steps, SelectedStep!);

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void MoveStepUp() => ListOrder.MoveBy(Steps, SelectedStep!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void MoveStepDown() => ListOrder.MoveBy(Steps, SelectedStep!, 1);

    private bool HasSelectedStep() => SelectedStep is not null;
}
