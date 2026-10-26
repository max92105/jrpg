using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Inventory page's visible rows with the selected one marked, or the empty line when the category holds
/// nothing.
/// </summary>
internal sealed class InventoryListView
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 10;

    /// <summary>The rows' text size in pixels; the description under the list shares it.</summary>
    public const int BodyPixels = 26;

    /// <summary>A row's icon starts this far in; the description under the list lines up with it.</summary>
    public const int RowInset = 10;

    /// <summary>The name column starts this far in, past the icon's column.</summary>
    public const int NameLeft = RowInset + IconColumn;

    /// <summary>The quantity's right edge is this far from the row's.</summary>
    public const int QuantityInset = 16;

    /// <summary>The height of the visible rows together.</summary>
    public const int Height = VisibleRows * RowHeight;

    // Placeholder layout (decision 82): 52 px rows with a 32 px icon in a 48 px column. The empty line sits 24 px under
    // the rows' top.
    private const int RowHeight = 52;
    private const int IconColumn = 48;
    private const int RowIconSize = 32;
    private const int EmptyLineGap = 24;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _bodyFont;
    private readonly MeasuredText _empty;

    /// <param name="text">The body font and the empty line.</param>
    /// <param name="ui">The selection and icon textures.</param>
    public InventoryListView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _empty = MeasuredText.Of(_bodyFont, text.Messages.Text(MessageIds.InventoryEmpty));
    }

    /// <summary>The body font's width of <paramref name="text"/>, for a row's quantity.</summary>
    public MeasuredText MeasureQuantity(string text) => MeasuredText.Of(_bodyFont, text);

    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        if (place.Cursor is not { } cursor)
        {
            var emptyLeft = place.Area.X + ((place.Area.Width - _empty.Width) / 2f);
            batch.DrawString(_bodyFont, _empty.Text, new Vector2(emptyLeft, place.Area.Y + EmptyLineGap), UiTheme.TextSecondary * opacity);
            return;
        }

        var last = Math.Min(place.Rows.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = new Rectangle(place.Area.X, place.Area.Y + ((index - cursor.FirstVisible) * RowHeight), place.Area.Width, RowHeight);
            // The list with the cursor marks the row with the pointer; with the rail, the row stays chosen, dimmed.
            if (index == cursor.Selected)
            {
                _ui.DrawSelectedRow(batch, textures, row, place.HasCursor ? opacity : opacity * UiTheme.DimmedSelectionOpacity);
            }

            if (index == cursor.Selected && place.HasCursor)
            {
                MenuPointer.Draw(batch, textures.Pixel, new Vector2(row.X, row.Center.Y), opacity);
            }

            DrawRow(batch, textures, row, place.Rows[index], opacity);
        }
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
    /// <param name="Area">The rows' box: its top is the first row's, its width every row's.</param>
    /// <param name="Rows">Every row of the category, the hidden ones included.</param>
    /// <param name="Cursor">The selected row and the first visible one; null when the category is empty.</param>
    /// <param name="HasCursor">Whether the list has the cursor, rather than the category rail.</param>
    public readonly record struct Place(Rectangle Area, IReadOnlyList<InventoryRow> Rows, ListCursor? Cursor, bool HasCursor);
}
