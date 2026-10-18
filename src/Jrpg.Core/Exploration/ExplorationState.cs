using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>
/// The player on the current map and the map event in progress. The Game feeds it input each frame: walking, interact
/// presses, and "continue" once the player has read a step (ADR 0006).
/// </summary>
public sealed class ExplorationState
{
    private readonly ExplorationContext _context;
    private readonly ActiveMap _map;
    private readonly PlayerWalker _walker;
    private readonly MapCollision _collision;
    private readonly InteractionFinder _finder;
    private GridCell _lastCell;
    private bool _isTouchArmed;
    private EventRun? _run;

    internal ExplorationState(ExplorationContext context, MapLocation start)
    {
        _context = context;
        var catalog = context.Catalog;
        var definition = catalog.Maps[start.MapId];
        _map = new ActiveMap(definition, catalog.Objects, WalkabilityGrid.Build(definition.Layout, catalog.Tiles), context.World);
        _walker = new PlayerWalker(start.Position, start.Facing, catalog.Rules.Exploration);
        _collision = new MapCollision(_map);
        _finder = new InteractionFinder(_map, catalog.Rules.Exploration);

        // The arrival cell stays quiet, and object touch events wait until the player has stood still (decision 38).
        _lastCell = _walker.Cell;
    }

    public MapLocation Location => new(_map.Definition.Id, _walker.Position, _walker.Facing);

    /// <summary>The event step the player is reading, or null when no event runs.</summary>
    public EventStep? CurrentEventStep => _run?.Current;

    public bool IsEventRunning => _run is not null;

    /// <summary>
    /// Moves the player. Entering a new cell starts its touch event, once per entry. Otherwise, while moving, a touch
    /// event on the object in front starts, if the player has stood still since entering the map or since the last cell or
    /// object touch event (decision 38).
    /// </summary>
    /// <exception cref="InvalidOperationException">An event is running; the player can't walk during it.</exception>
    public void Walk(MoveIntent intent, TimeSpan delta)
    {
        ThrowIfEventRunning();
        _walker.Walk(intent, delta, _collision);
        if (!_walker.IsMoving)
        {
            _isTouchArmed = true;
            return;
        }

        if (_walker.Cell != _lastCell)
        {
            _lastCell = _walker.Cell;
            if (TryStartCellEvent(_lastCell))
            {
                return;
            }
        }

        if (_isTouchArmed && _finder.FindInFront(_walker.Position, _walker.Facing, EventActivation.Touch) is { } touched)
        {
            _isTouchArmed = false;
            Start(touched);
        }
    }

    /// <summary>Starts the interact event of the object the player faces, as the confirm button does.</summary>
    /// <returns>False when nothing in front can be interacted with.</returns>
    /// <exception cref="InvalidOperationException">An event is already running.</exception>
    public bool TryInteract()
    {
        ThrowIfEventRunning();
        var triggered = _finder.FindInFront(_walker.Position, _walker.Facing, EventActivation.Interact);
        if (triggered is null)
        {
            return false;
        }

        Start(triggered);
        return true;
    }

    /// <summary>The player has read the current step; the event moves on to its next waiting step or ends.</summary>
    /// <exception cref="InvalidOperationException">No event is running.</exception>
    /// <exception cref="NotSupportedException">The current step is a warp, which arrives in slice 4a.</exception>
    public void ContinueEvent()
    {
        var run = _run ?? throw new InvalidOperationException("No event is running.");

        // TODO(plan 2026-10-17-world-exploration-core, slice 4a): complete the warp by entering the target map.
        if (run.Current is WarpStep)
        {
            throw new NotSupportedException("Warps are completed in slice 4a.");
        }

        run.Continue();
        if (run.IsFinished)
        {
            _run = null;
        }
    }

    private bool TryStartCellEvent(GridCell cell)
    {
        var cellEvent = _map.CellEventAt(cell);
        if (cellEvent is null || (cellEvent.Event.IsOnce && _map.IsDone(cellEvent.Event)))
        {
            return false;
        }

        _isTouchArmed = false;
        Start(new TriggeredEvent(cellEvent.Event, Source: null));
        return true;
    }

    private void Start(TriggeredEvent triggered)
    {
        var run = new EventRun(triggered, _map, _context.Inventory, _context.Catalog.Items);
        _run = run.IsFinished ? null : run;
    }

    private void ThrowIfEventRunning()
    {
        if (_run is not null)
        {
            throw new InvalidOperationException("An event is running; call ContinueEvent until it ends.");
        }
    }
}
