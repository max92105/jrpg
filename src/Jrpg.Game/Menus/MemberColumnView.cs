using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the <see cref="MemberColumn"/> between the menu's tabs and a member page (decisions 255, 266): a small diamond
/// portrait per member in view, the shown one in the selection look (decision 252), and a small diamond above or below
/// when members are out of view, as the party sidebar marks its scroll.
/// </summary>
internal sealed class MemberColumnView
{
    /// <summary>The column's width; the member page starts right of it (placeholder).</summary>
    public const int Width = 80;

    // Placeholder layout (decision 82): 56 px portraits in a 2 px pale frame, 72 px apart, under a 24 px band for the
    // scroll cue, whose diamond is 10 px.
    private const int PortraitSize = 56;
    private const int PortraitFrame = 2;
    private const int Pitch = 72;
    private const int CueBand = 24;
    private const int CueDiagonal = 10;

    private readonly MemberColumn _column;
    private readonly UiTextures _ui;

    /// <param name="column">The members and the shown one, refreshed by the menu.</param>
    /// <param name="ui">The selected-row and baked portrait textures.</param>
    public MemberColumnView(MemberColumn column, UiTextures ui)
    {
        _column = column;
        _ui = ui;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="area">The column's place, as tall as the page.</param>
    /// <param name="opacity">0 to 1, while the menu or the page fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var last = Math.Min(_column.Members.Count, _column.FirstVisible + MemberColumn.VisibleCount);
        for (var index = _column.FirstVisible; index < last; index++)
        {
            var slot = new Rectangle(area.X, area.Y + CueBand + ((index - _column.FirstVisible) * Pitch), area.Width, Pitch);
            if (index == _column.ShownIndex)
            {
                _ui.DrawSelectedRow(batch, textures, slot, opacity);
                MenuPointer.Draw(batch, textures.Pixel, new Vector2(slot.X, slot.Center.Y), opacity);
            }

            DrawPortrait(batch, textures, new PortraitSlot(slot, _column.Members[index].Definition.PortraitFile), opacity);
        }

        DrawScrollCues(batch, textures.Pixel, area, opacity);
    }

    // A diamond like the Status page's, in a slightly larger pale diamond.
    private void DrawPortrait(SpriteBatch batch, TextureLibrary textures, PortraitSlot slot, float opacity)
    {
        var center = slot.Area.Center.ToVector2();
        Diamond.WithDiagonal(center, PortraitSize + (2 * PortraitFrame)).Draw(batch, textures.Pixel, UiTheme.BarLine * opacity);
        var portrait = _ui.DiamondPortrait(textures, slot.PortraitFile, PortraitSize);
        batch.Draw(portrait, center - new Vector2(PortraitSize / 2f), Color.White * opacity);
    }

    private void DrawScrollCues(SpriteBatch batch, Texture2D pixel, Rectangle area, float opacity)
    {
        var color = UiTheme.TextSecondary * opacity;
        if (_column.FirstVisible > 0)
        {
            Diamond.WithDiagonal(new Vector2(area.Center.X, area.Y + (CueBand / 2f)), CueDiagonal).Draw(batch, pixel, color);
        }

        if (_column.FirstVisible + MemberColumn.VisibleCount < _column.Members.Count)
        {
            var bottom = area.Y + CueBand + (MemberColumn.VisibleCount * Pitch) + (CueBand / 2f);
            Diamond.WithDiagonal(new Vector2(area.Center.X, bottom), CueDiagonal).Draw(batch, pixel, color);
        }
    }

    // One member's place in the column and the portrait drawn there.
    private readonly record struct PortraitSlot(Rectangle Area, string PortraitFile);
}
