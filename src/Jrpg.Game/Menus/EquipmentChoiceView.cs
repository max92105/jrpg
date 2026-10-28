using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws an <see cref="EquipmentChoice"/> in the page column at the page's right, which the page draws: the slot's
/// name, Name and Qty column titles, then "Remove", each owned item with its icon and count, and each item another
/// member wears, named with its wearer in the secondary color.
/// </summary>
internal sealed class EquipmentChoiceView
{
    /// <summary>The choice column's width (placeholder).</summary>
    public const int PanelWidth = 600;

    // Placeholder layout (decision 82): a 24 x 20 margin, a 30 px heading, column titles over a hairline, then 52 px
    // rows (a 32 px icon in a 48 px column, then the name; one row per piece, so no quantity, decision 345).
    private const int Margin = 24;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int BodyPixels = 26;
    private const int LabelPixels = 18;
    private const int ColumnTitlesTop = 60;
    private const int RowsTop = ColumnTitlesTop + ColumnTitles.Height;
    private const int RowHeight = 52;
    private const int RowInset = 10;
    private const int IconColumn = 48;
    private const int IconSize = 32;

    private readonly UiTextures _ui;
    private readonly UiMessages _messages;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _remove;
    private readonly ColumnTitles _columnTitles;
    private string _heading = string.Empty;
    private string[] _wornRows = [];

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: icons, section line, selected row.</param>
    public EquipmentChoiceView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _messages = text.Messages;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _remove = text.Messages.Text(MessageIds.EquipmentRemove);
        var labelFont = text.Fonts.FontAt(LabelPixels);
        _columnTitles = new ColumnTitles(
            labelFont,
            ui,
            [
                ColumnTitle.FromLeft(text.Messages.Text(MessageIds.InventoryColumnItem), RowInset + IconColumn),
            ]);
    }

    /// <summary>Builds the text once, when the choice opens.</summary>
    /// <param name="choice">The items offered.</param>
    /// <param name="slotName">The slot's name, as the heading.</param>
    public void Show(EquipmentChoice choice, string slotName)
    {
        _heading = slotName;
        _wornRows = [.. choice.Takeable.Select(worn => _messages.Format(
            MessageIds.EquipmentWornRow,
            new MessageArgument("item", worn.Piece.Base.Name),
            new MessageArgument("member", worn.Wearer.Definition.Name)))];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="choice">The same choice <see cref="Show"/> was given.</param>
    /// <param name="panel">The choice column's area, at the page's right.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, EquipmentChoice choice, Rectangle panel)
    {
        var left = panel.X + Margin;
        var top = panel.Y + TopMargin;
        var width = panel.Width - (2 * Margin);
        batch.DrawString(_headingFont, _heading, new Vector2(left, top), UiTheme.TextPrimary);
        _columnTitles.Draw(batch, textures, new Rectangle(left, top + ColumnTitlesTop, width, 0), opacity: 1);

        var rows = new Rectangle(left, top + RowsTop, width, RowHeight);
        _ui.DrawGlidingSelection(batch, textures, choice.Selection.RowIn(rows), opacity: 1);
        var last = Math.Min(choice.RowCount, choice.FirstVisibleRow + EquipmentChoice.VisibleRows);
        for (var index = choice.FirstVisibleRow; index < last; index++)
        {
            DrawRow(batch, textures, choice, RowAt(rows, index - choice.FirstVisibleRow), index);
        }
    }

    private static Rectangle RowAt(Rectangle firstRow, int visibleRow) =>
        firstRow with { Y = firstRow.Y + (visibleRow * RowHeight) };

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, EquipmentChoice choice, Rectangle row, int index)
    {
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var nameLeft = row.X + RowInset + IconColumn;
        if (index == 0)
        {
            batch.DrawString(_bodyFont, _remove, new Vector2(nameLeft, textTop), UiTheme.TextPrimary);
            return;
        }

        var iconTop = row.Y + ((RowHeight - IconSize) / 2);
        if (choice.WornAt(index) is { } worn)
        {
            batch.Draw(_ui.Icon(textures, worn.Piece.Base.IconFile, IconSize), new Vector2(row.X + RowInset, iconTop), Color.White);
            batch.DrawString(_bodyFont, _wornRows[index - 1 - choice.Owned.Count], new Vector2(nameLeft, textTop), UiTheme.TextSecondary);
            return;
        }

        var owned = choice.Owned[index - 1].Base;
        batch.Draw(_ui.Icon(textures, owned.IconFile, IconSize), new Vector2(row.X + RowInset, iconTop), Color.White);
        batch.DrawString(_bodyFont, owned.Name, new Vector2(nameLeft, textTop), UiTheme.TextPrimary);
    }
}
