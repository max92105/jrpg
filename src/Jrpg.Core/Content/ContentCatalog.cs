using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Content;

/// <summary>All loaded content definitions, by id, and the global rules.</summary>
public sealed record ContentCatalog
{
    public required IReadOnlyDictionary<string, ItemDefinition> Items { get; init; }

    public required IReadOnlyDictionary<string, ClassDefinition> Classes { get; init; }

    public required IReadOnlyDictionary<string, SkillDefinition> Skills { get; init; }

    public required IReadOnlyDictionary<string, PartyMemberDefinition> PartyMembers { get; init; }

    public required IReadOnlyDictionary<string, EnemyDefinition> Enemies { get; init; }

    public required IReadOnlyDictionary<string, EncounterDefinition> Encounters { get; init; }

    public required GameRules Rules { get; init; }
}
