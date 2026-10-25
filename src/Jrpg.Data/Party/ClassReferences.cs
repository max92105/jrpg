using Jrpg.Core.Skills;

namespace Jrpg.Data.Party;

/// <summary>The already-loaded content a class definition refers to and must agree with.</summary>
internal sealed record ClassReferences(IReadOnlyDictionary<string, SkillDefinition> Skills);
