using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Saving;
using Jrpg.Core.World;

namespace Jrpg.Core.Session;

/// <summary>All mutable state of one playthrough; what a save captures. Replaces the demo's static GameSystems.</summary>
public sealed class GameSession
{
    // The catalog, inventory, world and random source shared by exploration and battles.
    private readonly ExplorationContext _context;

    private GameSession(PartyState party, ExplorationContext context, MapLocation start)
    {
        Party = party;
        _context = context;
        Inventory = context.Inventory;
        Exploration = new ExplorationState(context, party, start);
        Equipment = new PartyEquipment(Inventory);
        FieldItems = new FieldItemUse(party, Inventory);
        EnemyTactics = new EnemyTactics(context.Random);
    }

    public PartyState Party { get; }

    public Inventory Inventory { get; }

    public PartyEquipment Equipment { get; }

    /// <summary>Using consumables from the menu.</summary>
    public FieldItemUse FieldItems { get; }

    /// <summary>The player on the current map.</summary>
    public ExplorationState Exploration { get; }

    /// <summary>What enemies do in this playthrough's battles, drawing from the session's random source (ADR 0004).</summary>
    public EnemyTactics EnemyTactics { get; }

    /// <summary>The battle started by <see cref="StartPendingBattle"/>, until <see cref="FinishBattle"/>; null otherwise.</summary>
    public BattleState? ActiveBattle { get; private set; }

    /// <summary>Everything since the playthrough began, menus, battles and pause included (developer decision 35).</summary>
    public TimeSpan PlayTime { get; private set; }

    /// <summary>Starts a playthrough led by the chosen character, alone in the party (decision 163).</summary>
    /// <param name="catalog">Validated content; the new game starts from its <see cref="ContentCatalog.NewGame"/>.</param>
    /// <param name="leaderId">The member chosen to lead; one of the new game's leader choices.</param>
    /// <param name="random">The seeded source for everything random in the playthrough, such as battle rolls.</param>
    /// <exception cref="ArgumentException">No leader choice has <paramref name="leaderId"/>.</exception>
    public static GameSession StartNew(ContentCatalog catalog, string leaderId, IRandom random)
    {
        var newGame = catalog.NewGame;
        if (!newGame.LeaderChoices.Any(choice => choice.MemberId == leaderId))
        {
            throw new ArgumentException($"'{leaderId}' isn't one of the new game's leader choices.", nameof(leaderId));
        }

        var leaderDefinition = catalog.PartyMembers[leaderId];
        var leader = PartyMember.Recruit(leaderDefinition, catalog.ClassOf(leaderDefinition), catalog.Skills);
        var party = new PartyState([leader], [leader], catalog.Rules.Party);
        var inventory = InventoryHolding(newGame.StartingItems, catalog);
        return new GameSession(party, new ExplorationContext(catalog, inventory, new WorldState(), random), newGame.StartLocation);
    }

    /// <summary>
    /// Rebuilds a playthrough from a save. The save is assumed repaired against the catalog by Data (ADR 0007). The
    /// player enters the saved map the way a warp or a new game does.
    /// </summary>
    /// <param name="save">What the playthrough was when saved.</param>
    /// <param name="catalog">The content the save is restored against.</param>
    /// <param name="random">A fresh seeded source; the random state isn't saved (developer decision 43).</param>
    public static GameSession Restore(SaveGame save, ContentCatalog catalog, IRandom random)
    {
        var members = save.Members.Select(progress => RestoreMember(progress, catalog)).ToList();
        var party = new PartyState(members, ActiveOrder(members, save.ActiveMemberIds), catalog.Rules.Party);
        var inventory = InventoryHolding(save.Inventory, catalog);
        var world = new WorldState(save.CompletedEvents, save.ObjectSwaps);
        return new GameSession(party, new ExplorationContext(catalog, inventory, world, random), save.Location) { PlayTime = save.PlayTime };
    }

