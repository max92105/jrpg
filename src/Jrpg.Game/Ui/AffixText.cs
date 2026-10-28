using Jrpg.Core.Items;
using Jrpg.Core.Stats;

namespace Jrpg.Game.Ui;

/// <summary>
/// A rolled affix and a rolled piece's whole line as the player reads them: "+4 Power", "+2 % Crit chance", and "Rare
/// · +4 Power · +2 % Crit chance". An affix's value is in its stat's bonus unit, as an item's bonus: whole points for
/// a primary, max HP or speed, whole percents for the rest (ADR 0026).
/// </summary>
internal static class AffixText
{
    public static string Of(RolledAffix affix, UiMessages messages)
    {
        var amount = StatKinds.IsHeldInPermille(affix.Stat)
            ? messages.Format(MessageIds.StatPercent, MessageArgument.Number("value", affix.Value))
            : NumberText.Of(affix.Value);
        var signed = affix.Value > 0 ? $"+{amount}" : amount;
        return messages.Format(MessageIds.AffixLine, new MessageArgument("value", signed), new MessageArgument("stat", messages.Text(StatNames.IdOf(affix.Stat))));
    }

    /// <summary>
    /// The piece's rarity, then each affix in roll order (decision 361: it replaces a rolled piece's description).
    /// </summary>
    public static string LineOf(GearPiece piece, UiMessages messages) =>
        string.Join(
            messages.Text(MessageIds.AffixSeparator),
            [messages.Text(RarityNames.IdOf(piece.Rarity)), .. piece.Affixes.Select(affix => Of(affix, messages))]);
}
