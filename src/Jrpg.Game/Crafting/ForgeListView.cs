using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Draws the Forge's list column: a page column (decision 269) with the column title, the owned pieces (icon, name in
/// its rarity's color, decision 362) under the shared selection (decision 252), or a line when none is owned.
/// </summary>
internal sealed class ForgeListView
{
    /// <summary>How many rows show at once; more pieces scroll.</summary>
    public const int VisibleRows = 12;

    // The Inventory list's shape (decision 82): a 28 x 20 margin, the column title over a hairline, then 52 px rows
    // with a 32 px icon in a 48 px column; the empty line where the first row would be.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int LabelPixels = 18;
    private const int RowHeight = 52;
    private const int RowInset = 10;
    private const int IconColumn = 48;
    private const int RowIconSize = 32;
    private const int NameLeft = RowInset + IconColumn;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _bodyFont;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly ColumnTitles _columnTitles;
    private readonly string _empty;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: icons, the selected row, the hairline.</param>
    /// <param name="bodyFont">The rows' font.</param>
    public ForgeListView(ScreenText text, UiTextures ui, SpriteFontBase bodyFont)
    {
        _ui = ui;
        _bodyFont = bodyFont;
        _columnTitles = new ColumnTitles(text.Fonts.FontAt(LabelPixels), ui, [ColumnTitle.FromLeft(text.Messages.Text(MessageIds.InventoryColumnItem), NameLeft)]);
        _empty = text.Messages.Text(MessageIds.ForgeEmpty);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the gradients and the plain pixel come from.</param>
    /// <param name="list">The owned pieces and the cursor on them.</param>
    /// <param name="area">The column's area; the panel runs the screen's height behind it.</param>
    /// <param name="opacity">The Forge's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ForgeList list, Rectangle area, float opacity)
    {
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(area), opacity);
        var left = area.X + Margin;
        var width = area.Right - Margin - left;
        _columnTitles.Draw(batch, textures, new Rectangle(left, area.Y + TopMargin, width, 0), opacity);
        var firstRow = new Rectangle(left, area.Y + TopMargin + ColumnTitles.Height, width, RowHeight);
        if (list.Cursor is not { } cursor)
        {
            var textTop = firstRow.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
            batch.DrawString(_bodyFont, _empty, new Vector2(firstRow.X + NameLeft, textTop), UiTheme.TextSecondary * opacity);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, list.Selection.RowIn(firstRow), opacity);
        var last = Math.Min(list.Rows.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = firstRow with { Y = firstRow.Y + ((index - cursor.FirstVisible) * RowHeight) };
            DrawRow(batch, textures, row, list.Rows[index].Piece, opacity);
        }
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, GearPiece piece, float opacity)
    {
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var iconTop = row.Y + ((RowHeight - RowIconSize) / 2);
        batch.Draw(_ui.Icon(textures, piece.Base.IconFile, RowIconSize), new Vector2(row.X + RowInset, iconTop), Color.White * opacity);
        batch.DrawString(_bodyFont, piece.Base.Name, new Vector2(row.X + NameLeft, textTop), UiTheme.RarityColorOf(piece.Rarity) * opacity);
    }
}
