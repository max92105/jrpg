using Jrpg.Core.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Dialogue;

/// <summary>
/// The message for a give-item step, saying what was really added and what didn't fit (ADR 0006, developer decision 55).
/// Built once when the step is shown.
/// </summary>
internal static class FoundItemText
{
    public static string Of(ItemReceipt receipt, UiMessages messages)
    {
        var lost = receipt.Lost > 0
            ? messages.Format(MessageIds.ItemFoundLost, MessageArgument.Number("lost", receipt.Lost))
            : null;
        if (receipt.Added == 0)
        {
            // A full stack takes nothing, so only the loss is worth saying.
            return lost ?? string.Empty;
        }

        var found = receipt.Added == 1
            ? messages.Format(MessageIds.ItemFound, new MessageArgument("item", receipt.Item.Name))
            : messages.Format(
                MessageIds.ItemFoundMany,
                new MessageArgument("item", receipt.Item.Name),
                MessageArgument.Number("count", receipt.Added));
        return lost is null ? found : $"{found} {lost}";
    }
}
