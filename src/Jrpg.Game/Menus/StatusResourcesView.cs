using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Status page's Resources section beside the skills (decision 368): its band, then one row per class
/// resource, in the middle band's row look (<see cref="StatusParametersView.CellAt"/>) so the two read alike.
/// </summary>
internal sealed class StatusResourcesView
{
    /// <summary>The most class resources the section has room for.</summary>
    public const int MostResources = 2;

    /// <summary>The section's height: its band, then a row per resource it has room for.</summary>
    public const int Height = StatusParametersView.BelowBand + (MostResources * StatusParametersView.RowHeight);

    private readonly SheetRowLook _rows;
    private readonly SectionTitle _title;
    private Measured? _shown;

    /// <param name="text">Messages and fonts.</param>
    public StatusResourcesView(ScreenText text)
    {
        _rows = new SheetRowLook(text);
        _title = new SectionTitle(text, MessageIds.StatusResources);
    }

    /// <summary>Measures the sheet's resources, when the shown member changes.</summary>
    public void Show(StatusSheet sheet) => _shown = new Measured(
        [.. sheet.Resources.Select(resource => resource.Name)],
        [.. sheet.Resources.Select(resource => _rows.Measure(resource.Text))]);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="section">The band's top left and the section's width.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    /// <exception cref="InvalidOperationException">No sheet was shown yet.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        var shown = _shown ?? throw new InvalidOperationException("Show a sheet before drawing it.");
        _title.Draw(batch, textures, section, opacity);
        for (var index = 0; index < shown.Values.Length; index++)
        {
            var cell = StatusParametersView.CellAt(section, index, section.Width);
            _rows.Draw(batch, cell, new SheetRowLook.Cell(shown.Names[index], shown.Values[index], UiTheme.TextPrimary), opacity);
        }
    }

    // The shown member's resources, measured once when they are shown.
    private sealed record Measured(string[] Names, MeasuredText[] Values);
}
