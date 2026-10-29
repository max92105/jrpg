using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Status page's middle band (decision 334, the plan's S1a): Combat in the left half, the eight resistances
/// in two columns of four in the right half, the four stats only affixes give in two columns of two under them
/// (decision 368; Resources sit beside the skills). Each row is a small spaced-caps label with its value right-aligned
/// close to it, so the two read together (<see cref="SheetRowLook"/>, which the Equipment page shares).
/// </summary>
internal sealed class StatusParametersView
{
    /// <summary>The band's height: the taller of Combat and of Resistances with Affixes under them.</summary>
    public const int Height = CombatHeight > AffixesBottom ? CombatHeight : AffixesBottom;

    /// <summary>A row's height, shared with <see cref="StatusResourcesView"/>.</summary>
    internal const int RowHeight = 34;

    /// <summary>
    /// How far under a section's top its first row starts, shared with <see cref="StatusResourcesView"/>.
    /// </summary>
    internal const int BelowBand = SectionTitle.Height + 10;

    // Placeholder layout (S1a's arithmetic): 34 px rows from 10 px under each band; cells 12 px in and at most 380 px
    // wide, the halves 48 px apart; the resistances in two columns of four, the affix stats 16 px under them in two of
    // two, both sections' columns 24 px apart; HP's 4 px bar under its label, 12 px short of its value.
    private const int CombatRows = 7;
    private const int ResistanceRowsPerColumn = 4;
    private const int AffixRowsPerColumn = 2;
    private const int CellInset = 12;
    private const int CellWidth = 380;
    private const int HalfGap = 48;
    private const int ColumnGap = 24;
    private const int SectionGap = 16;
    private const int HpBarHeight = 4;
    private const int HpBarValueGap = 12;
    private const int CombatHeight = BelowBand + (CombatRows * RowHeight);
    private const int ResistancesHeight = BelowBand + (ResistanceRowsPerColumn * RowHeight);
    private const int AffixesTop = ResistancesHeight + SectionGap;
    private const int AffixesBottom = AffixesTop + BelowBand + (AffixRowsPerColumn * RowHeight);

    private readonly StatusLabels _labels;
    private readonly SheetRowLook _rows;
    private readonly SectionTitle _combatTitle;
    private readonly SectionTitle _resistancesTitle;
    private readonly SectionTitle _affixesTitle;
    private Measured? _shown;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="labels">The combat, resistance and affix labels, in the sheet's order.</param>
    public StatusParametersView(ScreenText text, StatusLabels labels)
    {
        _labels = labels;
        _rows = new SheetRowLook(text);
        _combatTitle = new SectionTitle(text, MessageIds.StatusCombat);
        _resistancesTitle = new SectionTitle(text, MessageIds.StatusResistances);
        _affixesTitle = new SectionTitle(text, MessageIds.StatusAffixes);
    }

    /// <summary>Measures the sheet's values, when the shown member changes.</summary>
    public void Show(StatusSheet sheet) => _shown = new Measured(
        [.. sheet.Combat.Select(_rows.Measure)],
        [.. sheet.Resistances.Select((value, index) => new SheetRowLook.Cell(_labels.Resistances[index], _rows.Measure(value), ColorOf(sheet.ResistanceTones[index])))],
        [.. sheet.Affixes.Select((value, index) => new SheetRowLook.Cell(_labels.Affixes[index], _rows.Measure(value), UiTheme.TextPrimary))]);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the bar come from.</param>
    /// <param name="place">Where the band goes, and HP's bar.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    /// <exception cref="InvalidOperationException">No sheet was shown yet.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var shown = _shown ?? throw new InvalidOperationException("Show a sheet before drawing it.");
        var frame = new Frame(batch, textures, opacity);
        var band = place.Band;
        var halfWidth = (band.Width - HalfGap) / 2;
        var combat = band with { Width = halfWidth };
        DrawCombat(frame, combat, shown.Combat, place.HpBar);

        var right = band with { X = band.X + halfWidth + HalfGap, Width = halfWidth };
        _resistancesTitle.Draw(batch, textures, right, opacity);
        DrawInTwoColumns(frame, right, shown.Resistances, ResistanceRowsPerColumn);
        var affixes = right with { Y = right.Y + AffixesTop };
        _affixesTitle.Draw(batch, textures, affixes, opacity);
        DrawInTwoColumns(frame, affixes, shown.Affixes, AffixRowsPerColumn);
    }

    // Each combat line under the band; HP's bar runs under its label.
    private void DrawCombat(Frame frame, Rectangle section, MeasuredText[] values, StatBar hpBar)
    {
        _combatTitle.Draw(frame.Batch, frame.Textures, section, frame.Opacity);
        for (var index = 0; index < values.Length; index++)
        {
            _rows.Draw(frame.Batch, CellAt(section, index, section.Width), new SheetRowLook.Cell(_labels.Combat[index], values[index], UiTheme.TextPrimary), frame.Opacity);
        }

        var hp = CellAt(section, StatusSheet.HpLine, section.Width);
        var barWidth = (int)(hp.Width - values[StatusSheet.HpLine].Width) - HpBarValueGap;
        hpBar.Draw(frame.Batch, frame.Textures, new Rectangle(hp.X, hp.Bottom - HpBarHeight, barWidth, HpBarHeight), frame.Opacity);
    }

    // A section's cells in two columns under its band, down the first column, then the second.
    private void DrawInTwoColumns(Frame frame, Rectangle section, SheetRowLook.Cell[] cells, int rowsPerColumn)
    {
        var columnWidth = (section.Width - CellInset - ColumnGap) / 2;
        for (var index = 0; index < cells.Length; index++)
        {
            var column = section with { X = section.X + (index / rowsPerColumn * (columnWidth + ColumnGap)) };
            _rows.Draw(frame.Batch, CellAt(column, index % rowsPerColumn, columnWidth + CellInset), cells[index], frame.Opacity);
        }
    }

    /// <summary>
    /// A row's cell under a section's band: at most the cell width, so its value stays close to its label.
    /// </summary>
    internal static Rectangle CellAt(Rectangle section, int row, int columnWidth) => new(
        section.X + CellInset, section.Y + BelowBand + (row * RowHeight), Math.Min(CellWidth, columnWidth - CellInset), RowHeight);

    // A weakness reads as a danger, a resistance at the cap in the accent color (S1b).
    private static Color ColorOf(ResistanceTone tone) => tone switch
    {
        ResistanceTone.Weak => UiTheme.Danger,
        ResistanceTone.Capped => UiTheme.Positive,
        _ => UiTheme.TextPrimary,
    };


    /// <summary>Where the band goes this frame.</summary>
    /// <param name="Band">The band's top left and width, under the profile.</param>
    /// <param name="HpBar">The shown member's HP bar.</param>
    public readonly record struct Place(Rectangle Band, StatBar HpBar);

    // What the drawing helpers draw with this frame.
    private readonly record struct Frame(SpriteBatch Batch, TextureLibrary Textures, float Opacity);

    // The shown sheet's values, measured once when it is shown.
    private sealed record Measured(
        MeasuredText[] Combat, SheetRowLook.Cell[] Resistances, SheetRowLook.Cell[] Affixes);
}
