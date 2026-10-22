using Jrpg.Core.Events;
using Jrpg.Core.Items;

namespace Jrpg.Core.Exploration;

/// <summary>
/// One running map event, as a cursor over its steps (ADR 0006). Steps the player doesn't need to see apply at once;
/// dialogue, a received item and a warp stay <see cref="Current"/> until the Game calls <see cref="Continue"/>.
/// </summary>
internal sealed class EventRun
{
    private readonly TriggeredEvent _triggered;
    private readonly ActiveMap _map;
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private int _index = -1;

    /// <summary>Starts the event: marks a "once" event done first, then runs up to the first step that waits.</summary>
    public EventRun(TriggeredEvent triggered, ActiveMap map, Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        _triggered = triggered;
        _map = map;
        _inventory = inventory;
        _items = items;

        // Marked before any step, so a warp in the middle can't make it run twice.
        if (triggered.Event.IsOnce)
        {
            map.MarkDone(triggered.Event);
        }

        AdvanceToNextWaitingStep();
    }

    private IReadOnlyList<EventStep> Steps => _triggered.Event.Steps;

    /// <summary>The step waiting for the player or the Game, or null once the event is over.</summary>
    public EventStep? Current => IsFinished ? null : Steps[_index];

    /// <summary>What the current give-item step added and lost; null on any other step.</summary>
    public ItemReceipt? CurrentReceipt { get; private set; }

    public bool IsFinished => _index >= Steps.Count;

    /// <summary>The player read the current step, or the Game finished its warp fade-out.</summary>
    /// <exception cref="InvalidOperationException">The event is already over.</exception>
    public void Continue()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException($"Event '{_triggered.Event.Id}' is already over.");
        }

        AdvanceToNextWaitingStep();
    }

    private void AdvanceToNextWaitingStep()
    {
        CurrentReceipt = null;
        for (_index++; _index < Steps.Count; _index++)
        {
            if (ApplyAndCheckWait(Steps[_index]))
            {
                return;
            }
        }
    }

    private bool ApplyAndCheckWait(EventStep step)
    {
        switch (step)
        {
            case SwapObjectStep swap:
                // Content validation only allows swaps in object events (slice 3b).
                var source = _triggered.Source
                    ?? throw new InvalidOperationException($"Event '{_triggered.Event.Id}' swaps an object but has none.");
                _map.SwapObject(source, swap.ObjectId);
                return false;
            case GiveItemStep give:
                var item = _items[give.ItemId];
                var lost = _inventory.Add(item, give.Quantity);
                CurrentReceipt = new ItemReceipt(item, give.Quantity - lost, lost);
                return true;
            case DialogueStep or WarpStep:
                return true;
            default:
                throw new InvalidOperationException($"Step {step.GetType().Name} has no rule in EventRun.");
        }
    }
}
