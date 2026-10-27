using System.Collections.Immutable;
using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What differs between two of a member's numbers, in the order the Equipment page lists it: Attack and Defense first,
/// while the battle still uses them (decision 337), then every stat kind whose value differs, in <see cref="StatKind"/>
/// order. Built when what is compared changes, never per frame.
/// </summary>
internal sealed record StatComparison(ImmutableArray<StatComparison.Change> Changes)
{
    /// <param name="before">The sheet and battle stats as they are, or without the item.</param>
    /// <param name="after">The sheet and battle stats with the change made.</param>
    public static StatComparison Between(Numbers before, Numbers after)
    {
        var changes = ImmutableArray.CreateBuilder<Change>();
        AddIfDifferent(changes, new Change(MessageIds.StatAttack, Kind: null, before.Stats.Attack, after.Stats.Attack));
        AddIfDifferent(changes, new Change(MessageIds.StatDefense, Kind: null, before.Stats.Defense, after.Stats.Defense));
        foreach (var kind in Enum.GetValues<StatKind>())
        {
            AddIfDifferent(changes, new Change(StatNames.IdOf(kind), kind, before.Sheet.ValueOf(kind), after.Sheet.ValueOf(kind)));
        }

        return new StatComparison(changes.ToImmutable());
    }

    // A percent stat is compared as the screen writes it, a whole percent (ADR 0026): a change under 1 % would read
    // "2 % ▸ 2 %", so it isn't listed.
    private static void AddIfDifferent(ImmutableArray<Change>.Builder changes, Change change)
    {
        if (Shown(change, change.Before) != Shown(change, change.After))
        {
            changes.Add(change);
        }
    }

    private static int Shown(Change change, int value) =>
        change.Kind is { } kind && StatKinds.IsHeldInPermille(kind) ? StatFormula.WholePercentOf(value) : value;

    /// <summary>A member's numbers to compare: the sheet, and the attack and defense the battle still reads.</summary>
    public readonly record struct Numbers(StatSheet Sheet, CombatStats Stats);

    /// <summary>One number that differs.</summary>
    /// <param name="NameId">Its name in ui.json.</param>
    /// <param name="Kind">Its stat kind, which says how to write it; null for Attack and Defense.</param>
    /// <param name="Before">As the sheet (<see cref="StatSheet.ValueOf"/>) or the battle stats give it.</param>
    /// <param name="After">The same, with the change made.</param>
    public readonly record struct Change(string NameId, StatKind? Kind, int Before, int After);
}
