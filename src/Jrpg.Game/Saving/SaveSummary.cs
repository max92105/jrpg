namespace Jrpg.Game.Saving;

/// <summary>What a save file's entry shows of a readable save (legacy <c>SaveDataPreview</c>).</summary>
/// <param name="LocationName">The display name of the map the save is on.</param>
/// <param name="PlayTime">Play time when saved.</param>
/// <param name="SavedAt">When the player saved.</param>
/// <param name="ActiveParty">The active party in order, leader first.</param>
internal sealed record SaveSummary(string LocationName, TimeSpan PlayTime, DateTimeOffset SavedAt, IReadOnlyList<PartyMemberSummary> ActiveParty);
