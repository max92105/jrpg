using System.Collections.Immutable;
using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Equipment page's sheet (legacy <c>EquipmentView</c>): the member's name, what they wear in each slot, the
/// "Remove all" action, their parameters with the preview of a change, and why the last change didn't happen.
/// </summary>
internal sealed class EquipmentSheetView
{
    // Legacy EquipmentView: a 28 x 20 margin, the member's name as Text.Heading, section titles over a hairline, 44 px
    // ListRows (a 26 px icon in a 40 px column, the slot name, then what's worn), a hairline 10 px above "Remove all",
    // and the parameters as two columns of 36 px lines 40 px apart.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int EquipmentTop = 44;
    private const int SectionHeight = 28;
    private const int RowHeight = 44;
    private const int RowInset = 10;
    private const int IconColumn = 40;
    private const int IconSize = 26;
    private const int WornColumn = 200;
    private const int ActionGap = 10;
    private const int ParametersGap = 18;
    private const int StatLineHeight = 36;
    private const int ColumnGap = 40;
    private const int ValueOffset = 160;
    private const int PreviewArrowGap = 70;
    private const int PreviewArrowSide = 6;
    private const int PreviewValueGap = 16;
    private const int NoticeGap = 24;
    private const int AttackLine = 2;
    private const int DefenseLine = 3;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SectionTitle _equipmentTitle;
    private readonly SectionTitle _parametersTitle;
    private readonly EquipmentLabels _labels;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    public EquipmentSheetView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _equipmentTitle = new SectionTitle(text, ui, MessageIds.StatusEquipment);
        _parametersTitle = new SectionTitle(text, ui, MessageIds.StatusParameters);
        _labels = EquipmentLabels.From(text.Messages);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">What to draw and where; the area is the sheet's, left of the choice when it's open.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var left = place.Area.X + Margin;
        var top = place.Area.Y + TopMargin;
        var width = place.Area.Right - Margin - left;
        batch.DrawString(_headingFont, place.Sheet.Name, new Vector2(left, top), UiTheme.TextPrimary * opacity);

        var equipment = new Rectangle(left, top + EquipmentTop, width, 0);
        _equipmentTitle.Draw(batch, textures, equipment, opacity);
        var rowsTop = equipment.Y + SectionHeight;
        for (var index = 0; index < EquipmentSheet.Slots.Length; index++)
        {
            var row = new ListRow(new Rectangle(left, rowsTop + (index * RowHeight), width, RowHeight), index == place.SelectedRow);
            DrawSlotRow(batch, textures, new SlotRow(row, _labels.Slots[index], place.Sheet.Worn[index]), opacity);
        }

        var removeAllTop = rowsTop + (EquipmentSheet.Slots.Length * RowHeight) + ActionGap;
        batch.Draw(textures.Pixel, new Rectangle(left, removeAllTop - 1, width, 1), UiTheme.Stroke * opacity);
        var removeAll = new ListRow(new Rectangle(left, removeAllTop, width, RowHeight), place.SelectedRow == EquipmentSheet.Slots.Length);
        DrawRemoveAllRow(batch, textures, removeAll, opacity);

