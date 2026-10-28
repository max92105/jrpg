using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>What the player chose for a slot.</summary>
/// <param name="Piece">The piece to wear; null to take off what's worn.</param>
/// <param name="Giver">
/// The member wearing the piece, to take it from (decision 137); null for "Remove" and owned pieces.
/// </param>
internal sealed record EquipmentPick(GearPiece? Piece, PartyMember? Giver);
