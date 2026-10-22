using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>What the player chose for a slot.</summary>
/// <param name="Item">The item to wear; null to take off what's worn.</param>
/// <param name="Giver">The member wearing the item, to take it from (decision 137); null for "Remove" and owned items.</param>
internal sealed record EquipmentPick(ItemDefinition? Item, PartyMember? Giver);
