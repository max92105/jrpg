using FontStashSharp;
using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Turns Core's quotes into the rows the Forge shows: the action's name, its materials measured for right-aligning, and
/// the refusal's line. Built once with the Forge; the choice formats through it when it opens.
/// </summary>
internal sealed class ForgeActionFormat
{
    private readonly UiMessages _messages;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private readonly SpriteFontBase _bodyFont;

    /// <param name="messages">The actions' names, the materials' lines and the refusals'.</param>
    /// <param name="items">The materials' names.</param>
    /// <param name="bodyFont">Measures the materials once, when a row is built.</param>
    public ForgeActionFormat(UiMessages messages, IReadOnlyDictionary<string, ItemDefinition> items, SpriteFontBase bodyFont)
    {
        _messages = messages;
        _items = items;
        _bodyFont = bodyFont;
    }

    public ForgeActionRow RowOf(ForgeAction action, ForgeQuote quote) =>
        new(
            action,
            _messages.Text(LabelIdOf(action)),
            MeasuredText.Of(_bodyFont, MaterialListText.Of(quote.Materials, _items, _messages)),
            quote.Refusal is { } refusal ? _messages.Text(ForgeRefusalNames.IdOf(refusal)) : null);

    private static string LabelIdOf(ForgeAction action) => action switch
    {
        ForgeAction.Salvage => MessageIds.ForgeSalvage,
        ForgeAction.Upgrade => MessageIds.ForgeUpgrade,
        ForgeAction.Reroll => MessageIds.ForgeReroll,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "An action with no name in ui.json."),
    };
}
