using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Quests page's detail column (decision 392): in the side column, the quest's title in the title serif, MAIN
/// or SIDE under it in spaced caps, the description wrapped, then an Objective band with the stage's text wrapped (or
/// the completed note). The texts are wrapped when the detail changes, never per frame.
/// </summary>
internal sealed class QuestDetailView
{
    /// <summary>The gap between the list column and this one, and from this one to the page's right edge.</summary>
    public const int Gap = 40;

    /// <summary>The description's and the stage text's font size; the fit test counts lines with it.</summary>
    internal const int TextPixels = 22;

    // The slice's arithmetic: a 20 px top margin, the title (38 px serif), the kind in 18 px caps, a 16 px gap, the
    // description's lines (22 px), a 16 px gap, the band (36 + 10), the stage text's lines; all 28 px inside the
    // column.
    private const int TitlePixels = 38;
    private const int CapsPixels = 18;
    private const int PartGap = 16;
    private const int BandHeight = SectionTitle.Height + 10;

    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _capsFont;
    private readonly SpriteFontBase _textFont;
    private readonly SectionTitle _objective;
    private readonly ColumnPanelView _panel = new(UiTheme.SideColumn);
    private Wrapped? _wrapped;

    /// <param name="text">The fonts and the band's label.</param>
    public QuestDetailView(ScreenText text)
    {
        _titleFont = text.Fonts.TitleFontAt(TitlePixels);
        _capsFont = text.Fonts.FontAt(CapsPixels);
        _textFont = text.Fonts.FontAt(TextPixels);
        _objective = new SectionTitle(text, MessageIds.QuestsObjective);
    }

    /// <summary>How many times the texts were wrapped; the test seam for "once per change".</summary>
    internal int WrapCount { get; private set; }

    /// <summary>The height of everything but the two wrapped texts: what they share the column with.</summary>
    internal int FixedHeight => PageListLayout.TopMargin + _titleFont.LineHeight + _capsFont.LineHeight + PartGap + PartGap + BandHeight;

    /// <summary>Where the column goes beside a list column of <paramref name="listWidth"/> in the page area.</summary>
    public static Rectangle ColumnIn(Rectangle area, int listWidth)
    {
        var left = area.X + listWidth + Gap;
        return new Rectangle(left, area.Y, area.Right - Gap - left, area.Height);
    }

    /// <summary>
    /// Lays the detail out for a column <paramref name="width"/> wide, when either changed, and tells the height the
    /// parts take from the column's top; nothing for no detail.
    /// </summary>
    internal int Show(QuestDetail? detail, int width)
    {
        if (detail is null)
        {
            _wrapped = null;
            return 0;
        }

        if (!ReferenceEquals(detail, _wrapped?.Detail) || width != _wrapped.Width)
        {
            var textWidth = width - (2 * PageListLayout.Margin);
            _wrapped = new Wrapped(
                detail,
                width,
                detail.KindName.ToUpperInvariant(),
                StatusSkillsView.DescriptionLayout(_textFont, textWidth, detail.Description),
                StatusSkillsView.DescriptionLayout(_textFont, textWidth, detail.StageText));
            WrapCount++;
        }

        return FixedHeight + _wrapped.Description.Size.Y + _wrapped.Stage.Size.Y;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the band come from.</param>
    /// <param name="place">The chosen quest's detail, if any, and the column.</param>
    /// <param name="opacity">The menu's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var column = place.Column;
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(column), opacity);
        Show(place.Detail, column.Width);
        if (_wrapped is not { } wrapped)
        {
            return;
        }

        var left = column.X + PageListLayout.Margin;
        var width = column.Width - (2 * PageListLayout.Margin);
        var top = column.Y + PageListLayout.TopMargin;
        batch.DrawString(_titleFont, wrapped.Detail.Title, new Vector2(left, top), UiTheme.TextPrimary * opacity);
        top += _titleFont.LineHeight;
        batch.DrawString(_capsFont, wrapped.Caps, new Vector2(left, top), UiTheme.TextSecondary * opacity, characterSpacing: SheetRowLook.LabelSpacing);
        top += _capsFont.LineHeight + PartGap;
        wrapped.Description.Draw(batch, new Vector2(left, top), UiTheme.TextSecondary * opacity);
        top += wrapped.Description.Size.Y + PartGap;
        _objective.Draw(batch, textures, new Rectangle(left, top, width, 0), opacity);
        top += BandHeight;
        wrapped.Stage.Draw(batch, new Vector2(left, top), UiTheme.TextPrimary * opacity);
    }

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Detail">The chosen quest's detail; null while the shown category lists no quest.</param>
    /// <param name="Column">The column's area, from <see cref="ColumnIn"/>.</param>
    public readonly record struct Place(QuestDetail? Detail, Rectangle Column);

    // One detail's texts laid out for one column width; replaced whole when either changes.
    private sealed record Wrapped(QuestDetail Detail, int Width, string Caps, RichTextLayout Description, RichTextLayout Stage);
}