        var parameters = new Rectangle(left, removeAllTop + RowHeight + ParametersGap, width, 0);
        DrawParameters(batch, textures, new ParametersPlace(parameters, place.Sheet.Stats, place.Preview), opacity);
        if (place.Notice is not null)
        {
            var noticeTop = parameters.Y + SectionHeight + (2 * StatLineHeight) + NoticeGap;
            batch.DrawString(_bodyFont, place.Notice, new Vector2(left + RowInset, noticeTop), UiTheme.Danger * opacity);
        }
    }

    private void DrawSlotRow(SpriteBatch batch, TextureLibrary textures, SlotRow slot, float opacity)
    {
        var row = slot.Row.Area;
        if (slot.Row.IsSelected)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity);
        }

        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, slot.Label, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.TextSecondary * opacity);
        var wornLeft = row.X + RowInset + IconColumn + WornColumn;
        if (slot.Worn is not { } worn)
        {
            batch.DrawString(_bodyFont, _labels.Empty, new Vector2(wornLeft, textTop), UiTheme.TextDisabled * opacity);
            return;
        }

        var iconTop = row.Y + ((RowHeight - IconSize) / 2);
        batch.Draw(_ui.Icon(textures, worn.IconFile, IconSize), new Vector2(wornLeft - IconColumn, iconTop), Color.White * opacity);
        batch.DrawString(_bodyFont, worn.Name, new Vector2(wornLeft, textTop), UiTheme.TextPrimary * opacity);
    }

    private void DrawRemoveAllRow(SpriteBatch batch, TextureLibrary textures, ListRow row, float opacity)
    {
        if (row.IsSelected)
        {
            _ui.DrawSelectedRow(batch, textures, row.Area, opacity);
        }

        var textTop = row.Area.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, _labels.RemoveAll, new Vector2(row.Area.X + RowInset + IconColumn, textTop), UiTheme.TextPrimary * opacity);
    }

    // Legacy StatsPanel: Max HP and Max MP, then Attack and Defense, two to a line.
    private void DrawParameters(SpriteBatch batch, TextureLibrary textures, ParametersPlace place, float opacity)
    {
        var section = place.Section;
        _parametersTitle.Draw(batch, textures, section, opacity);
        var columnWidth = (section.Width - RowInset - ColumnGap) / 2;
        for (var index = 0; index < _labels.Stats.Length; index++)
        {
            var columnLeft = section.X + RowInset + (index % 2 * (columnWidth + ColumnGap));
            var position = new Vector2(columnLeft, section.Y + SectionHeight + (index / 2 * StatLineHeight));
            batch.DrawString(_bodyFont, _labels.Stats[index], position, UiTheme.TextSecondary * opacity);
            batch.DrawString(_bodyFont, place.Stats[index], position + new Vector2(ValueOffset, 0), UiTheme.TextPrimary * opacity);
            if (place.Preview is { } preview && index is AttackLine or DefenseLine)
            {
                var value = index == AttackLine ? preview.Attack : preview.Defense;
                DrawPreview(batch, textures, position + new Vector2(ValueOffset + PreviewArrowGap, 0), value);
            }
        }
    }

    // Legacy StatLine's preview: an arrow, drawn as a small diamond since the font lacks ▸, then the new value colored by
    // whether it goes up or down. The choice only opens once the menu has faded in, so it draws at full opacity.
    private void DrawPreview(SpriteBatch batch, TextureLibrary textures, Vector2 position, StatPreviewText.PreviewValue value)
    {
        var arrowCenter = position + new Vector2(PreviewArrowSide, _bodyFont.LineHeight / 2f);
        new Diamond(arrowCenter, PreviewArrowSide).Draw(batch, textures.Pixel, UiTheme.TextSecondary);
        batch.DrawString(_bodyFont, value.Text, position + new Vector2(PreviewValueGap, 0), value.Color);
    }

    /// <summary>What to draw this frame, and where.</summary>
    /// <param name="Sheet">The shown member.</param>
    /// <param name="Area">The sheet's part of the page.</param>
    /// <param name="SelectedRow">A slot's index in <see cref="EquipmentSheet.Slots"/>, or its length for "Remove all".</param>
    /// <param name="Preview">The highlighted choice's effect on attack and defense, or null when no choice is open.</param>
    /// <param name="Notice">Why the last change didn't happen, or null.</param>
    public readonly record struct Place(EquipmentSheet Sheet, Rectangle Area, int SelectedRow, StatPreviewText? Preview, string? Notice);

    // One 44 px row and whether the cursor is on it.
    private readonly record struct ListRow(Rectangle Area, bool IsSelected);

    // A slot's row: its label and what's worn there, null for nothing.
    private readonly record struct SlotRow(ListRow Row, string Label, ItemDefinition? Worn);

    // The parameters section: where it goes, the values in the labels' order, and the preview if a choice is open.
    private readonly record struct ParametersPlace(Rectangle Section, ImmutableArray<string> Stats, StatPreviewText? Preview);

    // The sheet's fixed text; Slots follows EquipmentSheet.Slots, Stats the sheet's.
    private sealed record EquipmentLabels(string[] Slots, string Empty, string RemoveAll, string[] Stats)
    {
        public static EquipmentLabels From(UiMessages messages) => new(
            [.. EquipmentSheet.Slots.Select(slot => messages.Text(EquipmentSheet.LabelIdOf(slot)))],
            messages.Text(MessageIds.SlotEmpty),
            messages.Text(MessageIds.EquipmentRemoveAll),
            [messages.Text(MessageIds.StatMaxHp), messages.Text(MessageIds.StatMaxMp), messages.Text(MessageIds.StatAttack), messages.Text(MessageIds.StatDefense)]);
    }
}
