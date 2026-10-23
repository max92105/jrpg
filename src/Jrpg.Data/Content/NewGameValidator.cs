using System.Globalization;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;

namespace Jrpg.Data.Content;

internal static class NewGameValidator
{
    public static void Validate(string fileName, NewGameDefinition newGame, NewGameReferences references)
    {
        var problem = FindLeaderChoicesProblem(newGame.LeaderChoices, references)
            ?? FindStartingItemsProblem(newGame.StartingItems, references)
            ?? FindStartLocationProblem(newGame.StartLocation, references)
            ?? (string.IsNullOrWhiteSpace(newGame.FieldSpriteFile) ? "fieldSpriteFile must name the map sprite's file." : null);
        if (problem is not null)
        {
            throw new ContentValidationException($"{fileName}: {problem}");
        }
    }

    private static string? FindLeaderChoicesProblem(IReadOnlyList<LeaderChoice> choices, NewGameReferences references)
    {
        if (choices.Count == 0)
        {
            return "leaderChoices must list at least one character.";
        }

        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (choices.Any(choice => choice is null || string.IsNullOrWhiteSpace(choice.MemberId)))
        {
            return "leaderChoices has a null entry or an empty memberId.";
        }

        var unknownId = choices.Select(choice => choice.MemberId).FirstOrDefault(id => !references.MemberIds.Contains(id));
        return unknownId is not null
            ? $"leaderChoices has unknown party member id '{unknownId}'."
            : FindRepeatedId("leaderChoices", [.. choices.Select(choice => choice.MemberId)]);
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

    private static string? FindStartLocationProblem(MapLocation start, NewGameReferences references)
    {
        if (!references.Maps.TryGetValue(start.MapId, out var map))
        {
            return $"startLocation.mapId has unknown map id '{start.MapId}'.";
        }

        var layout = map.Layout;
        var position = start.Position;
        return layout.Contains(GridCell.Containing(position))
            ? null
            : string.Create(
                CultureInfo.InvariantCulture,
                $"startLocation.position ({position.X}, {position.Y}) must be inside map '{start.MapId}' ({layout.Width}x{layout.Height}).");
    }

    private static string? FindRepeatedId(string field, IReadOnlyList<string> ids)
    {
        var repeated = ids.GroupBy(id => id).FirstOrDefault(group => group.Count() > 1)?.Key;
        return repeated is null ? null : $"{field} lists '{repeated}' more than once.";
    }
}
