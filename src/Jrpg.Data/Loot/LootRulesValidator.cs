using System.Text.Json;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Loot;

/// <summary>
/// Checks <c>rules.json</c> <c>loot</c>, the rarity table (ADR 0030): Common, Magic and Rare each have a row and Unique
/// none, as it never rolls (decision 339); weights are at least 0 with one above it; Common rolls no affix, and each
/// row's affix count range stays within 0..5 (placeholder bound).
/// </summary>
internal static class LootRulesValidator
{
    private const int MostAffixes = 5;

    private static readonly Rarity[] _rolled = [Rarity.Common, Rarity.Magic, Rarity.Rare];

    public static void Validate(string fileName, LootRules loot)
    {
        if (FindProblem(loot.Rarities) is { } problem)
        {
            throw new ContentValidationException($"{fileName}: loot.rarities.{problem}");
        }
    }

    private static string? FindProblem(IReadOnlyDictionary<Rarity, RarityRule> rarities)
    {
        if (rarities.ContainsKey(Rarity.Unique))
        {
            return $"{NameOf(Rarity.Unique)} must not be listed: unique items never roll.";
        }

        foreach (var rarity in _rolled)
        {
            if (!rarities.TryGetValue(rarity, out var rule))
            {
                return $"{NameOf(rarity)} is missing.";
            }

            // The serializer doesn't apply nullable annotations to dictionary values, so a stray null gets here.
            if (rule is null)
            {
                return $"{NameOf(rarity)} is null.";
            }

            if (FindRuleProblem(rarity, rule) is { } problem)
            {
                return $"{NameOf(rarity)}.{problem}";
            }
        }

        return rarities.Values.All(rule => rule.Weight == 0) ? "weight must be above 0 for at least one rarity." : null;
    }

    // Common is the plain tier: it rolls no affix (decision 339).
    private static string? FindRuleProblem(Rarity rarity, RarityRule rule)
    {
        var mostAffixes = rarity == Rarity.Common ? 0 : MostAffixes;
        return AtLeast("weight", rule.Weight, 0)
            ?? Within("minAffixes", rule.MinAffixes, 0, mostAffixes)
            ?? Within("maxAffixes", rule.MaxAffixes, rule.MinAffixes, mostAffixes);
    }

    private static string NameOf(Rarity rarity) => JsonNamingPolicy.CamelCase.ConvertName(rarity.ToString());
}
