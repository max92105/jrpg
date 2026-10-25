using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Events;

namespace Jrpg.Editor.Events;

/// <summary>One event in the event dialog: when it runs, and its steps in order. Its id never changes.</summary>
internal sealed partial class EventViewModel : ObservableObject
{
    public EventViewModel(MapEvent mapEvent)
    {
        Id = mapEvent.Id;
        Activation = mapEvent.Activation;
        Prompt = mapEvent.Prompt ?? "";
        IsOnce = mapEvent.IsOnce;
        OnlyWhenObjectId = mapEvent.OnlyWhenObjectId ?? "";
        OnlyWhenLeaderId = mapEvent.OnlyWhenLeaderId ?? "";
        Steps = [.. mapEvent.Steps.Select(EventStepViewModel.From)];
        Steps.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Label));
    }

    public string Id { get; }

    public ObservableCollection<EventStepViewModel> Steps { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(CanHavePrompt))]
    public partial EventActivation Activation { get; set; }

    /// <summary>The hint shown when the player can interact, such as "Open".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Prompt { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial bool IsOnce { get; set; }

    /// <summary>The object the event's object must be for it to run, such as a closed door; empty for always.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string OnlyWhenObjectId { get; set; }

    /// <summary>The member who must lead for the event to run, such as an intro's character; empty for anyone.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string OnlyWhenLeaderId { get; set; }

    /// <summary>False for an arrival event: nothing is pressed to start it, so the game refuses a prompt.</summary>
    public bool CanHavePrompt => Activation != EventActivation.Arrive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveStepCommand), nameof(MoveStepUpCommand), nameof(MoveStepDownCommand))]
    public partial EventStepViewModel? SelectedStep { get; set; }

    /// <summary>The one-line summary, such as <c>Press "Open" if door_close: 1 step(s), once</c> or <c>On arrival while omer leads: 4 step(s), once</c>.</summary>
    public string Label
    {
        get
        {
            var when = Activation switch
            {
                EventActivation.Touch => "Walk into",
                EventActivation.Arrive => "On arrival",
                _ => $"Press \"{Prompt}\"",
            };
            var objectCondition = OnlyWhenObjectId.Length > 0 ? $" if {OnlyWhenObjectId}" : "";
            var leaderCondition = OnlyWhenLeaderId.Length > 0 ? $" while {OnlyWhenLeaderId} leads" : "";
            return $"{when}{objectCondition}{leaderCondition}: {Steps.Count} step(s){(IsOnce ? ", once" : "")}";
        }
    }

    public MapEvent ToEvent() => new()
    {
        Id = Id,
        Activation = Activation,
        Prompt = !CanHavePrompt || string.IsNullOrWhiteSpace(Prompt) ? null : Prompt.Trim(),
        IsOnce = IsOnce,
        OnlyWhenObjectId = OnlyWhenObjectId.Length > 0 ? OnlyWhenObjectId : null,
        OnlyWhenLeaderId = OnlyWhenLeaderId.Length > 0 ? OnlyWhenLeaderId : null,
        Steps = [.. Steps.Select(step => step.ToStep())],
    };

    /// <summary>Why a step can't be kept as typed, naming it; null when every step can.</summary>
    public string? Problem =>
        Steps.Select((step, index) => step.Problem is { } problem ? $"step {index + 1}: {problem}" : null).FirstOrDefault(problem => problem is not null);

    [RelayCommand]
    private void AddDialogue() => AddStep(EventStepKind.Dialogue);

    [RelayCommand]
    private void AddGiveItem() => AddStep(EventStepKind.GiveItem);

    [RelayCommand]
    private void AddSwapObject() => AddStep(EventStepKind.SwapObject);

    [RelayCommand]
    private void AddWarp() => AddStep(EventStepKind.Warp);

    [RelayCommand]
    private void AddStill() => AddStep(EventStepKind.Still);

    [RelayCommand]
    private void AddRecruit() => AddStep(EventStepKind.Recruit);

    [RelayCommand]
    private void AddVoice() => AddStep(EventStepKind.Voice);

    [RelayCommand]
    private void AddRest() => AddStep(EventStepKind.Rest);

    // After the chosen step, or last when none is chosen.
    private void AddStep(EventStepKind kind)
    {
        var step = EventStepViewModel.New(kind);
        Steps.Insert(SelectedStep is null ? Steps.Count : Steps.IndexOf(SelectedStep) + 1, step);
        SelectedStep = step;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void RemoveStep() => SelectedStep = ListOrder.Remove(Steps, SelectedStep!);

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void MoveStepUp() => ListOrder.MoveBy(Steps, SelectedStep!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void MoveStepDown() => ListOrder.MoveBy(Steps, SelectedStep!, 1);

    private bool HasSelectedStep() => SelectedStep is not null;
}
