using System.Collections.Immutable;
using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What differs between two of a member's sheets, in the order the Equipment page lists it: Weapon damage first
/// (decision 337), then every stat kind whose value differs, in <see cref="StatKind"/> order. Built when what is
/// compared changes, never per frame.
/// </summary>
internal sealed record StatComparison(ImmutableArray<StatComparison.Change> Changes)
{
    /// <param name="before">The sheet as it is, or without the item.</param>
    /// <param name="after">The sheet with the change made.</param>
    public static StatComparison Between(StatSheet before, StatSheet after)
    {
        var changes = ImmutableArray.CreateBuilder<Change>();
        AddIfDifferent(changes, new Change(MessageIds.StatWeaponDamage, Kind: null, StatFormula.WeaponDamage(before), StatFormula.WeaponDamage(after)));
        foreach (var kind in Enum.GetValues<StatKind>())
        {
            AddIfDifferent(changes, new Change(StatNames.IdOf(kind), kind, before.ValueOf(kind), after.ValueOf(kind)));
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

    /// <summary>One number that differs.</summary>
    /// <param name="NameId">Its name in ui.json.</param>
    /// <param name="Kind">Its stat kind, which says how to write it; null for Weapon damage.</param>
    /// <param name="Before">As the sheet gives it: <see cref="StatSheet.ValueOf"/>, or the weapon damage.</param>
    /// <param name="After">The same, with the change made.</param>
    public readonly record struct Change(string NameId, StatKind? Kind, int Before, int After);
}
