using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>
/// The event dialog for one placed object: its events in order, edited apart from the map until OK. New events get the
/// next id the object hasn't used (<see cref="EventIds"/>) and start like the legacy editor's: pressed, prompt "Check".
/// </summary>
internal sealed partial class EventListViewModel : ObservableObject
{
    private const string NewEventPrompt = "Check";

    private readonly PlacedObject _placed;

    /// <param name="placed">The object whose events are edited.</param>
    /// <param name="objectIds">Every object id, for "only while the object is".</param>
    public EventListViewModel(PlacedObject placed, IEnumerable<string> objectIds)
    {
        _placed = placed;
        Events = [.. placed.Events.Select(mapEvent => new EventViewModel(mapEvent))];
        OnlyWhenChoices = ["", .. objectIds.Order(StringComparer.Ordinal)];
        SelectedEvent = Events.Count > 0 ? Events[0] : null;
    }

    public string Title => $"Events of {_placed.Uid} ({_placed.ObjectId})";

    public ObservableCollection<EventViewModel> Events { get; }

    public IReadOnlyList<EventActivation> Activations { get; } = Enum.GetValues<EventActivation>();

    /// <summary>The object ids an event can be limited to; the empty one means always.</summary>
    public IReadOnlyList<string> OnlyWhenChoices { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveEventCommand), nameof(MoveEventUpCommand), nameof(MoveEventDownCommand))]
    public partial EventViewModel? SelectedEvent { get; set; }

    /// <summary>The edit that gives the object these events; null when they are the ones it has.</summary>
    public SetObjectEventsEdit? ToEdit()
    {
        IReadOnlyList<MapEvent> events = [.. Events.Select(mapEvent => mapEvent.ToEvent())];
        return MapEventComparison.AreSame(events, _placed.Events) ? null : new SetObjectEventsEdit(_placed.Uid, _placed.Events, events);
    }

    [RelayCommand]
    private void AddEvent()
    {
        // The object's events as the dialog opened count too, so an event deleted here doesn't hand its id on.
        var usedIds = Events.Select(mapEvent => mapEvent.Id).Concat(_placed.Events.Select(mapEvent => mapEvent.Id));
        var added = new EventViewModel(new MapEvent
        {
            Id = EventIds.Next(_placed.Uid, usedIds),
            Activation = EventActivation.Interact,
            Prompt = NewEventPrompt,
            Steps = [],
        });
        Events.Add(added);
        SelectedEvent = added;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedEvent))]
    private void RemoveEvent() => SelectedEvent = ListOrder.Remove(Events, SelectedEvent!);

    [RelayCommand(CanExecute = nameof(HasSelectedEvent))]
    private void MoveEventUp() => ListOrder.MoveBy(Events, SelectedEvent!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedEvent))]
    private void MoveEventDown() => ListOrder.MoveBy(Events, SelectedEvent!, 1);

    private bool HasSelectedEvent() => SelectedEvent is not null;
}
