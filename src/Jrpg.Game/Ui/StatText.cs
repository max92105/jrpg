using Jrpg.Core.Stats;

namespace Jrpg.Game.Ui;

/// <summary>
/// A stat's value as the player reads it, the same on every screen: a percent stat, held in permille, as a whole
/// percent rounded down (ADR 0026), "12 %"; a primary, max HP or speed as its number, "250".
/// </summary>
internal static class StatText
{
    /// <param name="kind">Which stat, which says whether the value is a percent.</param>
    /// <param name="value">As <see cref="StatSheet.ValueOf"/> gives it: permille for a percent stat.</param>
    /// <param name="messages">The percent format.</param>
    public static string ValueOf(StatKind kind, int value, UiMessages messages) => StatKinds.IsHeldInPermille(kind)
        ? messages.Format(MessageIds.StatPercent, MessageArgument.Number("value", StatFormula.WholePercentOf(value)))
        : NumberText.Of(value);
}
