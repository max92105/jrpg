using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use.</summary>
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
    public static StatusLabels From(UiMessages messages) => new(
        [messages.Text(MessageIds.StatusLevel), messages.Text(MessageIds.StatusExp), messages.Text(MessageIds.StatusNextLevel)],
        messages.Text(MessageIds.StatusMaxLevel),
        [messages.Text(MessageIds.StatHp), messages.Text(MessageIds.StatAttack), messages.Text(MessageIds.StatDefense)],
        messages.Text(MessageIds.StatusSpeed),
        [messages.Text(MessageIds.SlotWeapon), messages.Text(MessageIds.SlotArmor)],
        messages.Text(MessageIds.SlotEmpty),
        messages);

    /// <summary>A lasting resource's amount out of its max, as "2/3".</summary>
    public string LastingAmountText(int amount, int max) =>
        Messages.Format(MessageIds.StatusLastingAmount, MessageArgument.Number("amount", amount), MessageArgument.Number("max", max));
}
