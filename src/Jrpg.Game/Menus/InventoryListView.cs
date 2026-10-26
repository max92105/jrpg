using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Inventory page's list: spaced-caps column titles, the visible rows with the selected one marked, and the
/// selected item's description (or the last use's notice) in a dark strip right under its row, as in docs/ui-reference.
/// An empty category shows its empty line instead of rows.
/// </summary>
internal sealed class InventoryListView
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 10;

    /// <summary>The list's height: the column titles, then the visible rows.</summary>
    public const int Height = ColumnTitles.Height + (VisibleRows * RowHeight);

    // Placeholder layout (decision 82): 18 px column titles; 52 px rows of 26 px text, 10 px in, with a 32 px icon in a
    // 48 px column and the quantity 16 px from the right; the strip's 22 px text 10 px from its top and bottom.
    private const int LabelPixels = 18;
    private const int BodyPixels = 26;
    private const int DetailPixels = 22;
    private const int RowHeight = 52;
    private const int RowInset = 10;
    private const int IconColumn = 48;
    private const int NameLeft = RowInset + IconColumn;
    private const int QuantityInset = 16;
    private const int RowIconSize = 32;
    private const int StripPadding = 10;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _detailFont;
    private readonly ColumnTitles _columnTitles;
    private readonly MeasuredText _empty;

    /// <param name="text">The fonts, the column titles and the empty line.</param>
    /// <param name="ui">The selection, hairline and icon textures.</param>
    public InventoryListView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _detailFont = text.Fonts.FontAt(DetailPixels);
        _columnTitles = new ColumnTitles(
            text.Fonts.FontAt(LabelPixels),
            ui,
            [
                ColumnTitle.FromLeft(text.Messages.Text(MessageIds.InventoryColumnItem), NameLeft),
                ColumnTitle.FromRight(text.Messages.Text(MessageIds.InventoryColumnQuantity), QuantityInset),
            ]);
        _empty = MeasuredText.Of(_bodyFont, text.Messages.Text(MessageIds.InventoryEmpty));
    }

    private int StripHeight => _detailFont.LineHeight + (2 * StripPadding);

    /// <summary>The body font's width of <paramref name="text"/>, for a row's quantity.</summary>
    public MeasuredText MeasureQuantity(string text) => MeasuredText.Of(_bodyFont, text);

    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        _columnTitles.Draw(batch, textures, place.Area, opacity);
        var firstRow = new Rectangle(place.Area.X, place.Area.Y + ColumnTitles.Height, place.Area.Width, RowHeight);
        var listBottom = firstRow.Y + (VisibleRows * RowHeight);
        if (place.Cursor is not { } cursor)
        {
            var emptyLeft = firstRow.X + ((firstRow.Width - _empty.Width) / 2f);
            var emptyTop = firstRow.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
            batch.DrawString(_bodyFont, _empty.Text, new Vector2(emptyLeft, emptyTop), UiTheme.TextSecondary * opacity);
            DrawDetail(batch, textures.Pixel, StripFor(place, firstRow, listBottom), opacity);
            return;
        }

        var band = place.Selection.BandOver(firstRow);
        DrawSelection(batch, textures, new SelectionBand(band, place.HasCursor, place.Selection.PointerAt(band)), opacity);
        var last = Math.Min(place.Rows.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = firstRow with { Y = firstRow.Y + ((index - cursor.FirstVisible) * RowHeight) };
            DrawRow(batch, textures, row, place.Rows[index], opacity);
        }

        DrawDetail(batch, textures.Pixel, StripFor(place, band, listBottom), opacity);
    }

    // The list with the cursor marks the row with the pointer; with the rail, the row stays chosen, dimmed.
    private void DrawSelection(SpriteBatch batch, TextureLibrary textures, SelectionBand band, float opacity)
    {
        if (!band.HasCursor)
        {
            _ui.DrawSelectedRow(batch, textures, band.Area, opacity * UiTheme.DimmedSelectionOpacity);
            return;
        }

        _ui.DrawSelectedRow(batch, textures, band.Area, opacity);
        MenuPointer.Draw(batch, textures.Pixel, band.Pointer, opacity);
    }

    // Under the selected row, or above it near the list's bottom, over the rows there. A notice reads brighter than a
    // description; the strip dims with the row while the rail has the cursor.
    private DetailStrip StripFor(Place place, Rectangle band, int listBottom)
    {
        var selected = place.Cursor is { } cursor ? place.Rows[cursor.Selected] : null;
        var area = band with { Y = ItemDetailLine.TopFor(band, listBottom, StripHeight), Height = StripHeight };
        var color = place.Detail.Notice is null ? UiTheme.TextSecondary : UiTheme.TextPrimary;
        return new DetailStrip(area, place.Detail.TextFor(selected), color, place.HasCursor ? 1 : UiTheme.DimmedSelectionOpacity);
    }

    // In the battle's description color, so both tips match.
    private void DrawDetail(SpriteBatch batch, Texture2D pixel, DetailStrip strip, float opacity)
    {
        if (strip.Text.Length == 0)
        {
            return;
        }

        var shown = opacity * strip.Opacity;
        batch.Draw(pixel, strip.Area, UiTheme.CommandHintFill * shown);
        batch.DrawString(_detailFont, strip.Text, new Vector2(strip.Area.X + NameLeft, strip.Area.Y + StripPadding), strip.Color * shown);
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, InventoryRow item, float opacity)
    {
        var textColor = (item.IsUsable ? UiTheme.TextPrimary : UiTheme.TextDisabled) * opacity;
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var iconTop = row.Y + ((RowHeight - RowIconSize) / 2);
        batch.Draw(_ui.Icon(textures, item.IconFile, RowIconSize), new Vector2(row.X + RowInset, iconTop), Color.White * opacity);
        batch.DrawString(_bodyFont, item.Name, new Vector2(row.X + NameLeft, textTop), textColor);
        var quantityLeft = row.Right - QuantityInset - item.Quantity.Width;
        batch.DrawString(_bodyFont, item.Quantity.Text, new Vector2(quantityLeft, textTop), textColor);
    }

    /// <summary>Where the list goes and what it shows.</summary>
    /// <param name="Area">The list's box: the column titles at its top, then the rows, each as wide as it.</param>
    /// <param name="Rows">Every row of the category, the hidden ones included.</param>
    /// <param name="Cursor">The selected row and the first visible one; null when the category is empty.</param>
    /// <param name="Selection">Where the selected row's band is, by its row in the visible window.</param>
    /// <param name="HasCursor">Whether the list has the cursor, rather than the category rail.</param>
    /// <param name="Detail">The strip's notice, if the last use left one.</param>
    public readonly record struct Place(
        Rectangle Area, IReadOnlyList<InventoryRow> Rows, ListCursor? Cursor, SelectionGlide Selection, bool HasCursor, ItemDetailLine Detail);

    // The selected row's band where the glide has it, whether the list has the cursor, and where the pointer goes.
    private readonly record struct SelectionBand(Rectangle Area, bool HasCursor, Vector2 Pointer);

    // The strip's place, its text and color, and how much the rail's cursor dims it.
    private readonly record struct DetailStrip(Rectangle Area, string Text, Color Color, float Opacity);
}
