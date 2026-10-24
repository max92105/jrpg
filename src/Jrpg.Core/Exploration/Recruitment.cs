using Jrpg.Core.Party;

namespace Jrpg.Core.Exploration;

/// <summary>What a recruit step did, for its message: the member it names, and whether they joined just now.</summary>
/// <param name="Member">The member who joined, or the one already in the party.</param>
/// <param name="HasJoined">False when the member was already recruited, so nothing changed.</param>
public sealed record Recruitment(PartyMember Member, bool HasJoined);
