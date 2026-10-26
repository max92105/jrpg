using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use.</summary>
/// <param name="Progress">Level, experience, and experience to the next level.</param>
/// <param name="MaxLevel">Shown instead of the experience to the next level at the highest level.</param>
/// <param name="Stats">HP, Attack, Defense.</param>
/// <param name="Slots">Weapon, Armor.</param>
/// <param name="Empty">Shown for a slot with nothing in it.</param>
internal sealed record StatusLabels(
    string[] Progress,
    string MaxLevel,
    string[] Stats,
    string[] Slots,
    string Empty)
{
    public static StatusLabels From(UiMessages messages) => new(
        [messages.Text(MessageIds.StatusLevel), messages.Text(MessageIds.StatusExp), messages.Text(MessageIds.StatusNextLevel)],
        messages.Text(MessageIds.StatusMaxLevel),
        [messages.Text(MessageIds.StatHp), messages.Text(MessageIds.StatAttack), messages.Text(MessageIds.StatDefense)],
        [messages.Text(MessageIds.SlotWeapon), messages.Text(MessageIds.SlotArmor)],
        messages.Text(MessageIds.SlotEmpty));
}
