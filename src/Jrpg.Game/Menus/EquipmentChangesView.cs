using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Equipment page's comparison (decision 337) under its band, captioned "Changes" while a choice is open and
/// "Gives" otherwise: two columns of four lines, each a stat's spaced-caps label at the left and, right-aligned, its
/// value now, a small diamond and the value after in the up or down color; "+N more" under the first column. The lines
/// are measured once per comparison, never per frame.
/// </summary>
internal sealed class EquipmentChangesView
{
    /// <summary>The section's height: its band, then four lines and the "+N more" line.</summary>
    public const int Height = SectionTitle.Height + BandGap + ((LinesPerColumn + 1) * LineHeight);

    // Placeholder layout: 40 px lines from 10 px under the band, in two columns 40 px apart and 10 px in; the diamond
    // arrow sits 16 px either side of the values.
    private const int BandGap = 10;
    private const int LineHeight = 40;
    private const int LinesPerColumn = StatComparisonText.MostLines / 2;
    private const int ColumnGap = 40;
    private const int Inset = 10;
    private const int ArrowGap = 16;
    private const int ArrowSide = 6;

    private readonly SheetRowLook _rows;
    private readonly SectionTitle _changesTitle;
    private readonly SectionTitle _givesTitle;
    private StatComparisonText? _measuredChanges;
    private MeasuredLine[] _measured = [];

    /// <param name="text">The captions.</param>
    /// <param name="rows">The shared row look.</param>
    public EquipmentChangesView(ScreenText text, SheetRowLook rows)
    {
        _rows = rows;
        _changesTitle = new SectionTitle(text, MessageIds.EquipmentChanges);
        _givesTitle = new SectionTitle(text, MessageIds.EquipmentGives);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the band and the plain pixel come from.</param>
    /// <param name="place">Where the section goes and what it lists.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var title = place.CaptionId == MessageIds.EquipmentGives ? _givesTitle : _changesTitle;
        title.Draw(batch, textures, place.Section, opacity);
        if (place.Changes is not { } changes)
        {
            return;
        }

        Measure(changes);
        for (var index = 0; index < _measured.Length; index++)
        {
            DrawLine(batch, textures, LineAt(place.Section, index), _measured[index], opacity);
        }

        if (changes.More is { } more)
        {
            _rows.DrawLabel(batch, LineAt(place.Section, LinesPerColumn), more, opacity);
        }
    }

    // The labels take the row look's caps here, once per comparison.
    private void Measure(StatComparisonText changes)
    {
        if (ReferenceEquals(changes, _measuredChanges))
        {
            return;
        }

        _measuredChanges = changes;
        _measured = [.. changes.Lines.Select(line => new MeasuredLine(line.Label.ToUpperInvariant(), _rows.Measure(line.Before), _rows.Measure(line.After), line.Color))];
    }

    // The value after ends at the line's right edge; the arrow and the value before sit left of it.
    private void DrawLine(SpriteBatch batch, TextureLibrary textures, Rectangle line, MeasuredLine measured, float opacity)
    {
        _rows.DrawLabel(batch, line, measured.Label, opacity);
        _rows.DrawValue(batch, line, new SheetRowLook.Value(measured.After, measured.Color, line.Right), opacity);
        var arrowCenter = new Vector2(line.Right - measured.After.Width - ArrowGap - ArrowSide, line.Y + (line.Height / 2f));
        new Diamond(arrowCenter, ArrowSide).Draw(batch, textures.Pixel, UiTheme.TextSecondary * opacity);
        _rows.DrawValue(batch, line, new SheetRowLook.Value(measured.Before, UiTheme.TextPrimary, arrowCenter.X - ArrowSide - ArrowGap), opacity);
    }

    // The first four lines fill the left column, the next four the right one; the "+N more" line is the fifth row.
    private static Rectangle LineAt(Rectangle section, int index)
    {
        var columnWidth = (section.Width - Inset - ColumnGap) / 2;
        var left = section.X + Inset + (index / LinesPerColumn * (columnWidth + ColumnGap));
        return new Rectangle(left, section.Y + SectionTitle.Height + BandGap + (index % LinesPerColumn * LineHeight), columnWidth, LineHeight);
    }

    /// <summary>Where the section goes this frame, and what it lists.</summary>
    /// <param name="Section">The band's top left and width; the section runs <see cref="Height"/> down from it.</param>
    /// <param name="Changes">What differs; null for nothing to list.</param>
    /// <param name="CaptionId">
    /// <see cref="MessageIds.EquipmentChanges"/> or <see cref="MessageIds.EquipmentGives"/>.
    /// </param>
    public readonly record struct Place(Rectangle Section, StatComparisonText? Changes, string CaptionId);

    private readonly record struct MeasuredLine(string Label, MeasuredText Before, MeasuredText After, Color Color);
}
