using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>
/// The cell event dialog: the one event a cell runs when the player walks onto it, such as a warp at a map's edge.
/// A new one gets the content's id <c>cell_{x}_{y}</c>; it is always walked into, and offers dialogue and warp steps
/// (the game refuses swapping an object from a cell).
/// </summary>
internal sealed class CellEventViewModel
{
    private readonly GridCell _cell;
    private readonly CellEvent? _existing;

    /// <param name="cell">The cell edited.</param>
    /// <param name="existing">Its event as it is; null when it has none yet.</param>
    /// <param name="choices">The ids the steps can pick from.</param>
    public CellEventViewModel(GridCell cell, CellEvent? existing, StepChoices choices)
    {
        _cell = cell;
        _existing = existing;
        Choices = choices;
        Event = new EventViewModel(existing?.Event ?? new MapEvent
        {
            Id = $"cell_{cell.X}_{cell.Y}",
            Activation = EventActivation.Touch,
            Steps = [],
        });
    }

    public string Title => $"Event of cell ({_cell.X}, {_cell.Y})";

    public StepChoices Choices { get; }

    public EventViewModel Event { get; }

    /// <summary>Why the event can't be kept as typed; null when it can.</summary>
    public string? Problem => Event.Steps.Count == 0 ? "Add at least one step." : Event.Problem;

    /// <summary>The edit that gives the cell this event, once <see cref="Problem"/> is null; null when it is the one the cell has.</summary>
    /// <param name="cellEvents">The map's cell events as they are.</param>
    public SetCellEventEdit? ToEdit(IReadOnlyList<CellEvent> cellEvents)
    {
        var edited = Event.ToEvent();
        var isUnchanged = _existing is not null && MapEventComparison.AreSame([edited], [_existing.Event]);
        return isUnchanged ? null : SetCellEventEdit.For(cellEvents, _cell, edited);
    }
}
