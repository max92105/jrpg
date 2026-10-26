using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use.</summary>
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
        [.. EquipmentSheet.Slots.Select(slot => messages.Text(EquipmentSheet.LabelIdOf(slot)))],
        messages.Text(MessageIds.SlotEmpty));
}
