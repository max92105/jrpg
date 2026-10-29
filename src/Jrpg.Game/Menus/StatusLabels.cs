using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use. The captions are in caps, like
/// the reference's small labels ("HP", "LV"), uppercased once here.
/// </summary>
/// <param name="Progress">Level, experience, and experience to the next level.</param>
/// <param name="MaxLevel">Shown instead of the experience to the next level at the highest level.</param>
/// <param name="Attributes">The five primaries' names, in <see cref="StatusSheet.Attributes"/>' order.</param>
/// <param name="Combat">The Combat section's labels, in <see cref="StatusSheet.Combat"/>'s order.</param>
/// <param name="Resistances">Each damage type's short name, in <see cref="DamageType"/> order.</param>
/// <param name="Affixes">
/// Each stat only affixes give, in <see cref="StatKinds.AffixOnly"/>' order: its name, but Armor penetration's short
/// one, which fits its cell.
/// </param>
/// <param name="Messages">Where <see cref="LastingAmountText"/> is formatted from.</param>
internal sealed record StatusLabels(
    string[] Progress,
    string MaxLevel,
    string[] Attributes,
    string[] Combat,
    string[] Resistances,
    string[] Affixes,
    UiMessages Messages)
{
    public static StatusLabels From(UiMessages messages)
    {
        string Caps(string messageId) => messages.Text(messageId).ToUpperInvariant();
        return new(
            [Caps(MessageIds.StatusLevelShort), Caps(MessageIds.StatusExp), Caps(MessageIds.StatusNextLevel)],
            messages.Text(MessageIds.StatusMaxLevel),
            [.. StatKinds.Primaries.Select(kind => Caps(StatNames.IdOf(kind)))],
            [
                Caps(MessageIds.StatHp), Caps(MessageIds.StatWeaponDamage), Caps(MessageIds.StatusSpeed),
                Caps(MessageIds.StatAccuracy), Caps(MessageIds.StatusCrit), Caps(MessageIds.StatDodge), Caps(MessageIds.StatusCrowdControl),
            ],
            [.. Enum.GetValues<DamageType>().Select(type => Caps(DamageTypeNames.IdOf(type)))],
            [.. StatKinds.AffixOnly.Select(kind => Caps(kind == StatKind.ArmorPenetration ? MessageIds.StatusPenetration : StatNames.IdOf(kind)))],
            messages);
    }

    /// <summary>A lasting resource's amount out of its max, as "2/3".</summary>
    public string LastingAmountText(int amount, int max) =>
        Messages.Format(MessageIds.StatusLastingAmount, MessageArgument.Number("amount", amount), MessageArgument.Number("max", max));
}
