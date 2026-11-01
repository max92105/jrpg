using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A row of pointed tabs with their labels (decision 355, the victory card's tab): the shown one lit, the others dim.
/// The labels are measured once. The Quests page's categories are its first user; the victory's Items step and the
/// Shop's Buy and Sell tabs draw their own until next touched (the backlog).
/// </summary>
internal sealed class PointedTabsView
{
    /// <summary>The row's height; a tab sits centered in it.</summary>
    public const int Height = 48;

    // The gap between two tabs (placeholder, the Shop's).
    private const int TabGap = 24;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly MeasuredText[] _labels;

    /// <param name="ui">Where the tab's shape comes from.</param>
    /// <param name="font">The labels' font.</param>
    /// <param name="labels">The tabs' labels, in order.</param>
    public PointedTabsView(UiTextures ui, SpriteFontBase font, IReadOnlyList<string> labels)
    {
        _ui = ui;
        _font = font;
        _labels = [.. labels.Select(label => MeasuredText.Of(font, label))];
    }

    /// <summary>
    /// Where the tab at <paramref name="index"/> of <paramref name="count"/> goes in <paramref name="row"/>: the card
    /// header's width when the row has room, else narrowed so the tabs and their gaps fill the row (the Quests column
    /// is narrower than the Shop's).
    /// </summary>
    public static Rectangle TabAt(Rectangle row, int index, int count)
    {
        var width = Math.Min(PointedBarShape.CardHeaderWidth, (row.Width - (TabGap * (count - 1))) / count);
        return new Rectangle(row.X + (index * (width + TabGap)), row.Y + ((Height - PointedBarShape.CardHeaderHeight) / 2), width, PointedBarShape.CardHeaderHeight);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the tab's shape is made.</param>
    /// <param name="place">The row the tabs start in and which one is shown.</param>
    /// <param name="opacity">The screen's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        for (var index = 0; index < _labels.Length; index++)
        {
            var tab = TabAt(place.Row, index, _labels.Length);
            var tabOpacity = opacity * (index == place.Shown ? 1 : UiTheme.DimmedSelectionOpacity);
            batch.Draw(_ui.CardHeader(textures), tab, Color.White * tabOpacity);
            var label = _labels[index];
            batch.DrawString(_font, label.Text, new Vector2(tab.Center.X - (label.Width / 2f), tab.Center.Y - (_font.LineHeight / 2f)), UiTheme.TextPrimary * tabOpacity);
        }
    }

    /// <summary>What the row draws this frame.</summary>
    /// <param name="Row">
    /// The row the tabs fit in: they run right from its left edge, centered in <see cref="Height"/>, no wider than it.
    /// </param>
    /// <param name="Shown">The lit tab's index.</param>
    public readonly record struct Place(Rectangle Row, int Shown);
}
