using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Status page's fixed text, in the order <see cref="StatusSheet"/>'s arrays use, with its captions already in
/// caps, and the messages <see cref="ValueOfMax"/> formats with.
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
        [CapsOf(messages, MessageIds.StatusLevel), CapsOf(messages, MessageIds.StatusExp), CapsOf(messages, MessageIds.StatusNextLevel)],
        messages.Text(MessageIds.StatusMaxLevel),
        [CapsOf(messages, MessageIds.StatHp), CapsOf(messages, MessageIds.StatAttack), CapsOf(messages, MessageIds.StatDefense)],
        CapsOf(messages, MessageIds.StatSpeed),
        [.. EquipmentSheet.Slots.Select(slot => CapsOf(messages, EquipmentSheet.LabelIdOf(slot)))],
        messages.Text(MessageIds.SlotEmpty),
        messages);

    // A caption as the reference writes its labels: in caps, drawn spaced (docs/ui-reference).
    private static string CapsOf(UiMessages messages, string id) => messages.Text(id).ToUpperInvariant();

    /// <summary>An amount out of its max, written as HP is (decision 110).</summary>
    public string ValueOfMax(int value, int max) =>
        Messages.Format(MessageIds.StatValueOfMax, MessageArgument.Number("value", value), MessageArgument.Number("max", max));
}