    /// <summary>
    /// Starts the encounter a step left pending, between the active party and its enemies in encounter order. With every
    /// active member fallen, the battle starts already lost (decision 97), so the defeat screen offers its choices.
    /// </summary>
    /// <exception cref="InvalidOperationException">No encounter is pending, or a battle is already active.</exception>
    public BattleState StartPendingBattle()
    {
        if (ActiveBattle is not null)
        {
            throw new InvalidOperationException("A battle is already active; finish it first.");
        }

        var encounter = Exploration.PendingEncounter ?? throw new InvalidOperationException("No encounter is pending.");
        var enemies = encounter.EnemyIds.Select(id => _context.Catalog.Enemies[id]).ToList();
        ActiveBattle = BattleState.Start(Party, enemies, Inventory, _context.Random);
        return ActiveBattle;
    }

    /// <summary>
    /// Ends the active battle whatever its outcome: a victory grants its experience, and the encounter is cleared so the
    /// grace steps restart and saving works again.
    /// </summary>
    /// <returns>What a victory gave; null after an escape or a defeat.</returns>
    /// <exception cref="InvalidOperationException">No battle is active, or it isn't over.</exception>
    public VictoryRewards? FinishBattle()
    {
        if (ActiveBattle is not { IsOver: true } battle)
        {
            throw new InvalidOperationException("Only a battle that is over can be finished.");
        }

        LastingResourceCarry.CarryBack(battle);
        var rewards = battle.Phase == BattlePhase.Victory ? battle.GrantVictoryExperience(_context.Catalog.Rules.Experience) : null;
        Exploration.FinishEncounter();
        ActiveBattle = null;
        return rewards;
    }

    /// <summary>The Game adds every update's delta, so play time counts everything (developer decision 35).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The delta is negative.</exception>
    public void AddPlayTime(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        PlayTime += delta;
    }

    /// <summary>What a save keeps of this playthrough right now.</summary>
    /// <param name="savedAt">The Game's clock; Core never reads the time itself.</param>
    /// <exception cref="InvalidOperationException">An event is running or a battle is pending; saves happen between them (ADR 0007).</exception>
    public SaveGame Capture(DateTimeOffset savedAt)
    {
        if (Exploration.IsEventRunning || Exploration.PendingEncounter is not null)
        {
            throw new InvalidOperationException("Can't save while an event runs or a battle is pending.");
        }

        return new SaveGame
        {
            SavedAt = savedAt,
            PlayTime = PlayTime,
            Location = Exploration.Location,
            Members = [.. Party.Members.Select(member => member.CaptureProgress())],
            ActiveMemberIds = [.. Party.ActiveMembers.Select(member => member.Id)],
            Inventory = [.. Inventory.Entries],
            CompletedEvents = _context.World.CompletedEvents,
            ObjectSwaps = _context.World.ObjectSwaps,
        };
    }

    private static List<PartyMember> ActiveOrder(List<PartyMember> members, IReadOnlyList<string> activeIds) =>
        [.. activeIds.Select(id => members.Single(member => member.Id == id))];

    private static Inventory InventoryHolding(IReadOnlyList<InventoryEntry> entries, ContentCatalog catalog)
    {
        var inventory = new Inventory();
        foreach (var entry in entries)
        {
            inventory.Add(catalog.Items[entry.ItemId], entry.Quantity);
        }

        return inventory;
    }

    // Worn items go straight onto the member: under the move model they aren't in the saved inventory (ADR 0003).
    private static PartyMember RestoreMember(PartyMemberProgress progress, ContentCatalog catalog)
    {
        var definition = catalog.PartyMembers[progress.Id];
        var member = PartyMember.Restore(definition, catalog.ClassOf(definition), catalog.Skills, progress);
        if (progress.WeaponId is { } weaponId)
        {
            member.Wear(EquipmentSlot.Weapon, catalog.Items[weaponId]);
        }

        if (progress.ArmorId is { } armorId)
        {
            member.Wear(EquipmentSlot.Armor, catalog.Items[armorId]);
        }

        return member;
    }
}
