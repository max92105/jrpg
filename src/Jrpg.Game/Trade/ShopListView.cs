using FontStashSharp;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// Draws the Shop's list column: a page column (decision 269) with the Buy and Sell tabs above the shown tab's rows
/// (icon, name in its rarity's color, how many are owned, the price after the coin), a refused row greyed (decision
/// 370), under the shared selection (decision 252); or a line when the tab has nothing to show. While the cursor is on
/// the tabs the chosen tab carries the pointer and the row below stays chosen, dimmed.
/// </summary>
internal sealed class ShopListView
{
    /// <summary>How many rows show at once; more scroll.</summary>
    public const int VisibleRows = 13;

    // The list column at 1920 (the slice's arithmetic): the rows sit 28 px inside the column, as the Inventory's and
    // the Forge's, so the pointer stays in it; the two tabs (the victory card's pointed tab) take 48 px plus a 12 px
    // gap at the top, the column titles 36 more; then 52 px rows with a 40 px icon, the name from 96, the owned count
    // ending at 760 and the price at 960, all from the column's left.
    private const int Margin = 28;
    private const int TabsHeight = 48;
    private const int TabGap = 24;
    private const int RowsTop = TabsHeight + 12 + 36;
    private const int RowHeight = 52;
    private const int IconSize = 40;
    private const int NameLeft = 96 - Margin;
    private const int OwnedRight = 760 - Margin;
    private const int PriceRight = 960 - Margin;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly GoldAmountView _gold;
    private readonly MeasuredText[] _tabLabels;
    private readonly string[] _emptyLines;

    /// <param name="text">The tabs' labels and the empty lines.</param>
    /// <param name="ui">The shared textures: icons, the coin, the selected row, the tabs.</param>
    /// <param name="font">The rows' font, which the lists measured them with.</param>
    public ShopListView(ScreenText text, UiTextures ui, SpriteFontBase font)
    {
        _ui = ui;
        _font = font;
        _gold = new GoldAmountView(font, ui);
        _tabLabels = [MeasuredText.Of(font, text.Messages.Text(MessageIds.ShopBuy)), MeasuredText.Of(font, text.Messages.Text(MessageIds.ShopSell))];
        _emptyLines = [text.Messages.Text(MessageIds.ShopEmpty), text.Messages.Text(MessageIds.ShopNothingToSell)];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the gradients, the icons and the plain pixel come from.</param>
    /// <param name="place">The tab shown, where the cursor is, and the shown tab's rows.</param>
    /// <param name="area">The column's area; the panel runs the screen's height behind it.</param>
    /// <param name="opacity">The Shop's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, Rectangle area, float opacity)
    {
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(area), opacity);
        var inset = new Rectangle(area.X + Margin, area.Y, area.Width - (2 * Margin), area.Height);
        DrawTabs(batch, textures, place, inset, opacity);
        var firstRow = new Rectangle(inset.X, inset.Y + RowsTop, inset.Width, RowHeight);
        if (place.Selection.Cursor is not { } cursor)
        {
            batch.DrawString(_font, _emptyLines[(int)place.Tab], new Vector2(firstRow.X + NameLeft, TextTop(firstRow)), UiTheme.TextSecondary * opacity);
            return;
        }

        DrawRowSelection(batch, textures, place, firstRow, opacity);
        var last = Math.Min(place.Rows.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = firstRow with { Y = firstRow.Y + ((index - cursor.FirstVisible) * RowHeight) };
            DrawRow(batch, textures, row, place.Rows[index], opacity);
        }
    }

    // Two pointed tabs side by side; the shown one lit, the other dimmed; the pointer on the chosen one while the
    // cursor is there.
    private void DrawTabs(SpriteBatch batch, TextureLibrary textures, Place place, Rectangle inset, float opacity)
    {
        for (var index = 0; index < _tabLabels.Length; index++)
        {
            var isShown = index == (int)place.Tab;
            var tab = TabArea(inset, index);
            var tabOpacity = opacity * (isShown ? 1 : UiTheme.DimmedSelectionOpacity);
            batch.Draw(_ui.CardHeader(textures), tab, Color.White * tabOpacity);
            if (isShown && place.IsOnTabs)
            {
                _ui.DrawGlidingSelection(batch, textures, place.TabPointer.At(tab), opacity);
            }

            var label = _tabLabels[index];
            batch.DrawString(_font, label.Text, new Vector2(tab.Center.X - (label.Width / 2f), tab.Center.Y - (_font.LineHeight / 2f)), UiTheme.TextPrimary * tabOpacity);
        }
    }

    private static Rectangle TabArea(Rectangle inset, int index) =>
        new(inset.X + (index * (PointedBarShape.CardHeaderWidth + TabGap)), inset.Y + ((TabsHeight - PointedBarShape.CardHeaderHeight) / 2), PointedBarShape.CardHeaderWidth, PointedBarShape.CardHeaderHeight);

    // The pointer marks the row while the list has the cursor; while the tabs have it, the row stays chosen, dimmed.
    private void DrawRowSelection(SpriteBatch batch, TextureLibrary textures, Place place, Rectangle firstRow, float opacity)
    {
        var selected = place.Selection.Highlight.RowIn(firstRow);
        if (place.IsOnTabs)
        {
            _ui.DrawSelectedRow(batch, textures, selected.Area, opacity * UiTheme.DimmedSelectionOpacity);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, selected, opacity);
    }

    // A refused row is greyed whole (decision 370); otherwise a piece's name takes its rarity's color (decision 362).
    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, TradeRow entry, float opacity)
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

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Tab">The tab shown, whose rows these are.</param>
    /// <param name="IsOnTabs">Whether the cursor is on the tabs rather than in the list.</param>
    /// <param name="TabPointer">The pointer bobbing on the chosen tab.</param>
    /// <param name="Rows">The shown tab's rows.</param>
    /// <param name="Selection">The cursor over them and the gliding highlight; no cursor when the tab is empty.</param>
    public readonly record struct Place(ShopTab Tab, bool IsOnTabs, SelectionMotion TabPointer, IReadOnlyList<TradeRow> Rows, RowSelection Selection);
}
