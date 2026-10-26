using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Inventory page's list: the visible rows (icon, name, quantity), the selected row's glow and pointer, and
/// the item's detail strip under it; or a centered line when the category holds nothing.
/// </summary>
internal sealed class InventoryListView
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 10;

    // Placeholder layout (decision 82): 52 px rows with a 32 px icon in a 48 px column and the quantity 16 px from the
    // right; the empty line 24 px below the top.
    private const int RowHeight = 52;
    private const int IconColumn = 48;
    private const int RowIconSize = 32;
    private const int EmptyLineTop = 24;

    // The detail strip's text sits 12 x 6 px inside it (placeholder).
    private const int StripPaddingX = 12;
    private const int StripPaddingY = 6;

    /// <summary>The height of the visible rows, from the list's top.</summary>
    public const int Height = VisibleRows * RowHeight;

    // A row's icon's gap to the list's left edge.
    private const int RowInset = 10;

    /// <summary>Where the name column starts, from the list's left edge; its column title lines up with it.</summary>
    public const int NameLeft = RowInset + IconColumn;

    /// <summary>The quantity column's gap to the list's right edge; its column title lines up with it.</summary>
    public const int QuantityInset = 16;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _bodyFont;
    private readonly MeasuredText _empty;
    private readonly int _stripHeight;

    /// <param name="ui">The shared textures: icons, the selected row.</param>
    /// <param name="bodyFont">The page's body font, which its rows were measured with.</param>
    /// <param name="messages">The empty line's text.</param>
    public InventoryListView(UiTextures ui, SpriteFontBase bodyFont, UiMessages messages)
    {
        _ui = ui;
        _bodyFont = bodyFont;
        _empty = MeasuredText.Of(bodyFont, messages.Text(MessageIds.InventoryEmpty));
        _stripHeight = bodyFont.LineHeight + (2 * StripPaddingY);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">The rows, the cursor on them, and where the list goes.</param>
    /// <param name="opacity">The page's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var area = place.Area;
        if (place.Cursor is not { } cursor)
        {
            DrawEmpty(batch, textures, place, opacity);
            return;
        }

        DrawSelection(batch, textures, place, opacity);
        var first = cursor.FirstVisible;
        var last = Math.Min(place.Rows.Count, first + VisibleRows);
        for (var index = first; index < last; index++)
        {
            DrawRow(batch, textures, RowArea(area, index - first), place.Rows[index], opacity);
        }

        DrawDetail(batch, textures, place, opacity);
    }

    private static Rectangle RowArea(Rectangle list, int visibleRow) =>
        new(list.X, list.Y + (visibleRow * RowHeight), list.Width, RowHeight);

    // With nothing to select, a use's notice takes the empty line's place.
    private void DrawEmpty(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var area = place.Area;
        if (place.Detail is { } notice)
        {
            DrawStrip(batch, textures, new DetailStrip(notice, StripAt(area, area.Y + EmptyLineTop - StripPaddingY)), opacity);
            return;
        }

        var emptyLeft = area.X + ((area.Width - _empty.Width) / 2f);
        batch.DrawString(_bodyFont, _empty.Text, new Vector2(emptyLeft, area.Y + EmptyLineTop), UiTheme.TextSecondary * opacity);
    }

    // The strip follows the gliding highlight, over the rows below it, and dims with it while the rail has the cursor.
    private void DrawDetail(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        if (place.Detail is not { } detail)
        {
            return;
        }

        var selected = place.Selection.RowIn(RowArea(place.Area, 0)).Area;
        var top = ItemDetailLine.TopFor(selected, place.Area.Bottom, _stripHeight);
        var stripOpacity = place.HasCursor ? opacity : opacity * UiTheme.DimmedSelectionOpacity;
        DrawStrip(batch, textures, new DetailStrip(detail, StripAt(place.Area, top)), stripOpacity);
    }

    // From just left of the name column to the list's right edge.
    private Rectangle StripAt(Rectangle list, int top) =>
        new(list.X + NameLeft - StripPaddingX, top, list.Width - NameLeft + StripPaddingX, _stripHeight);

    // The battle's description box look, so both tips match: a dark fill over a bright hairline.
    private void DrawStrip(SpriteBatch batch, TextureLibrary textures, DetailStrip strip, float opacity)
    {
        var area = strip.Area;
        batch.Draw(textures.Pixel, area, UiTheme.CommandHintFill * opacity);
        batch.Draw(_ui.Gradient(textures, UiTheme.HighlightLine), area with { Y = area.Bottom - 1, Height = 1 }, Color.White * opacity);
        batch.DrawString(_bodyFont, strip.Text, new Vector2(area.X + StripPaddingX, area.Y + StripPaddingY), UiTheme.TextPrimary * opacity);
    }

    // The pointer marks the row while the list has the cursor; while the rail has it, the row stays chosen, dimmed.
    private void DrawSelection(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var selected = place.Selection.RowIn(RowArea(place.Area, 0));
        if (!place.HasCursor)
        {
            _ui.DrawSelectedRow(batch, textures, selected.Area, opacity * UiTheme.DimmedSelectionOpacity);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, selected, opacity);
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

    /// <summary>What the list draws, and where.</summary>
    /// <param name="Rows">Every row of the category, built when it or the items change.</param>
    /// <param name="Cursor">The selected row and the first visible one; null when the category is empty.</param>
    /// <param name="HasCursor">Whether the list has the cursor rather than the category rail.</param>
    /// <param name="Selection">Where the gliding highlight and the bobbing pointer are.</param>
    /// <param name="Area">The rows' column, from the first row's top, as tall as the visible rows.</param>
    /// <param name="Detail">The strip's text (<see cref="ItemDetailLine.TextFor"/>); null for no strip.</param>
    public readonly record struct Place(
        IReadOnlyList<InventoryRow> Rows, ListCursor? Cursor, bool HasCursor, SelectionMotion Selection, Rectangle Area, string? Detail);

    // The detail strip's text and its area.
    private readonly record struct DetailStrip(string Text, Rectangle Area);
}
