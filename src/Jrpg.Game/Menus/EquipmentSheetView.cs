using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Equipment page's sheet: the member's name, what they wear in each slot, the "Remove all" action, the
/// Changes a choice would make or the highlighted worn item gives (decision 337), and why the last change didn't
/// happen.
/// </summary>
internal sealed class EquipmentSheetView
{
    // Placeholder layout (decision 82; E2 does the look): a 28 x 20 margin, the member's name as a heading, section
    // titles as dark bands (decision 335), 40 px rows (a 32 px icon in a 48 px column, the slot name, then what's
    // worn), a hairline 10 px above "Remove all", and the Changes as two columns of four 40 px lines, values 320 px in
    // (names run long). The 848 px page: 20 + 60 + 46 to the first row, nine rows 360, the gap and "Remove all" 50, 18
    // to the Changes band (554), its 46, four lines 160 and the "+N more" line 40 (800), then 12 to the notice, which
    // ends near 842.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int BodyPixels = 26;
    private const int EquipmentTop = 60;
    private const int SectionHeight = SectionTitle.Height + 10;
    private const int RowHeight = 40;
    private const int RowInset = 10;
    private const int IconColumn = 48;
    private const int IconSize = 32;
    private const int WornColumn = 260;
    private const int ActionGap = 10;
    private const int ChangesGap = 18;
    private const int ChangeLineHeight = 40;
    private const int ChangeLinesPerColumn = StatComparisonText.MostLines / 2;
    private const int ColumnGap = 40;
    private const int ChangeValueOffset = 320;
    private const int PreviewArrowGap = 90;
    private const int PreviewArrowSide = 6;
    private const int PreviewValueGap = 16;
    private const int NoticeGap = 12;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SectionTitle _equipmentTitle;
    private readonly SectionTitle _changesTitle;
    private readonly EquipmentLabels _labels;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    public EquipmentSheetView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _equipmentTitle = new SectionTitle(text, MessageIds.StatusEquipment);
        _changesTitle = new SectionTitle(text, MessageIds.EquipmentChanges);
        _labels = EquipmentLabels.From(text.Messages);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">What to draw and where; the area is the sheet's, left of the choice when it's open.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var rows = FirstRowOf(place.Area);
        batch.DrawString(_headingFont, place.Sheet.Name, new Vector2(rows.X, place.Area.Y + TopMargin), UiTheme.TextPrimary * opacity);
        _equipmentTitle.Draw(batch, textures, rows with { Y = rows.Y - SectionHeight, Height = 0 }, opacity);

        // The shared selected-row look (decision 252); while the choice panel has the cursor, dimmed with no pointer.
        var selected = place.Selection.At(RowAt(rows, place.Selection.Position));
        if (place.HasCursor)
        {
            _ui.DrawGlidingSelection(batch, textures, selected, opacity);
        }
        else
        {
            _ui.DrawSelectedRow(batch, textures, selected.Area, opacity * UiTheme.DimmedSelectionOpacity);
        }

        for (var index = 0; index < EquipmentSheet.Slots.Length; index++)
        {
            DrawSlotRow(batch, textures, new SlotRow(RowAt(rows, index), _labels.Slots[index], place.Sheet.Worn[index]), opacity);
        }

        var removeAll = RowAt(rows, EquipmentSheet.Slots.Length);
        batch.Draw(textures.Pixel, removeAll with { Y = removeAll.Y - 1, Height = 1 }, UiTheme.Stroke * opacity);
        DrawRemoveAllRow(batch, removeAll, opacity);

