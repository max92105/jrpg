using System.Diagnostics.CodeAnalysis;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Party;

namespace Jrpg.Core.Exploration;

/// <summary>
/// Starts the current map's events when their trigger happens, and holds the one running (ADR 0006): the map's arrival
/// event once the player is on it and nothing else runs; a cell's touch event on entering the cell; the touch event of
/// the object in front, armed by standing still (decision 38); the interact event of the object in front on Confirm.
/// Which event of a cell may start is the leader's and the "once" marks' to decide, as for an object's.
/// </summary>
internal sealed class MapEventStarter
{
    private readonly ExplorationContext _context;
    private readonly PartyState _party;
    private ActiveMap _map;
    private InteractionFinder _finder;
    private bool _isTouchArmed;
    private bool _isArrivalPending;

    /// <param name="context">The content, items, world and random source an event changes.</param>
    /// <param name="party">Who recruit steps add to; its leader (fixed, decision 107) gates events.</param>
    /// <param name="map">The map entered first; <see cref="EnteredMap"/> replaces it on a warp.</param>
    /// <param name="finder">What stands in front of the player on that map.</param>
    public MapEventStarter(ExplorationContext context, PartyState party, ActiveMap map, InteractionFinder finder)
    {
        _context = context;
        _party = party;
        EnteredMap(map, finder);
    }

    /// <summary>The event in progress, with what its current step did; null while none runs.</summary>
    public EventRun? Run { get; private set; }

    /// <summary>
    /// A map was entered (a new game, a warp, a load): its arrival event waits for no event to run (ADR 0016), and
    /// object touch events wait until the player has stood still (decision 38).
    /// </summary>
    [MemberNotNull(nameof(_map), nameof(_finder))]
    public void EnteredMap(ActiveMap map, InteractionFinder finder)
    {
        _map = map;
        _finder = finder;
        _isTouchArmed = false;
        _isArrivalPending = true;
    }

    /// <summary>
    /// The map's arrival event runs once the player is on it and no event runs: at once on a new game or a load, and
    /// after the warping event has finished its remaining steps.
    /// </summary>
    public void StartArrivalIfPending()
    {
        if (!_isArrivalPending || Run is not null)
        {
            return;
        }

        _isArrivalPending = false;
        if (ArrivalEvents.FindFirst(_map, _party.Leader.Id) is { } arrival)
        {
            Start(arrival);
        }
    }

    /// <summary>The player stood still: the next touch event in front may start (decision 38).</summary>
    public void ArmTouch() => _isTouchArmed = true;

    /// <summary>Starts the touch event of the cell the player just entered, if it has one that may run now.</summary>
    public bool TryStartCellEvent(GridCell cell)
    {
        var cellEvent = _map.CellEventAt(cell);
        if (cellEvent is null || !cellEvent.Event.IsForLeader(_party.Leader.Id) || (cellEvent.Event.IsOnce && _map.IsDone(cellEvent.Event)))
        {
            return false;
        }

        _isTouchArmed = false;
        Start(new TriggeredEvent(cellEvent.Event, Source: null));
        return true;
    }

    /// <summary>Starts the touch event of the object in front, once per arming.</summary>
    public void StartTouchInFrontIfArmed(Point2D position, Facing facing)
    {
        if (_isTouchArmed && _finder.FindInFront(position, facing, EventActivation.Touch) is { } touched)
        {
            _isTouchArmed = false;
            Start(touched);
        }
    }

    /// <summary>Starts the interact event of the object in front, as the confirm button does.</summary>
    /// <returns>False when nothing in front can be interacted with.</returns>
    public bool TryStartInteract(Point2D position, Facing facing)
    {
        if (_finder.FindInFront(position, facing, EventActivation.Interact) is not { } triggered)
        {
            return false;
        }

        Start(triggered);
        return true;
    }

    /// <summary>
    /// The player read the current step (ADR 0006): the event moves on to its next waiting step or ends.
    /// </summary>
    /// <exception cref="InvalidOperationException">No event is running.</exception>
    public void Continue()
    {
        var run = RequireRun();
        run.Continue();
        SettleAfterMoved(run);
    }

    /// <summary>The player skipped the cutscene: the event runs on past its presentation steps (ADR 0016).</summary>
    /// <exception cref="InvalidOperationException">No event is running.</exception>
    public void SkipPresentation()
    {
        var run = RequireRun();
        run.SkipPresentation();
        SettleAfterMoved(run);
    }

    private EventRun RequireRun() => Run ?? throw new InvalidOperationException("No event is running.");

    // After the event moved on: it ends if it's over, letting a pending arrival event start.
    private void SettleAfterMoved(EventRun run)
    {
        if (run.IsFinished)
        {
            Run = null;
            StartArrivalIfPending();
        }
    }

    private void Start(TriggeredEvent triggered)
    {
        var run = new EventRun(triggered, _map, _context, _party);
        Run = run.IsFinished ? null : run;
    }
}
