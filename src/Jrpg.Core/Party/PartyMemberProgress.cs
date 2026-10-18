namespace Jrpg.Core.Party;

/// <summary>
/// What changes about a party member during play, as a save keeps it; worn items by item id, null when the slot is
/// empty. Everything else (max HP/MP, base stats, skills) comes from the member's definition (ADR 0007).
/// </summary>
public sealed record PartyMemberProgress(string Id, int Level, int Experience, int Hp, int Mp, string? WeaponId, string? ArmorId);
