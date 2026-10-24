namespace Jrpg.Core.Party;

/// <summary>
/// What changes about a party member during play, as a save keeps it; worn items by item id, null when the slot is
/// empty. Everything else comes from content: skills from the member's definition, max HP/MP and base stats from their
/// class at their level (ADR 0007, ADR 0017).
/// </summary>
public sealed record PartyMemberProgress(string Id, int Level, int Experience, int Hp, int Mp, string? WeaponId, string? ArmorId);
