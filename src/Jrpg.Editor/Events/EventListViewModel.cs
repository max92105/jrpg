using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>
/// The event dialog for one placed object: its events in order, edited apart from the map until OK. New events get the
/// next id the object hasn't used (<see cref="EventIds"/>) and start pressed, with the prompt "Check" (placeholders).
/// </summary>
internal sealed partial class EventListViewModel : ObservableObject
{
    private const string NewEventPrompt = "Check";

    private readonly PlacedObject _placed;
    private readonly string _likelyOpenedObjectId;

    /// <param name="placed">The object whose events are edited.</param>
    /// <param name="choices">The ids the steps and "only while the object is" can pick from.</param>
    /// <param name="likelyOpenedObjectId">What the chest and door presets turn the object into (<see cref="EventPresets.GuessOpenedObject"/>).</param>
    public EventListViewModel(PlacedObject placed, StepChoices choices, string likelyOpenedObjectId)
    {
        _placed = placed;
        _likelyOpenedObjectId = likelyOpenedObjectId;
        Events = [.. placed.Events.Select(mapEvent => new EventViewModel(mapEvent))];
        Choices = choices;
        OnlyWhenChoices = ["", .. choices.ObjectIds];
        SelectedEvent = Events.Count > 0 ? Events[0] : null;
    }

    public string Title => $"Events of {_placed.Uid} ({_placed.ObjectId})";

    public ObservableCollection<EventViewModel> Events { get; }

    public StepChoices Choices { get; }

    public IReadOnlyList<EventActivation> Activations { get; } = Enum.GetValues<EventActivation>();

    /// <summary>The object ids an event can be limited to; the empty one means always.</summary>
    public IReadOnlyList<string> OnlyWhenChoices { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveEventCommand), nameof(MoveEventUpCommand), nameof(MoveEventDownCommand))]
    public partial EventViewModel? SelectedEvent { get; set; }

    /// <summary>Why the events can't be kept as typed, naming the event and step; null when they can.</summary>
    public string? Problem =>
        Events.Select(item => item.Problem is { } problem ? $"Event {item.Id}, {problem}" : null).FirstOrDefault(problem => problem is not null);

    /// <summary>The edit that gives the object these events, once <see cref="Problem"/> is null; null when they are the ones it has.</summary>
    public SetObjectEventsEdit? ToEdit()
    {
        IReadOnlyList<MapEvent> events = [.. Events.Select(mapEvent => mapEvent.ToEvent())];
        return MapEventComparison.AreSame(events, _placed.Events) ? null : new SetObjectEventsEdit(_placed.Uid, _placed.Events, events);
    }

    [RelayCommand]
    private void AddEvent()
    {
        var added = new EventViewModel(new MapEvent
        {
            Id = IdAllocation().NewEventIds(1)[0],
            Activation = EventActivation.Interact,
            Prompt = NewEventPrompt,
            Steps = [],
        });
        Events.Add(added);
        SelectedEvent = added;
    }

    [RelayCommand]
    private void ApplyChestPreset() => ApplyPreset(EventPresets.Chest(IdAllocation(), _likelyOpenedObjectId));

    [RelayCommand]
    private void ApplyDoorPreset() => ApplyPreset(EventPresets.Door(IdAllocation()));

    [RelayCommand]
    private void ApplyDoorYouOpenPreset() => ApplyPreset(EventPresets.DoorYouOpen(IdAllocation(), _likelyOpenedObjectId));

    [RelayCommand]
    private void ApplySignPreset() => ApplyPreset(EventPresets.Sign(IdAllocation()));

    [RelayCommand]
    private void ApplyTalkPreset() => ApplyPreset(EventPresets.Talk(IdAllocation()));

    // Replaces every event, and shows the first field the preset leaves to fill in.
    private void ApplyPreset(IReadOnlyList<MapEvent> presetEvents)
    {
        Events.Clear();
        foreach (var mapEvent in presetEvents)
        {
            Events.Add(new EventViewModel(mapEvent));
        }

        SelectedEvent = Events.FirstOrDefault(item => item.Problem is not null) ?? Events[0];
        SelectedEvent.SelectedStep = SelectedEvent.Steps.FirstOrDefault(step => step.Problem is not null) ?? SelectedEvent.Steps.FirstOrDefault();
    }

    // The object's events as the dialog opened count too, so an event replaced or deleted here doesn't hand its id on.
    private EventIdAllocation IdAllocation() =>
        new(_placed.Uid, _placed.ObjectId, [.. Events.Select(mapEvent => mapEvent.Id).Concat(_placed.Events.Select(mapEvent => mapEvent.Id))]);

    [RelayCommand(CanExecute = nameof(HasSelectedEvent))]
    private void RemoveEvent() => SelectedEvent = ListOrder.Remove(Events, SelectedEvent!);

    [RelayCommand(CanExecute = nameof(HasSelectedEvent))]
    private void MoveEventUp() => ListOrder.MoveBy(Events, SelectedEvent!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedEvent))]
    private void MoveEventDown() => ListOrder.MoveBy(Events, SelectedEvent!, 1);

    private bool HasSelectedEvent() => SelectedEvent is not null;
}
