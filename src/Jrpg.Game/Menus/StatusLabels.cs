using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use. The captions are in caps, like
/// the reference's small labels ("HP", "LV"), uppercased once here.
/// </summary>
/// <param name="Progress">Level, experience, and experience to the next level.</param>
/// <param name="MaxLevel">Shown instead of the experience to the next level at the highest level.</param>
/// <param name="Stats">HP, Attack, Defense.</param>
/// <param name="Speed">The speed caption, beside HP.</param>
/// <param name="Slots">Weapon, Armor.</param>
/// <param name="Empty">Shown for a slot with nothing in it.</param>
/// <param name="Messages">Where <see cref="LastingAmountText"/> is formatted from.</param>
internal sealed record StatusLabels(
    string[] Progress,
    string MaxLevel,
    string[] Stats,
    string Speed,
    string[] Slots,
    string Empty,
    UiMessages Messages)
{
    public static StatusLabels From(UiMessages messages)
    {
        string Caps(string messageId) => messages.Text(messageId).ToUpperInvariant();
        return new(
            [Caps(MessageIds.StatusLevelShort), Caps(MessageIds.StatusExp), Caps(MessageIds.StatusNextLevel)],
            messages.Text(MessageIds.StatusMaxLevel),
            [Caps(MessageIds.StatHp), Caps(MessageIds.StatAttack), Caps(MessageIds.StatDefense)],
            Caps(MessageIds.StatusSpeed),
            [Caps(MessageIds.SlotWeapon), Caps(MessageIds.SlotArmor)],
            messages.Text(MessageIds.SlotEmpty),
            messages);
    }

    /// <summary>A lasting resource's amount out of its max, as "2/3".</summary>
    public string LastingAmountText(int amount, int max) =>
        Messages.Format(MessageIds.StatusLastingAmount, MessageArgument.Number("amount", amount), MessageArgument.Number("max", max));
}
