using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;

namespace Jrpg.Data.Content;

/// <summary>The already-loaded content the new-game file refers to and must agree with.</summary>
internal sealed record NewGameReferences(
    IReadOnlySet<string> MemberIds,
    IReadOnlyDictionary<string, ItemDefinition> Items,
    PartyRules Party,
    IReadOnlyDictionary<string, MapDefinition> Maps);