        var changes = removeAll with { Y = removeAll.Bottom + ChangesGap, Height = 0 };
        DrawChanges(batch, textures, new ChangesPlace(changes, place.Changes), opacity);
        if (place.Notice is not null)
        {
            var noticeTop = changes.Y + SectionHeight + ((ChangeLinesPerColumn + 1) * ChangeLineHeight) + NoticeGap;
            batch.DrawString(_bodyFont, place.Notice, new Vector2(rows.X + RowInset, noticeTop), UiTheme.Danger * opacity);
        }
    }

    // The first slot's row: under the heading and the equipment title, inside the margins.
    private static Rectangle FirstRowOf(Rectangle area)
    {
        var left = area.X + Margin;
        return new Rectangle(left, area.Y + TopMargin + EquipmentTop + SectionHeight, area.Right - Margin - left, RowHeight);
    }

    // A row's place, whole or between two while the highlight glides: "Remove all" sits a gap below the slots, so the
    // highlight crosses that gap on its way there.
    private static Rectangle RowAt(Rectangle firstRow, float row)
    {
        var gapCrossed = Math.Clamp(row - (EquipmentSheet.Slots.Length - 1), 0, 1) * ActionGap;
        return firstRow with { Y = firstRow.Y + (int)Math.Round((row * RowHeight) + gapCrossed) };
    }

    private void DrawSlotRow(SpriteBatch batch, TextureLibrary textures, SlotRow slot, float opacity)
    {
        var row = slot.Row;
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

    private void DrawRemoveAllRow(SpriteBatch batch, Rectangle row, float opacity)
    {
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, _labels.RemoveAll, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.TextPrimary * opacity);
    }

    // Each line's name, its value before, an arrow and its value after; four lines a column, "+N more" under the first.
    private void DrawChanges(SpriteBatch batch, TextureLibrary textures, ChangesPlace place, float opacity)
    {
        var section = place.Section;
        _changesTitle.Draw(batch, textures, section, opacity);
        if (place.Changes is not { } changes)
        {
            return;
        }

        for (var index = 0; index < changes.Lines.Length; index++)
        {
            var line = changes.Lines[index];
            var position = ChangeLineAt(section, index / ChangeLinesPerColumn, index % ChangeLinesPerColumn);
            batch.DrawString(_bodyFont, line.Label, position, UiTheme.TextSecondary * opacity);
            batch.DrawString(_bodyFont, line.Before, position + new Vector2(ChangeValueOffset, 0), UiTheme.TextPrimary * opacity);
            DrawAfter(batch, textures, position + new Vector2(ChangeValueOffset + PreviewArrowGap, 0), line, opacity);
        }

        if (changes.More is { } more)
        {
            batch.DrawString(_bodyFont, more, ChangeLineAt(section, 0, ChangeLinesPerColumn), UiTheme.TextSecondary * opacity);
        }
    }

    private static Vector2 ChangeLineAt(Rectangle section, int column, int line)
    {
        var columnWidth = (section.Width - RowInset - ColumnGap) / 2;
        var left = section.X + RowInset + (column * (columnWidth + ColumnGap));
        return new Vector2(left, section.Y + SectionHeight + (line * ChangeLineHeight));
    }

    // An arrow, drawn as a small diamond since the font lacks ▸, then the value after, colored by whether it goes up
    // or down.
    private void DrawAfter(SpriteBatch batch, TextureLibrary textures, Vector2 position, StatComparisonText.Line line, float opacity)
    {
        var arrowCenter = position + new Vector2(PreviewArrowSide, _bodyFont.LineHeight / 2f);
        new Diamond(arrowCenter, PreviewArrowSide).Draw(batch, textures.Pixel, UiTheme.TextSecondary * opacity);
        batch.DrawString(_bodyFont, line.After, position + new Vector2(PreviewValueGap, 0), line.Color * opacity);
    }

    /// <summary>What to draw this frame, and where.</summary>
    /// <param name="Sheet">The shown member.</param>
    /// <param name="Area">The sheet's part of the page.</param>
    /// <param name="Selection">
    /// Where the highlight is, in rows: a slot's index in <see cref="EquipmentSheet.Slots"/>, or its length for "Remove
    /// all".
    /// </param>
    /// <param name="HasCursor">False while the choice panel has it: the selected row dims, with no pointer.</param>
    /// <param name="Changes">What a change would do, or what the highlighted worn item gives; null for nothing.</param>
    /// <param name="Notice">Why the last change didn't happen, or null.</param>
    public readonly record struct Place(EquipmentSheet Sheet, Rectangle Area, SelectionMotion Selection, bool HasCursor, StatComparisonText? Changes, string? Notice);

    // A slot's row: its label and what's worn there, null for nothing.
    private readonly record struct SlotRow(Rectangle Row, string Label, ItemDefinition? Worn);

    // The Changes section: where it goes, and what it lists.
    private readonly record struct ChangesPlace(Rectangle Section, StatComparisonText? Changes);

    // The sheet's fixed text; Slots follows EquipmentSheet.Slots.
    private sealed record EquipmentLabels(string[] Slots, string Empty, string RemoveAll)
    {
        public static EquipmentLabels From(UiMessages messages) => new(
            [.. EquipmentSheet.Slots.Select(slot => messages.Text(EquipmentSheet.LabelIdOf(slot)))],
            messages.Text(MessageIds.SlotEmpty),
            messages.Text(MessageIds.EquipmentRemoveAll));
    }
}
