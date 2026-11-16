using Jrpg.Core.Loot;

namespace Jrpg.Data.Tests.Items;

internal static class AffixPools
{
    /// <summary>The pool for items without fixed affixes, whose checks never read it.</summary>
    public static IReadOnlyDictionary<string, AffixDefinition> None { get; } = new Dictionary<string, AffixDefinition>();
}
