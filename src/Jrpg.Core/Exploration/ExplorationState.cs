using System.Diagnostics.CodeAnalysis;
using Jrpg.Core.Battle;
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
    private readonly PlayerWalker _walker;
    private readonly RandomEncounters _encounters;
    private ActiveMap _map;
    private MapCollision _collision;
    private InteractionFinder _finder;
    private GridCell _lastCell;
    private bool _isTouchArmed;
    private EventRun? _run;

    internal ExplorationState(ExplorationContext context, MapLocation start)
    {
        _context = context;
        _walker = new PlayerWalker(start.Position, start.Facing, context.Catalog.Rules.Exploration);
        _encounters = new RandomEncounters(context.Random);
        EnterMap(start.MapId, start.Position);
        RefreshInteraction();
    }

    public MapLocation Location => new(_map.Definition.Id, _walker.Position, _walker.Facing);

    /// <summary>The map the player is on, read every frame so the Game can redraw after a warp.</summary>
    public MapDefinition CurrentMap => _map.Definition;

    /// <summary>The player's position, read every frame without building a <see cref="MapLocation"/>.</summary>
    public Point2D PlayerPosition => _walker.Position;

    public Facing PlayerFacing => _walker.Facing;

    /// <summary>True while the player pushes in a direction, even against a wall, so the walk animation plays.</summary>
    public bool IsPlayerMoving => _walker.IsMoving;

    /// <summary>The event step the player is reading, or null when no event runs.</summary>
    public EventStep? CurrentEventStep => _run?.Current;

    public bool IsEventRunning => _run is not null;

    /// <summary>What the current give-item step really added and lost, for its message; null on any other step.</summary>
    public ItemReceipt? CurrentItemReceipt => _run?.CurrentReceipt;

    /// <summary>
    /// What a placed object on the current map is now, after any swap (an opened chest or door), so the Game draws it
    /// as the player left it.
    /// </summary>
    public ObjectDefinition CurrentDefinitionOf(PlacedObject placed) => _map.CurrentDefinitionOf(placed);

    /// <summary>
    /// The battle a step just started, or null. The Game runs it, then calls <see cref="FinishEncounter"/>; the player
    /// can't walk or interact meanwhile. What follows a defeat is the Game's (decision 31).
    /// </summary>
    public EncounterDefinition? PendingEncounter { get; private set; }

    /// <summary>
    /// What the confirm button would start on the object the player faces, for the Game's prompt; null while an event
    /// runs or a battle is pending. The same record while the target stays the same, so the Game can compare references.
    /// </summary>
    public InteractionTarget? InteractionInFront { get; private set; }

    /// <summary>
    /// Moves the player. Entering a new cell starts its touch event, once per entry, or else rolls for a random battle
    /// (one step). Otherwise, while moving, a touch event on the object in front starts, if the player has stood still
    /// since entering the map or since the last cell or object touch event (decision 38).
    /// </summary>
    /// <exception cref="InvalidOperationException">An event is running or a battle is pending.</exception>
    public void Walk(MoveIntent intent, TimeSpan delta)
    {
        ThrowIfBusy();
        WalkAndTrigger(intent, delta);
        RefreshInteraction();
    }

    /// <summary>Starts the interact event of the object the player faces, as the confirm button does.</summary>
    /// <returns>False when nothing in front can be interacted with.</returns>
    /// <exception cref="InvalidOperationException">An event is running or a battle is pending.</exception>
    public bool TryInteract()
    {
        ThrowIfBusy();
        if (_finder.FindInFront(_walker.Position, _walker.Facing, EventActivation.Interact) is not { } triggered)
        {
            return false;
        }

        Start(triggered);
        RefreshInteraction();
        return true;
    }

    /// <summary>
    /// The player has read the current step, or the Game has faded out for a warp, which then moves the player to the
    /// center of the target cell. The event moves on to its next waiting step or ends.
    /// </summary>
    /// <exception cref="InvalidOperationException">No event is running.</exception>
    public void ContinueEvent()
    {
        var run = _run ?? throw new InvalidOperationException("No event is running.");
        if (run.Current is WarpStep warp)
        {
            EnterMap(warp.MapId, warp.Cell.Center);
        }

        run.Continue();
        if (run.IsFinished)
        {
            _run = null;
        }

        RefreshInteraction();
    }

    /// <summary>The pending battle is over, whatever its outcome; the next steps are grace steps again (decision 26).</summary>
    /// <exception cref="InvalidOperationException">No encounter is pending.</exception>
    public void FinishEncounter()
    {
        if (PendingEncounter is null)
        {
            throw new InvalidOperationException("No encounter is pending.");
        }

        PendingEncounter = null;
        _encounters.ResetSteps();
        RefreshInteraction();
    }

    private void WalkAndTrigger(MoveIntent intent, TimeSpan delta)
    {
        _walker.Walk(intent, delta, _collision);
        if (!_walker.IsMoving)
        {
            _isTouchArmed = true;
            return;
        }

        if (_walker.Cell != _lastCell)
        {
            _lastCell = _walker.Cell;
            if (TryStartCellEvent(_lastCell) || TryStartEncounter())
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

    private void RefreshInteraction()
    {
        var found = _run is null && PendingEncounter is null
            ? _finder.FindInFront(_walker.Position, _walker.Facing, EventActivation.Interact)
            : null;
        if (found is not { Source: { } placed } triggered)
        {
            InteractionInFront = null;
            return;
        }

        if (InteractionInFront is { } current && ReferenceEquals(current.Placed, placed) && ReferenceEquals(current.Event, triggered.Event))
        {
            return;
        }

        InteractionInFront = new InteractionTarget(placed, triggered.Event);
    }

    /// <summary>
    /// The one way onto a map, for a new game, a warp and (later) a loaded save, as legacy EnterMap was. The arrival
    /// cell stays quiet, and object touch events wait until the player has stood still (decision 38).
    /// </summary>
    [MemberNotNull(nameof(_map), nameof(_collision), nameof(_finder))]
    private void EnterMap(string mapId, Point2D target)
    {
        var catalog = _context.Catalog;
        var definition = catalog.Maps[mapId];
        var walkability = WalkabilityGrid.Build(definition.Layout, catalog.Tiles);
        _map = new ActiveMap(definition, catalog.Objects, walkability, _context.World);
        _collision = new MapCollision(_map);
        _finder = new InteractionFinder(_map, catalog.Rules.Exploration);
        _walker.PlaceAt(WarpArrival.Find(walkability, target));
        _lastCell = _walker.Cell;
        _isTouchArmed = false;
        _encounters.ResetSteps();
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

    // Called once per new cell without a cell event: that's what counts as a step.
    private bool TryStartEncounter()
    {
        var encounterId = _encounters.RollStep(_map.Definition.Settings.RandomEncounters);
        if (encounterId is null)
        {
            return false;
        }

        PendingEncounter = _context.Catalog.Encounters[encounterId];
        return true;
    }

    private void Start(TriggeredEvent triggered)
    {
        var run = new EventRun(triggered, _map, _context.Inventory, _context.Catalog.Items);
        _run = run.IsFinished ? null : run;
    }

    private void ThrowIfBusy()
    {
        if (_run is not null)
        {
            throw new InvalidOperationException("An event is running; call ContinueEvent until it ends.");
        }

        if (PendingEncounter is not null)
        {
            throw new InvalidOperationException("A battle is pending; call FinishEncounter once it's over.");
        }
    }
}
