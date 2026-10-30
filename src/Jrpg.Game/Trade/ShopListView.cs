using FontStashSharp;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// Draws the Shop's list column: a page column (decision 269) with the stock's rows (icon, name in its rarity's color,
/// how many are owned, the price after the coin), a refused row greyed (decision 370), under the shared selection
/// (decision 252); or a line when the stock is empty.
/// </summary>
internal sealed class ShopListView
{
    /// <summary>How many rows show at once; more scroll.</summary>
    public const int VisibleRows = 13;

    // The list column at 1920 (the slice's arithmetic): the rows sit 28 px inside the column, as the Inventory's and
    // the Forge's, so the pointer stays in it; they start under the room kept for the tabs (48 + 12) and the column
    // titles (36); 52 px rows with a 40 px icon, the name from 96, the owned count ending at 760 and the price at 960,
    // all from the column's left.
    private const int Margin = 28;
    private const int RowsTop = 48 + 12 + 36;
    private const int RowHeight = 52;
    private const int IconSize = 40;
    private const int NameLeft = 96 - Margin;
    private const int OwnedRight = 760 - Margin;
    private const int PriceRight = 960 - Margin;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly GoldAmountView _gold;
    private readonly string _empty;

    /// <param name="text">The empty line.</param>
    /// <param name="ui">The shared textures: icons, the coin, the selected row.</param>
    /// <param name="font">The rows' font, which the list measured them with.</param>
    public ShopListView(ScreenText text, UiTextures ui, SpriteFontBase font)
    {
        _ui = ui;
        _font = font;
        _gold = new GoldAmountView(font, ui);
        _empty = text.Messages.Text(MessageIds.ShopEmpty);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the gradients, the icons and the plain pixel come from.</param>
    /// <param name="list">The stock's rows and the cursor on them.</param>
    /// <param name="area">The column's area; the panel runs the screen's height behind it.</param>
    /// <param name="opacity">The Shop's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, StockList list, Rectangle area, float opacity)
    {
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(area), opacity);
        var firstRow = new Rectangle(area.X + Margin, area.Y + RowsTop, area.Width - (2 * Margin), RowHeight);
        if (list.Cursor is not { } cursor)
        {
            batch.DrawString(_font, _empty, new Vector2(firstRow.X + NameLeft, TextTop(firstRow)), UiTheme.TextSecondary * opacity);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, list.Selection.RowIn(firstRow), opacity);
        var last = Math.Min(list.Rows.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = firstRow with { Y = firstRow.Y + ((index - cursor.FirstVisible) * RowHeight) };
            DrawRow(batch, textures, row, list.Rows[index], opacity);
        }
    }

    // A refused row is greyed whole (decision 370); otherwise a piece's name takes its rarity's color (decision 362).
    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, StockRow entry, float opacity)
    {
        var textColor = (entry.IsAvailable ? UiTheme.TextPrimary : UiTheme.TextDisabled) * opacity;
        var nameColor = entry.IsAvailable && entry.Piece is { } piece ? UiTheme.RarityColorOf(piece.Rarity) * opacity : textColor;
        var textTop = TextTop(row);
        batch.Draw(_ui.Icon(textures, entry.Item.IconFile, IconSize), new Vector2(row.X, row.Y + ((RowHeight - IconSize) / 2)), Color.White * opacity);
        batch.DrawString(_font, entry.Name.Text, new Vector2(row.X + NameLeft, textTop), nameColor);
        batch.DrawString(_font, entry.Owned.Text, new Vector2(row.X + OwnedRight - entry.Owned.Width, textTop), textColor);
        var priceLeft = row.X + PriceRight - _gold.WidthOf(entry.Price);
        _gold.Draw(batch, textures, new GoldAmountView.Place(entry.Price, new Vector2(priceLeft, textTop), entry.IsAvailable ? UiTheme.TextPrimary : UiTheme.TextDisabled), opacity);
    }

    private float TextTop(Rectangle row) => row.Y + ((RowHeight - _font.LineHeight) / 2f);
}
