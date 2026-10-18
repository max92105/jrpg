using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Saving;
using Jrpg.Core.World;

namespace Jrpg.Core.Session;

/// <summary>All mutable state of one playthrough; what a save captures. Replaces the demo's static GameSystems.</summary>
public sealed class GameSession
{
    private readonly WorldState _world;

    private GameSession(PartyState party, Inventory inventory, WorldState world, ExplorationState exploration)
    {
        Party = party;
        Inventory = inventory;
        _world = world;
        Exploration = exploration;
        Equipment = new PartyEquipment(inventory);
        FieldItems = new FieldItemUse(party, inventory);
    }

    public PartyState Party { get; }

    public Inventory Inventory { get; }

    public PartyEquipment Equipment { get; }

    /// <summary>Using consumables from the menu.</summary>
    public FieldItemUse FieldItems { get; }

    /// <summary>The player on the current map.</summary>
    public ExplorationState Exploration { get; }

    /// <summary>Everything since the playthrough began, menus, battles and pause included (developer decision 35).</summary>
    public TimeSpan PlayTime { get; private set; }

    /// <summary>Starts a playthrough. The catalog is assumed validated by Data.</summary>
    /// <param name="catalog">Validated content; the new game starts from its <see cref="ContentCatalog.NewGame"/>.</param>
    /// <param name="random">The seeded source for everything random in the playthrough, such as battle rolls.</param>
    public static GameSession StartNew(ContentCatalog catalog, IRandom random)
    {
        var newGame = catalog.NewGame;
        var members = newGame.RecruitedMemberIds.Select(id => PartyMember.Recruit(catalog.PartyMembers[id], catalog.Skills)).ToList();
        var party = new PartyState(members, ActiveOrder(members, newGame.ActiveMemberIds), catalog.Rules.Party);
        var inventory = InventoryHolding(newGame.StartingItems, catalog);
        var world = new WorldState();
        var exploration = new ExplorationState(new ExplorationContext(catalog, inventory, world, random), newGame.StartLocation);
        return new GameSession(party, inventory, world, exploration);
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
        var exploration = new ExplorationState(new ExplorationContext(catalog, inventory, world, random), save.Location);
        return new GameSession(party, inventory, world, exploration) { PlayTime = save.PlayTime };
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
            CompletedEvents = _world.CompletedEvents,
            ObjectSwaps = _world.ObjectSwaps,
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
        var member = PartyMember.Restore(catalog.PartyMembers[progress.Id], catalog.Skills, progress);
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
