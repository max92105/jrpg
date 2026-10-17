using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Session;

/// <summary>All mutable state of one playthrough; what a save will capture. Replaces the demo's static GameSystems.</summary>
public sealed class GameSession
{
    private GameSession(PartyState party, Inventory inventory)
    {
        Party = party;
        Inventory = inventory;
        Equipment = new PartyEquipment(inventory);
        FieldItems = new FieldItemUse(party, inventory);
    }

    public PartyState Party { get; }

    public Inventory Inventory { get; }

    public PartyEquipment Equipment { get; }

    /// <summary>Using consumables from the menu.</summary>
    public FieldItemUse FieldItems { get; }

    /// <summary>Starts a playthrough from <see cref="ContentCatalog.NewGame"/>. The catalog is assumed validated by Data.</summary>
    public static GameSession StartNew(ContentCatalog catalog)
    {
        var newGame = catalog.NewGame;
        var members = newGame.RecruitedMemberIds.Select(id => Recruit(catalog, id)).ToList();
        var activeOrder = newGame.ActiveMemberIds.Select(id => members.Single(member => member.Id == id)).ToList();
        var party = new PartyState(members, activeOrder, catalog.Rules.Party);

        var inventory = new Inventory();
        foreach (var entry in newGame.StartingItems)
        {
            inventory.Add(catalog.Items[entry.ItemId], entry.Quantity);
        }

        return new GameSession(party, inventory);
    }

    private static PartyMember Recruit(ContentCatalog catalog, string memberId)
    {
        var definition = catalog.PartyMembers[memberId];
        return PartyMember.Recruit(definition, catalog.Skills);
    }
}
