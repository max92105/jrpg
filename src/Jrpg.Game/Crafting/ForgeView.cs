using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Draws the Forge screen: a translucent dark layer over the map, which shows through it, the title, and the owned
/// pieces (icon, name in its rarity's color, decision 362) under the shared selection; or a line when none is owned. A
/// placeholder placement until the plan's F1d.
/// </summary>
internal sealed class ForgeView
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 10;

    // The menu header's title size and side margin, so the Forge's title reads as a screen's; the rows in the menu
    // pages' body size.
    private const int TitleTextPixels = 38;
    private const int TitleMargin = 48;
    private const int BodyPixels = 26;

    // The Inventory list's row shape: 52 px rows with a 32 px icon in a 48 px column, the list 36 px under the title.
    private const int RowHeight = 52;
    private const int IconColumn = 48;
    private const int RowIconSize = 32;
    private const int RowInset = 10;
    private const int ListTop = TitleMargin + TitleTextPixels + 36;
    private const int ListWidth = 560;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _title;
    private readonly string _empty;

    /// <param name="text">The title, the empty line and the rows' font.</param>
    /// <param name="ui">The shared textures: icons, the selected row.</param>
    public ForgeView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _title = text.Messages.Text(MessageIds.ForgeTitle);
        _empty = text.Messages.Text(MessageIds.ForgeEmpty);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="list">The owned pieces and the cursor on them.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ForgeList list)
    {
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_titleFont, _title, new Vector2(TitleMargin, TitleMargin), UiTheme.TextPrimary);
        var firstRow = new Rectangle(TitleMargin, ListTop, ListWidth, RowHeight);
        if (list.Cursor is not { } cursor)
        {
            batch.DrawString(_bodyFont, _empty, new Vector2(firstRow.X + RowInset, firstRow.Y), UiTheme.TextSecondary);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, list.Selection.RowIn(firstRow), opacity: 1);
        var last = Math.Min(list.Pieces.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = firstRow with { Y = firstRow.Y + ((index - cursor.FirstVisible) * RowHeight) };
            DrawRow(batch, textures, row, list.Pieces[index]);
        }
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, GearPiece piece)
    {
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var iconTop = row.Y + ((RowHeight - RowIconSize) / 2);
        batch.Draw(_ui.Icon(textures, piece.Base.IconFile, RowIconSize), new Vector2(row.X + RowInset, iconTop), Color.White);
        batch.DrawString(_bodyFont, piece.Base.Name, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.RarityColorOf(piece.Rarity));
    }
}
