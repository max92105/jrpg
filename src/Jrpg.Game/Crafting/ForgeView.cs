using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Draws the Forge screen: a translucent dark layer over the map, which shows through it, the title, the owned pieces
/// (icon, name in its rarity's color, decision 362) under the shared selection or a line when none is owned, and beside
/// them the detail column: the highlighted piece's name, rarity and affix lines, then every material owned (decision
/// 370). A placeholder placement until the plan's F1d.
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

    // The detail column starts a margin right of the list; its lines are the body's line height apart, the materials
    // the Equipment sheet's 40 px rows a line under the affixes.
    private const int DetailLeft = TitleMargin + ListWidth + TitleMargin;
    private const int MaterialRowHeight = 40;

    private readonly UiTextures _ui;
    private readonly SheetRowLook _look;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _title;
    private readonly string _empty;
    private readonly string _materialsHeading;
    private readonly string _noAffixes;

    /// <param name="text">The title, the lines and the rows' font.</param>
    /// <param name="ui">The shared textures: icons, the selected row.</param>
    /// <param name="look">Draws the material rows, whose counts it measured.</param>
    public ForgeView(ScreenText text, UiTextures ui, SheetRowLook look)
    {
        _ui = ui;
        _look = look;
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _title = text.Messages.Text(MessageIds.ForgeTitle);
        _empty = text.Messages.Text(MessageIds.ForgeEmpty);
        _materialsHeading = text.Messages.Text(MessageIds.ForgeMaterials);
        _noAffixes = text.Messages.Text(MessageIds.ForgeNoAffixes);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="list">The owned pieces and the cursor on them.</param>
    /// <param name="materials">Every material and how many are owned.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ForgeList list, MaterialStock materials)
    {
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_titleFont, _title, new Vector2(TitleMargin, TitleMargin), UiTheme.TextPrimary);
        DrawList(batch, textures, list);
        var detailTop = list.SelectedRow is { } selected ? DrawPiece(batch, selected, ListTop) : ListTop;
        DrawMaterials(batch, materials, detailTop + _bodyFont.LineHeight);
    }

    private void DrawList(SpriteBatch batch, TextureLibrary textures, ForgeList list)
    {
        var firstRow = new Rectangle(TitleMargin, ListTop, ListWidth, RowHeight);
        if (list.Cursor is not { } cursor)
        {
            batch.DrawString(_bodyFont, _empty, new Vector2(firstRow.X + RowInset, firstRow.Y), UiTheme.TextSecondary);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, list.Selection.RowIn(firstRow), opacity: 1);
        var last = Math.Min(list.Rows.Count, cursor.FirstVisible + VisibleRows);
        for (var index = cursor.FirstVisible; index < last; index++)
        {
            var row = firstRow with { Y = firstRow.Y + ((index - cursor.FirstVisible) * RowHeight) };
            DrawRow(batch, textures, row, list.Rows[index].Piece);
        }
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, GearPiece piece)
    {
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var iconTop = row.Y + ((RowHeight - RowIconSize) / 2);
        batch.Draw(_ui.Icon(textures, piece.Base.IconFile, RowIconSize), new Vector2(row.X + RowInset, iconTop), Color.White);
        batch.DrawString(_bodyFont, piece.Base.Name, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.RarityColorOf(piece.Rarity));
    }

    // The name in its rarity's color, the rarity's name, then one line per affix.
    private int DrawPiece(SpriteBatch batch, ForgeRow row, int top)
    {
        var piece = row.Piece;
        batch.DrawString(_bodyFont, piece.Base.Name, new Vector2(DetailLeft, top), UiTheme.RarityColorOf(piece.Rarity));
        top += _bodyFont.LineHeight;
        batch.DrawString(_bodyFont, row.RarityName, new Vector2(DetailLeft, top), UiTheme.TextSecondary);
        top += _bodyFont.LineHeight;
        if (row.AffixLines.Count == 0)
        {
            batch.DrawString(_bodyFont, _noAffixes, new Vector2(DetailLeft, top), UiTheme.TextSecondary);
            return top + _bodyFont.LineHeight;
        }

        foreach (var line in row.AffixLines)
        {
            batch.DrawString(_bodyFont, line, new Vector2(DetailLeft, top), UiTheme.TextPrimary);
            top += _bodyFont.LineHeight;
        }

        return top;
    }

    private void DrawMaterials(SpriteBatch batch, MaterialStock materials, int top)
    {
        batch.DrawString(_bodyFont, _materialsHeading, new Vector2(DetailLeft, top), UiTheme.TextPrimary);
        var row = new Rectangle(DetailLeft, top + _bodyFont.LineHeight, VirtualScreen.Width - DetailLeft - TitleMargin, MaterialRowHeight);
        foreach (var material in materials.Rows)
        {
            _look.Draw(batch, row, new SheetRowLook.Cell(material.Name, material.Count, UiTheme.RarityMaterial), opacity: 1);
            row.Y += MaterialRowHeight;
        }
    }
}
