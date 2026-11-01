using System.Diagnostics.CodeAnalysis;
using Jrpg.Core.Battle;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;

namespace Jrpg.Core.Exploration;

/// <summary>
/// The player on the current map: walking, the random encounters, the interaction prompt, and the map entered. The Game
/// feeds it input each frame: walking, interact presses, and "continue" once the player has read a step (ADR 0006). The
/// map's events are started and run by the <see cref="MapEventStarter"/>; this reads what its current step did.
/// </summary>
public sealed class ExplorationState
{
    private readonly ExplorationContext _context;
    private readonly PartyState _party;
    private readonly PlayerWalker _walker;
    private readonly RandomEncounters _encounters;
    private readonly MapEventStarter _events;
    private ActiveMap _map;
    private MapCollision _collision;
    private InteractionFinder _finder;
    private GridCell _lastCell;

    /// <param name="context">The content, items, world and random source.</param>
    /// <param name="party">Who recruit steps add to; its leader (fixed, decision 107) gates events.</param>
    /// <param name="start">Where the player lands: a new game's start, or a save's location.</param>
    internal ExplorationState(ExplorationContext context, PartyState party, MapLocation start)
    {
        _context = context;
        _party = party;
        _walker = new PlayerWalker(start.Position, start.Facing, context.Catalog.Rules.Exploration);
        _encounters = new RandomEncounters(context.Random);
        EnterMap(start.MapId, start.Position);
        _events = new MapEventStarter(context, party, _map, _finder);
        _events.StartArrivalIfPending();
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
    public EventStep? CurrentEventStep => _events.Run?.Current;

    public bool IsEventRunning => _events.Run is not null;

    /// <summary>What the current give-item step really added and lost, for its message; null on any other step.</summary>
    public ItemReceipt? CurrentItemReceipt => _events.Run?.CurrentReceipt;

    /// <summary>What the current recruit step did, for its message; null on any other step.</summary>
    public Recruitment? CurrentRecruitment => _events.Run?.CurrentRecruitment;

    /// <summary>The current priced rest waiting for the Game to pay (decision 382); null on any other step.</summary>
    public InnStay? CurrentInnStay => _events.Run?.CurrentInnStay;

    /// <summary>The voice-over the running event started, for the Game to play; null when none speaks.</summary>
    public VoiceStep? StartedVoice => _events.Run?.StartedVoice;

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
        if (!_events.TryStartInteract(_walker.Position, _walker.Facing))
        {
            return false;
        }

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
        if (_events.Run?.Current is WarpStep warp)
        {
            EnterMap(warp.MapId, warp.Cell.Center);
            _events.EnteredMap(_map, _finder);
        }

        _events.Continue();
        RefreshInteraction();
    }

    /// <summary>
    /// The player skipped a cutscene: the event runs on past its presentation steps, such as stills, to the next step
    /// that changes the game or waits for the player, or ends (ADR 0016).
    /// </summary>
    /// <exception cref="InvalidOperationException">No event is running.</exception>
    public void SkipEventPresentation()
    {
        _events.SkipPresentation();
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
            _events.ArmTouch();
            return;
        }

        if (_walker.Cell != _lastCell)
        {
            _lastCell = _walker.Cell;
            if (_events.TryStartCellEvent(_lastCell) || TryStartEncounter())
            {
                return;
            }
        }

        _events.StartTouchInFrontIfArmed(_walker.Position, _walker.Facing);
    }

    private void RefreshInteraction()
    {
        var found = _events.Run is null && PendingEncounter is null
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
    /// The one way onto a map, for a new game, a warp and a loaded save (ADR 0006): the map, its collision and what
    /// stands in front; the player lands on the nearest walkable cell. The caller tells the events the map was entered.
    /// </summary>
    [MemberNotNull(nameof(_map), nameof(_collision), nameof(_finder))]
    private void EnterMap(string mapId, Point2D target)
    {
        var catalog = _context.Catalog;
        var definition = catalog.Maps[mapId];
        var walkability = WalkabilityGrid.Build(definition.Layout, catalog.Tiles);
        _map = new ActiveMap(definition, catalog.Objects, walkability, _context.World);
        _collision = new MapCollision(_map);
        _finder = new InteractionFinder(_map, catalog.Rules.Exploration, _party.Leader.Id);
        _walker.PlaceAt(WarpArrival.Find(walkability, target));
        _lastCell = _walker.Cell;
        _encounters.ResetSteps();
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

    private void ThrowIfBusy()
    {
        if (_events.Run is not null)
        {
            throw new InvalidOperationException("An event is running; call ContinueEvent until it ends.");
        }

        if (PendingEncounter is not null)
        {
            throw new InvalidOperationException("A battle is pending; call FinishEncounter once it's over.");
        }
    }
}
