using System.Globalization;
using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws an <see cref="EquipmentChoice"/> in a translucent panel at the page's right: the slot's name, Name and Qty
/// column titles, then "Remove", each owned item with its icon and count, and each item another member wears, named
/// with its wearer in the secondary color.
/// </summary>
internal sealed class EquipmentChoiceView
{
    /// <summary>The panel's width (placeholder).</summary>
    public const int PanelWidth = 560;

    // Placeholder layout (decision 82): a 24 x 20 margin, a 22 px heading, column titles over a hairline, then 44 px
    // rows (a 26 px icon in a 40 px column, the quantity 16 px from the right).
    private const int Margin = 24;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int LabelPixels = 14;
    private const int ColumnTitlesTop = 48;
    private const int ColumnTitleLineGap = 20;
    private const int RowsTop = ColumnTitlesTop + 28;
    private const int RowHeight = 44;
    private const int RowInset = 10;
    private const int IconColumn = 40;
    private const int IconSize = 26;
    private const int QuantityInset = 16;

    private readonly UiTextures _ui;
    private readonly UiMessages _messages;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly string _remove;
    private readonly string _nameTitle;
    private readonly string _quantityTitle;
    private readonly float _quantityTitleWidth;
    private string _heading = string.Empty;
    private (string Text, float Width)[] _quantities = [];
    private string[] _wornRows = [];

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: icons, section line, selected row.</param>
    public EquipmentChoiceView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _messages = text.Messages;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _remove = text.Messages.Text(MessageIds.EquipmentRemove);
        _nameTitle = text.Messages.Text(MessageIds.InventoryColumnName);
        _quantityTitle = text.Messages.Text(MessageIds.InventoryColumnQuantity);
        _quantityTitleWidth = _labelFont.MeasureString(_quantityTitle).X;
    }

    /// <summary>Builds the text once, when the choice opens.</summary>
    /// <param name="choice">The items offered.</param>
    /// <param name="slotName">The slot's name, as the heading.</param>
    /// <param name="inventory">How many of each item are owned.</param>
    public void Show(EquipmentChoice choice, string slotName, Inventory inventory)
    {
        _heading = slotName;
        _quantities = [.. choice.Items.Select(item =>
        {
            var quantity = inventory.QuantityOf(item.Id).ToString(CultureInfo.InvariantCulture);
            return (quantity, _bodyFont.MeasureString(quantity).X);
        })];
        _wornRows = [.. choice.Takeable.Select(worn => _messages.Format(
            MessageIds.EquipmentWornRow,
            new MessageArgument("item", worn.Item.Name),
            new MessageArgument("member", worn.Wearer.Definition.Name)))];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="choice">The same choice <see cref="Show"/> was given.</param>
    /// <param name="panel">The panel's area, at the page's right.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, EquipmentChoice choice, Rectangle panel)
    {
        batch.Draw(textures.Pixel, panel, UiTheme.Surface);
        batch.Draw(textures.Pixel, new Rectangle(panel.X, panel.Y, 1, panel.Height), UiTheme.Stroke);
        batch.Draw(textures.Pixel, new Rectangle(panel.Right - 1, panel.Y, 1, panel.Height), UiTheme.Stroke);

        var left = panel.X + Margin;
        var top = panel.Y + TopMargin;
        var width = panel.Width - (2 * Margin);
        batch.DrawString(_headingFont, _heading, new Vector2(left, top), UiTheme.TextPrimary);
        var titlesTop = top + ColumnTitlesTop;
        batch.DrawString(_labelFont, _nameTitle, new Vector2(left + RowInset + IconColumn, titlesTop), UiTheme.TextSecondary);
        batch.DrawString(_labelFont, _quantityTitle, new Vector2(left + width - QuantityInset - _quantityTitleWidth, titlesTop), UiTheme.TextSecondary);
        batch.Draw(_ui.SectionLine(textures), new Rectangle(left, titlesTop + ColumnTitleLineGap, width, 1), Color.White);

        var last = Math.Min(choice.RowCount, choice.FirstVisibleRow + EquipmentChoice.VisibleRows);
        for (var index = choice.FirstVisibleRow; index < last; index++)
        {
            var row = new Rectangle(left, top + RowsTop + ((index - choice.FirstVisibleRow) * RowHeight), width, RowHeight);
            DrawRow(batch, textures, choice, row, index);
        }
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, EquipmentChoice choice, Rectangle row, int index)
    {
        if (index == choice.SelectedRow)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity: 1);
        }

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
            batch.Draw(_ui.Icon(textures, worn.Item.IconFile, IconSize), new Vector2(row.X + RowInset, iconTop), Color.White);
            batch.DrawString(_bodyFont, _wornRows[index - 1 - choice.Items.Count], new Vector2(nameLeft, textTop), UiTheme.TextSecondary);
            return;
        }

        var item = choice.Items[index - 1];
        batch.Draw(_ui.Icon(textures, item.IconFile, IconSize), new Vector2(row.X + RowInset, iconTop), Color.White);
        batch.DrawString(_bodyFont, item.Name, new Vector2(nameLeft, textTop), UiTheme.TextPrimary);
        var (quantity, quantityWidth) = _quantities[index - 1];
        batch.DrawString(_bodyFont, quantity, new Vector2(row.Right - QuantityInset - quantityWidth, textTop), UiTheme.TextPrimary);
    }
}
