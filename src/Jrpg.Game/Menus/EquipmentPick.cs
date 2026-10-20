using Jrpg.Core.Items;

namespace Jrpg.Game.Menus;

/// <summary>What the player chose for a slot.</summary>
/// <param name="Item">The item to wear; null to take off what's worn.</param>
internal sealed record EquipmentPick(ItemDefinition? Item);
