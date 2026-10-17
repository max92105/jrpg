using Jrpg.Core.Content;
using Jrpg.Core.Items;

namespace Jrpg.Data.Content;

internal static class NewGameValidator
{
    public static void Validate(string fileName, NewGameDefinition newGame, NewGameReferences references)
    {
        var problem = FindRecruitedProblem(newGame, references)
            ?? FindActiveProblem(newGame, references)
            ?? FindStartingItemsProblem(newGame.StartingItems, references);
        if (problem is not null)
        {
            throw new ContentValidationException($"{fileName}: {problem}");
        }
    }

    private static string? FindRecruitedProblem(NewGameDefinition newGame, NewGameReferences references)
    {
        if (newGame.RecruitedMemberIds.Count == 0)
        {
            return "recruitedMemberIds must list at least one party member.";
        }

        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (newGame.RecruitedMemberIds.Any(string.IsNullOrWhiteSpace) || newGame.ActiveMemberIds.Any(string.IsNullOrWhiteSpace))
        {
            return "recruitedMemberIds and activeMemberIds must not contain a null or empty id.";
        }

        var unknownId = newGame.RecruitedMemberIds.FirstOrDefault(id => !references.MemberIds.Contains(id));
        return unknownId is not null
            ? $"recruitedMemberIds has unknown party member id '{unknownId}'."
            : FindRepeatedId("recruitedMemberIds", newGame.RecruitedMemberIds);
    }

    private static string? FindActiveProblem(NewGameDefinition newGame, NewGameReferences references)
    {
        var activeCount = newGame.ActiveMemberIds.Count;
        if (activeCount == 0 || activeCount > references.Party.MaxActiveMembers)
        {
            return $"activeMemberIds must list 1 to {references.Party.MaxActiveMembers} members, leader first (was {activeCount}).";
        }

        var notRecruitedId = newGame.ActiveMemberIds.FirstOrDefault(id => !newGame.RecruitedMemberIds.Contains(id));
        return notRecruitedId is not null
            ? $"activeMemberIds has '{notRecruitedId}', who isn't in recruitedMemberIds."
            : FindRepeatedId("activeMemberIds", newGame.ActiveMemberIds);
    }

    private static string? FindStartingItemsProblem(IReadOnlyList<InventoryEntry> entries, NewGameReferences references)
    {
        foreach (var entry in entries)
        {
            // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
            if (entry is null)
            {
                return "startingItems has a null entry.";
            }

            if (!references.Items.TryGetValue(entry.ItemId, out var item))
            {
                return $"startingItems has unknown item id '{entry.ItemId}'.";
            }

            if (entry.Quantity < 1 || entry.Quantity > item.MaxStack)
            {
                return $"startingItems '{entry.ItemId}': quantity must be within 1..{item.MaxStack} (was {entry.Quantity}).";
            }
        }

        return FindRepeatedId("startingItems", [.. entries.Select(entry => entry.ItemId)]);
    }

    private static string? FindRepeatedId(string field, IReadOnlyList<string> ids)
    {
        var repeated = ids.GroupBy(id => id).FirstOrDefault(group => group.Count() > 1)?.Key;
        return repeated is null ? null : $"{field} lists '{repeated}' more than once.";
    }
}
