using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Statuses;

/// <summary>
/// The rules for <c>statuses.json</c> (ADR 0055): each record named, with at least one part, at most one ticking part
/// and each kind once, every amount in its kind's range (decision 797), a damage type on damage ticks only (decision
/// 803); and the records of one family alike, part for part in kind and type, so a refresh can keep the stronger one of
/// each part (decision 800).
/// </summary>
internal static class StatusCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<StatusDefinition> statuses)
    {
        new ContentListValidator<StatusDefinition>(fileName, "status", status => status.Id).Validate(statuses, FindProblem);
        if (FindFamilyProblem(statuses) is { } problem)
        {
            throw new ContentValidationException($"{fileName}: {problem}");
        }
    }

    private static string? FindProblem(StatusDefinition status)
    {
        if (string.IsNullOrWhiteSpace(status.Name))
        {
            return "name must not be empty.";
        }

        if (status.Family is { } family && string.IsNullOrWhiteSpace(family))
        {
            return "family must name the statuses it shares, or be left out.";
        }

        return status.Parts is not { Count: > 0 } ? "parts must list at least one part." : FindPartsProblem(status.Parts);
    }

    private static string? FindPartsProblem(IReadOnlyList<StatusPart> parts)
    {
        var kinds = new HashSet<StatusPartKind>();
        for (var index = 0; index < parts.Count; index++)
        {
            // The serializer lets a null list element through.
            if (parts[index] is not { } part)
            {
                return $"parts[{index}] is null.";
            }

            if (!kinds.Add(part.Kind))
            {
                return $"parts[{index}]: {NameOf(part.Kind)} is listed twice.";
            }

            if ((FindAmountProblem(part, index) ?? FindTypeProblem(part, index)) is { } problem)
            {
                return problem;
            }
        }

        // A tick reports one amount and one tone (decision 797).
        return kinds.Contains(StatusPartKind.DamageEachTurn) && kinds.Contains(StatusPartKind.HealEachTurn)
            ? "a status ticks once: it can't hold both damageEachTurn and healEachTurn."
            : null;
    }

    private static string? FindAmountProblem(StatusPart part, int index)
    {
        var field = $"parts[{index}]: {NameOf(part.Kind)}'s amount";
        return part.Kind switch
        {
            StatusPartKind.DamageEachTurn or StatusPartKind.HealEachTurn => RangeProblems.AtLeast(field, part.Amount, 1),
            StatusPartKind.SkipTurn or StatusPartKind.DrawAttacks =>
                part.Amount == 0 ? null : $"parts[{index}]: {NameOf(part.Kind)} has no amount (was {part.Amount}).",
            StatusPartKind.ResistanceChange => part.Amount == 0 ? $"{field} must not be 0." : null,
            StatusPartKind.DamageTakenChange or StatusPartKind.DamageDealtChange =>
                part.Amount is > -100 and not 0 ? null : $"{field} must be above -100 and not 0 (was {part.Amount}).",
            _ => throw new InvalidOperationException($"No amount rule for {part.Kind}."),
        };
    }

    // A type means what a damage tick meets resistance as (decision 803): no other kind deals damage.
    private static string? FindTypeProblem(StatusPart part, int index)
    {
        if (part.DamageType is not { } type || part.Kind == StatusPartKind.DamageEachTurn)
        {
            return null;
        }

        var typeName = ContentJsonOptions.EnumName(type);
        return $"parts[{index}]: damageType is only for damageEachTurn (was {typeName} on {NameOf(part.Kind)}).";
    }

    // Each family's records tick at the same time and have the same part kinds and types in the same order as its first
    // record: a refresh takes the granted record's timing, so a mixed family could tick twice in one turn.
    private static string? FindFamilyProblem(IReadOnlyList<StatusDefinition> statuses)
    {
        foreach (var family in statuses.GroupBy(status => status.FamilyKey))
        {
            var first = family.First();
            var unlike = family.FirstOrDefault(status => !ShapeOf(status).SequenceEqual(ShapeOf(first)));
            if (unlike is not null)
            {
                return $"family '{family.Key}': {unlike.Id}'s parts differ from {first.Id}'s.";
            }

            var otherTiming = family.FirstOrDefault(status => status.ActsAt != first.ActsAt);
            if (otherTiming is not null)
            {
                return $"family '{family.Key}': {otherTiming.Id} acts at another time than {first.Id}.";
            }
        }

        return null;
    }

    private static IEnumerable<(StatusPartKind Kind, DamageType? Type)> ShapeOf(StatusDefinition status) =>
        status.Parts.Select(part => (part.Kind, part.DamageType));

    private static string NameOf(StatusPartKind kind) => ContentJsonOptions.EnumName(kind);
}
