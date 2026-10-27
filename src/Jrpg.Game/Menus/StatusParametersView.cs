using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Status page's middle band (decision 334, the plan's S1a): Combat in the left half, the eight resistances
/// in two columns of four in the right half with Resources under them. Each row is a small spaced-caps label with its
/// value right-aligned close to it, so the two read together (<see cref="SheetRowLook"/>, which the Equipment page
/// shares).
/// </summary>
internal sealed class StatusParametersView
{
    /// <summary>The band's height: the taller of Combat and of Resistances with Resources under them.</summary>
    public const int Height = CombatHeight > ResistancesAndResourcesHeight ? CombatHeight : ResistancesAndResourcesHeight;

    /// <summary>The most class resources the band has room for under the resistances.</summary>
    public const int MostResources = 2;

    // Placeholder layout (S1a's arithmetic): 34 px rows from 10 px under each band; cells 12 px in and at most 380 px
    // wide, the halves 48 px apart; the resistances in two columns of four 24 px apart; Resources 16 px under them;
    // HP's 4 px bar under its label, 12 px short of its value.
    private const int CombatRows = 7;
    private const int ResistanceRowsPerColumn = 4;
    private const int RowHeight = 34;
    private const int BelowBand = SectionTitle.Height + 10;
    private const int CellInset = 12;
    private const int CellWidth = 380;
    private const int HalfGap = 48;
    private const int ResistanceColumnGap = 24;
    private const int SectionGap = 16;
    private const int HpBarHeight = 4;
    private const int HpBarValueGap = 12;
    private const int CombatHeight = BelowBand + (CombatRows * RowHeight);
    private const int ResourcesTop = BelowBand + (ResistanceRowsPerColumn * RowHeight) + SectionGap;
    private const int ResistancesAndResourcesHeight = ResourcesTop + BelowBand + (MostResources * RowHeight);

    private readonly StatusLabels _labels;
    private readonly SheetRowLook _rows;
    private readonly SectionTitle _combatTitle;
    private readonly SectionTitle _resistancesTitle;
    private readonly SectionTitle _resourcesTitle;
    private Measured? _shown;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="labels">The combat and resistance labels, in the sheet's order.</param>
    public StatusParametersView(ScreenText text, StatusLabels labels)
    {
        _labels = labels;
        _rows = new SheetRowLook(text);
        _combatTitle = new SectionTitle(text, MessageIds.StatusCombat);
        _resistancesTitle = new SectionTitle(text, MessageIds.StatusResistances);
        _resourcesTitle = new SectionTitle(text, MessageIds.StatusResources);
    }

    /// <summary>Measures the sheet's values, when the shown member changes.</summary>
    public void Show(StatusSheet sheet) => _shown = new Measured(
        [.. sheet.Combat.Select(_rows.Measure)],
        [.. sheet.Resistances.Select(_rows.Measure)],
        sheet.ResistanceTones,
        [.. sheet.Resources.Select(resource => resource.Name)],
        [.. sheet.Resources.Select(resource => _rows.Measure(resource.Text))]);

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
        DrawResistances(frame, right, shown.Resistances, shown.ResistanceTones);
        var resources = right with { Y = right.Y + ResourcesTop };
        _resourcesTitle.Draw(batch, textures, resources, opacity);
        for (var index = 0; index < shown.Resources.Length; index++)
        {
            _rows.Draw(frame.Batch, CellAt(resources, index, resources.Width), new SheetRowLook.Cell(shown.ResourceNames[index], shown.Resources[index], UiTheme.TextPrimary), frame.Opacity);
        }
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

    private void DrawResistances(Frame frame, Rectangle section, MeasuredText[] values, ResistanceTone[] tones)
    {
        _resistancesTitle.Draw(frame.Batch, frame.Textures, section, frame.Opacity);
        var columnWidth = (section.Width - CellInset - ResistanceColumnGap) / 2;
        for (var index = 0; index < values.Length; index++)
        {
            var column = section with { X = section.X + (index / ResistanceRowsPerColumn * (columnWidth + ResistanceColumnGap)) };
            var cell = CellAt(column, index % ResistanceRowsPerColumn, columnWidth + CellInset);
            _rows.Draw(frame.Batch, cell, new SheetRowLook.Cell(_labels.Resistances[index], values[index], ColorOf(tones[index])), frame.Opacity);
        }
    }

    // A row's cell under a section's band: at most CellWidth wide, so its value stays close to its label.
    private static Rectangle CellAt(Rectangle section, int row, int columnWidth) => new(
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
        MeasuredText[] Combat, MeasuredText[] Resistances, ResistanceTone[] ResistanceTones, string[] ResourceNames, MeasuredText[] Resources);
}
