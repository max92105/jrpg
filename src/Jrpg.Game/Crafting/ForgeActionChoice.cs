using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>
/// The actions open on one piece, with their cursor and gliding highlight. Each is quoted by Core when the choice
/// opens, so a refused one is shown greyed with its reason (decision 370); the choice decides nothing itself. A greyed
/// action can still be highlighted, so its reason can be read.
/// </summary>
internal sealed class ForgeActionChoice
{
    private readonly ForgeActionRow[] _rows;
    private readonly ListCursor _cursor;

    /// <param name="piece">The owned piece the actions are quoted for.</param>
    /// <param name="forge">Core's Forge, which quotes each action.</param>
    /// <param name="format">Turns the quotes into rows.</param>
    public ForgeActionChoice(GearPiece piece, Forge forge, ForgeActionFormat format)
    {
        _rows =
        [
            format.RowOf(ForgeAction.Salvage, forge.QuoteSalvage(piece)),
            format.RowOf(ForgeAction.Upgrade, forge.QuoteUpgrade(piece)),
            format.RowOf(ForgeAction.Reroll, forge.QuoteReroll(piece)),
        ];
        _cursor = new ListCursor(_rows.Length, _ => true);
        Selection.JumpTo(_cursor);
    }

    /// <summary>In the order shown: Salvage, Upgrade, Reroll.</summary>
    public IReadOnlyList<ForgeActionRow> Rows => _rows;

    public ForgeActionRow Selected => _rows[_cursor.Selected];

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Selection { get; } = new();

    public void Advance(TimeSpan delta) => Selection.Advance(delta);

    public void MoveUp()
    {
        _cursor.MoveUp();
        Selection.Follow(_cursor);
    }

    public void MoveDown()
    {
        _cursor.MoveDown();
        Selection.Follow(_cursor);
    }
}
