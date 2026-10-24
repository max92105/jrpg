namespace Jrpg.Data.Party;

/// <summary>The already-loaded content a party member definition refers to and must agree with.</summary>
internal sealed record PartyMemberReferences(IReadOnlySet<string> ClassIds, IReadOnlySet<string> SkillIds);
