using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Equipment page's sheet: the member's name, what they wear in each slot (the slot as a small spaced-caps
/// label, the item's icon and name at body size, a hairline under each row, as the Status page's rows read), the
/// "Remove all" action, the comparison (<see cref="EquipmentChangesView"/>) and why the last change didn't happen.
/// </summary>
internal sealed class EquipmentSheetView
{
    // Placeholder layout (decision 82): a 28 x 20 margin, the member's name as a heading, section titles as dark bands
    // (decision 335), 40 px rows (the slot label 10 px in, then a 32 px icon in a 48 px column and what's worn), a 10
    // px gap above "Remove all", then the comparison. The 848 px page: 20 + 60 + 46 to the first row, nine rows 360,
    // the gap and "Remove all" 50, 18 to the Changes band (554), the section's 246 (800), then 12 to the notice, which
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
    private const int NoticeGap = 12;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SectionTitle _equipmentTitle;
    private readonly SheetRowLook _rows;
    private readonly EquipmentChangesView _changes;
    private readonly EquipmentLabels _labels;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    public EquipmentSheetView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _equipmentTitle = new SectionTitle(text, MessageIds.StatusEquipment);
        _rows = new SheetRowLook(text);
        _changes = new EquipmentChangesView(text, _rows);
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
        DrawRemoveAllRow(batch, removeAll, opacity);

        var changes = removeAll with { Y = removeAll.Bottom + ChangesGap, Height = 0 };
        _changes.Draw(batch, textures, new EquipmentChangesView.Place(changes, place.Changes, place.ChangesCaptionId), opacity);
        if (place.Notice is not null)
        {
            var noticeTop = changes.Y + EquipmentChangesView.Height + NoticeGap;
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

    // The slot's label small at the left, a hairline under the row so nine rows stay readable, then what's worn: its
    // icon and name, or "None" dimmed.
    private void DrawSlotRow(SpriteBatch batch, TextureLibrary textures, SlotRow slot, float opacity)
    {
        var row = slot.Row;
        _rows.DrawLabel(batch, row with { X = row.X + RowInset }, slot.Label, opacity);
        batch.Draw(textures.Pixel, row with { Y = row.Bottom - 1, Height = 1 }, UiTheme.Stroke * opacity);
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var wornLeft = row.X + RowInset + IconColumn + WornColumn;
        if (slot.Worn?.Base is not { } worn)
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

    /// <summary>What to draw this frame, and where.</summary>
    /// <param name="Sheet">The shown member.</param>
    /// <param name="Area">The sheet's part of the page.</param>
    /// <param name="Selection">
    /// Where the highlight is, in rows: a slot's index in <see cref="EquipmentSheet.Slots"/>, or its length for "Remove
    /// all".
    /// </param>
    /// <param name="HasCursor">False while the choice panel has it: the selected row dims, with no pointer.</param>
    /// <param name="Changes">What a change would do, or what the highlighted worn item gives; null for nothing.</param>
    /// <param name="ChangesCaptionId">Which of the two the comparison shows: its band's caption.</param>
    /// <param name="Notice">Why the last change didn't happen, or null.</param>
    public readonly record struct Place(
        EquipmentSheet Sheet,
        Rectangle Area,
        SelectionMotion Selection,
        bool HasCursor,
        StatComparisonText? Changes,
        string ChangesCaptionId,
        string? Notice);

    // A slot's row: its label and what's worn there, null for nothing.
    private readonly record struct SlotRow(Rectangle Row, string Label, GearPiece? Worn);

    // The sheet's fixed text; Slots follows EquipmentSheet.Slots, in the caps the row look's labels are drawn in.
    private sealed record EquipmentLabels(string[] Slots, string Empty, string RemoveAll)
    {
        public static EquipmentLabels From(UiMessages messages) => new(
            [.. EquipmentSheet.Slots.Select(slot => messages.Text(EquipmentSheet.LabelIdOf(slot)).ToUpperInvariant())],
            messages.Text(MessageIds.SlotEmpty),
            messages.Text(MessageIds.EquipmentRemoveAll));
    }
}
