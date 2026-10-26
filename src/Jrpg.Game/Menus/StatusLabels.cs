using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use, and the messages
/// <see cref="ValueOfMax"/> formats with.
/// </summary>
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
        messages.Text(MessageIds.StatSpeed),
        [.. EquipmentSheet.Slots.Select(slot => messages.Text(EquipmentSheet.LabelIdOf(slot)))],
        messages.Text(MessageIds.SlotEmpty),
        messages);

    /// <summary>An amount out of its max, written as HP is (decision 110).</summary>
    public string ValueOfMax(int value, int max) =>
        Messages.Format(MessageIds.StatValueOfMax, MessageArgument.Number("value", value), MessageArgument.Number("max", max));
}
