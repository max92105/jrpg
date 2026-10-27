using System.Collections.Immutable;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Menus;

/// <summary>
/// A <see cref="StatComparison"/> as the Equipment page writes it, formatted once: each line's name, value before and
/// after, the after value colored by whether it goes up or down; past <see cref="MostLines"/>, how many more there are.
/// </summary>
internal sealed record StatComparisonText(ImmutableArray<StatComparisonText.Line> Lines, string? More)
{
    /// <summary>The most lines the page shows, two columns of four (placeholder: no item here changes more).</summary>
    public const int MostLines = 8;

    public static StatComparisonText Of(StatComparison comparison, UiMessages messages)
    {
        var changes = comparison.Changes;
        var lines = changes.Take(MostLines).Select(change => new Line(
            messages.Text(change.NameId),
            ValueText(change, change.Before, messages),
            ValueText(change, change.After, messages),
            UiTheme.PreviewColorOf(change.After - change.Before)));
        var hidden = changes.Length - MostLines;
        var more = hidden > 0 ? messages.Format(MessageIds.EquipmentMoreChanges, MessageArgument.Number("count", hidden)) : null;
        return new StatComparisonText([.. lines], more);
    }

    private static string ValueText(StatComparison.Change change, int value, UiMessages messages) =>
        change.Kind is { } kind ? StatText.ValueOf(kind, value, messages) : NumberText.Of(value);

    /// <summary>One line: the stat's name, its value before and after, and the after value's color.</summary>
    public sealed record Line(string Label, string Before, string After, Color Color);
}
