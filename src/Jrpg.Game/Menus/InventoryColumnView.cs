using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Inventory page's list column: the category rail, then on a page column beside it the category's name, the
/// column titles and the list with the selected item's detail strip.
/// </summary>
internal sealed class InventoryColumnView
{
    // Placeholder layout (decision 82): a 28 x 20 margin, a heading title, column titles over a hairline, then the
    // list.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int LabelPixels = 18;
    private const int ColumnTitlesTop = 60;
    private const int RowsTop = ColumnTitlesTop + ColumnTitles.Height;

    // The list starts 12 px right of the category rail (placeholder).
    private const int RailGap = 12;

    private readonly CategoryRailView _railView;
    private readonly InventoryListView _listView;
    private readonly ColumnPanelView _listPanel = new(UiTheme.PageColumn);
    private readonly SpriteFontBase _headingFont;
    private readonly ColumnTitles _columnTitles;
    private readonly string[] _categoryNames;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    /// <param name="bodyFont">The rows' font, which the page measures their quantities with.</param>
    /// <param name="categories">The rail's tabs, whose names head the list.</param>
    public InventoryColumnView(ScreenText text, UiTextures ui, SpriteFontBase bodyFont, IReadOnlyList<CategoryTab> categories)
    {
        _railView = new CategoryRailView(ui);
        _listView = new InventoryListView(ui, bodyFont, text.Messages);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _columnTitles = new ColumnTitles(
            text.Fonts.FontAt(LabelPixels),
            ui,
            [
                ColumnTitle.FromLeft(text.Messages.Text(MessageIds.InventoryColumnItem), InventoryListView.NameLeft),
                ColumnTitle.FromRight(text.Messages.Text(MessageIds.InventoryColumnQuantity), InventoryListView.QuantityInset),
            ]);
        _categoryNames = [.. categories.Select(tab => text.Messages.Text(tab.LabelId))];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the gradients and the plain pixel come from.</param>
    /// <param name="place">What the column shows and where.</param>
    /// <param name="opacity">The page's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var area = place.Area;
        var listColumn = area with { X = area.X + CategoryRailView.Width, Width = area.Width - CategoryRailView.Width };
        _listPanel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(listColumn), opacity);
        _railView.Draw(batch, textures, new CategoryRailView.Place(place.Rail, area), opacity);
        var left = area.X + CategoryRailView.Width + RailGap + Margin;
        var top = area.Y + TopMargin;
        var width = area.Right - Margin - left;
        batch.DrawString(_headingFont, _categoryNames[place.Rail.Selected], new Vector2(left, top), UiTheme.TextPrimary * opacity);
        _columnTitles.Draw(batch, textures, new Rectangle(left, top + ColumnTitlesTop, width, 0), opacity);

        var rows = new Rectangle(left, top + RowsTop, width, InventoryListView.Height);
        var list = new InventoryListView.Place(place.Rows, place.Cursor, place.ListHasCursor, place.Selection, rows, place.Detail);
        _listView.Draw(batch, textures, list, opacity);
    }

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Rail">The categories, and whether the rail has the cursor.</param>
    /// <param name="Rows">The category's items, in order.</param>
    /// <param name="Cursor">The list's cursor; null when the category is empty.</param>
    /// <param name="ListHasCursor">Whether the list shows its pointer and full-strength selection.</param>
    /// <param name="Selection">Where the list's highlight has glided to.</param>
    /// <param name="Area">The column's area on the page.</param>
    /// <param name="Detail">The text under the selected row, or null.</param>
    public readonly record struct Place(
        CategoryRail Rail,
        IReadOnlyList<InventoryRow> Rows,
        ListCursor? Cursor,
        bool ListHasCursor,
        SelectionMotion Selection,
        Rectangle Area,
        string? Detail);
}
