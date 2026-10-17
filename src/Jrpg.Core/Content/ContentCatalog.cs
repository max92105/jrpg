using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Content;

/// <summary>All loaded content definitions, by id, and the global rules.</summary>
public sealed record ContentCatalog(
    IReadOnlyDictionary<string, ItemDefinition> Items,
    IReadOnlyDictionary<string, ClassDefinition> Classes,
    IReadOnlyDictionary<string, PartyMemberDefinition> PartyMembers,
    GameRules Rules);
