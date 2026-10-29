using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Draws the Forge's detail column: the highlighted piece's name in its rarity's color beside its rarity, its affix
/// lines under an Affixes band, every material owned under a Materials band (decisions 335, 370), and, while the
/// actions are open, the Actions band with each action's materials, a refused one greyed with its reason under the
/// rows, or what the last action gave in the reason line's place. With no piece owned, the materials alone.
/// </summary>
internal sealed class ForgeDetailView
{
    // The Equipment sheet's shape: a 56 px name row (the rarity 16 px right of the name), 46 px bands (36 + 10), 40 px
    // rows, 24 px between sections; the action rows 48 px, as the menu's choices, the reason a 40 px line under them.
    private const int NameRowHeight = 56;
    private const int BandHeight = SectionTitle.Height + 10;
    private const int RowHeight = 40;
    private const int ActionRowHeight = 48;
    private const int RowInset = 10;
    private const int SectionGap = 24;
    private const int RarityGap = 16;

    private readonly UiTextures _ui;
    private readonly SheetRowLook _look;
    private readonly SpriteFontBase _bodyFont;
    private readonly SectionTitle _affixesTitle;
    private readonly SectionTitle _materialsTitle;
    private readonly SectionTitle _actionsTitle;
    private readonly string _noAffixes;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: the selected row.</param>
    /// <param name="look">Draws the material rows, whose counts it measured.</param>
    /// <param name="bodyFont">The name, the affix lines and the actions' font.</param>
    public ForgeDetailView(ScreenText text, UiTextures ui, SheetRowLook look, SpriteFontBase bodyFont)
    {
        _ui = ui;
        _look = look;
        _bodyFont = bodyFont;
        _affixesTitle = new SectionTitle(text, MessageIds.StatusAffixes);
        _materialsTitle = new SectionTitle(text, MessageIds.ForgeMaterials);
        _actionsTitle = new SectionTitle(text, MessageIds.ForgeActions);
        _noAffixes = text.Messages.Text(MessageIds.ForgeNoAffixes);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">
    /// The highlighted piece, if any, the materials, the open actions, and the column's area.
    /// </param>
    /// <param name="opacity">The Forge's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var top = place.Area.Y;
        if (place.Row is { } row)
        {
            top = DrawPiece(batch, textures, row, place.Area with { Y = top }, opacity) + SectionGap;
        }

        top = DrawMaterials(batch, textures, place, top, opacity) + SectionGap;
        if (place.Actions is { } actions)
        {
            DrawActions(batch, textures, actions, place.Area with { Y = top }, opacity);
        }
        else if (place.Notice is { } notice)
        {
            DrawLine(batch, notice, place.Area.X + RowInset, top, UiTheme.TextSecondary * opacity);
        }
    }

    // The name row, then the Affixes band and one line per affix; returns where the section ends.
    private int DrawPiece(SpriteBatch batch, TextureLibrary textures, ForgeRow row, Rectangle area, float opacity)
    {
        var piece = row.Piece;
        var nameTop = area.Y + ((NameRowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, row.Name.Text, new Vector2(area.X + RowInset, nameTop), UiTheme.RarityColorOf(piece.Rarity) * opacity);
        var rarityLeft = area.X + RowInset + (int)Math.Ceiling(row.Name.Width) + RarityGap;
        _look.DrawLabel(batch, new Rectangle(rarityLeft, area.Y, 0, NameRowHeight), row.RarityName.ToUpperInvariant(), opacity);
        var band = area.Y + NameRowHeight;
        _affixesTitle.Draw(batch, textures, area with { Y = band, Height = 0 }, opacity);
        var lineTop = band + BandHeight;
        if (row.AffixLines.Count == 0)
        {
            DrawLine(batch, _noAffixes, area.X + RowInset, lineTop, UiTheme.TextDisabled * opacity);
            return lineTop + RowHeight;
        }

        foreach (var line in row.AffixLines)
        {
            DrawLine(batch, line, area.X + RowInset, lineTop, UiTheme.TextPrimary * opacity);
            lineTop += RowHeight;
        }

        return lineTop;
    }

    private int DrawMaterials(SpriteBatch batch, TextureLibrary textures, Place place, int top, float opacity)
    {
        _materialsTitle.Draw(batch, textures, place.Area with { Y = top, Height = 0 }, opacity);
        var materialRow = new Rectangle(place.Area.X + RowInset, top + BandHeight, place.Area.Width - RowInset, RowHeight);
        foreach (var material in place.Materials)
        {
            _look.Draw(batch, materialRow, new SheetRowLook.Cell(material.Name, material.Count, UiTheme.RarityMaterial), opacity);
            materialRow.Y += RowHeight;
        }

        return materialRow.Y;
    }

    // The label at the left and the materials at the right, both greyed when Core refuses the action; the highlighted
    // action's reason under the rows.
    private void DrawActions(SpriteBatch batch, TextureLibrary textures, ForgeActionChoice actions, Rectangle area, float opacity)
    {
        _actionsTitle.Draw(batch, textures, area with { Height = 0 }, opacity);
        var firstRow = new Rectangle(area.X, area.Y + BandHeight, area.Width, ActionRowHeight);
        _ui.DrawGlidingSelection(batch, textures, actions.Selection.RowIn(firstRow), opacity);
        var row = firstRow;
        foreach (var action in actions.Rows)
        {
            var color = (action.IsAvailable ? UiTheme.TextPrimary : UiTheme.TextDisabled) * opacity;
            var textTop = row.Y + ((ActionRowHeight - _bodyFont.LineHeight) / 2f);
            batch.DrawString(_bodyFont, action.Label, new Vector2(row.X + RowInset, textTop), color);
            batch.DrawString(_bodyFont, action.Materials.Text, new Vector2(row.Right - RowInset - action.Materials.Width, textTop), color);
            row.Y += ActionRowHeight;
        }

        if (actions.Selected.Reason is { } reason)
        {
            DrawLine(batch, reason, area.X + RowInset, row.Y, UiTheme.TextSecondary * opacity);
        }
    }

    private void DrawLine(SpriteBatch batch, string text, int left, int rowTop, Color color) =>
        batch.DrawString(_bodyFont, text, new Vector2(left, rowTop + ((RowHeight - _bodyFont.LineHeight) / 2f)), color);

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Row">The highlighted piece; null when none is owned.</param>
    /// <param name="Materials">Every material and how many are owned.</param>
    /// <param name="Actions">The actions open on the piece; null while the list has the cursor.</param>
    /// <param name="Notice">
    /// What the last action gave, shown while the actions are closed; null when nothing was done.
    /// </param>
    /// <param name="Area">The column's area.</param>
    public readonly record struct Place(ForgeRow? Row, IReadOnlyList<MaterialCount> Materials, ForgeActionChoice? Actions, string? Notice, Rectangle Area);
}
