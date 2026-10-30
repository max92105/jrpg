using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// Draws the Shop's detail column: the highlighted row's name in its rarity's color, its description wrapped at the
/// column's width, how many the party owns, a rolled piece's affix lines under an Affixes band (decisions 335, 362),
/// and a line for what a trade said or gave. The description and the owned line are laid out again only when the row
/// changes, so drawing formats nothing.
/// </summary>
internal sealed class ShopDetailView
{
    // The slice's arithmetic: a 56 px name row, the description's lines, a 40 px owned line, a 46 px band (36 + 10)
    // with 40 px affix lines, a 40 px notice line, 16 px between the parts; all 10 px inside the column.
    private const int NameRowHeight = 56;
    private const int LineHeight = 40;
    private const int BandHeight = SectionTitle.Height + 10;
    private const int PartGap = 16;
    private const int Inset = 10;

    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _descriptionFont;
    private readonly SectionTitle _affixesTitle;
    private readonly UiMessages _messages;
    private TradeRow? _shownRow;
    private RichTextLayout? _description;
    private string _owned = string.Empty;

    /// <param name="text">Messages and fonts.</param>
    public ShopDetailView(ScreenText text)
    {
        _bodyFont = text.Fonts.FontAt(ShopView.BodyPixels);
        _descriptionFont = text.Fonts.FontAt(DescriptionPixels);
        _affixesTitle = new SectionTitle(text, MessageIds.StatusAffixes);
        _messages = text.Messages;
    }

    /// <summary>
    /// The description's text size (placeholder): smaller than the rows, as the Status page's skills.
    /// </summary>
    private static int DescriptionPixels => 22;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">The highlighted row, if any, the trade's line, and the column's area.</param>
    /// <param name="opacity">The Shop's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var left = place.Area.X + Inset;
        var width = place.Area.Width - (2 * Inset);
        Show(place.Row, width);
        var top = place.Area.Y;
        if (place.Row is { } row && _description is { } description)
        {
            top = DrawRow(batch, textures, row, description, new Rectangle(left, top, width, place.Area.Height), opacity);
        }

        if (place.Notice is { } notice)
        {
            batch.DrawString(_bodyFont, notice, new Vector2(left, top + ((LineHeight - _bodyFont.LineHeight) / 2f)), UiTheme.TextSecondary * opacity);
        }
    }

    // Laid out again only when the row or the width changes.
    private void Show(TradeRow? row, int width)
    {
        if (ReferenceEquals(row, _shownRow) && (_description is null || _description.Width == width))
        {
            return;
        }

        _shownRow = row;
        _description = row is null ? null : StatusSkillsView.DescriptionLayout(_descriptionFont, width, row.Item.Description);
        _owned = row is null ? string.Empty : _messages.Format(MessageIds.ShopOwned, MessageArgument.Number("count", row.OwnedCount));
    }

    // The name, the description, the owned line, then the affix lines under their band; returns where the next part
    // goes.
    private int DrawRow(SpriteBatch batch, TextureLibrary textures, TradeRow row, RichTextLayout description, Rectangle area, float opacity)
    {
        var nameColor = row.Piece is { } piece ? UiTheme.RarityColorOf(piece.Rarity) : UiTheme.TextPrimary;
        batch.DrawString(_bodyFont, row.Name.Text, new Vector2(area.X, area.Y + ((NameRowHeight - _bodyFont.LineHeight) / 2f)), nameColor * opacity);
        var top = area.Y + NameRowHeight;
        description.Draw(batch, new Vector2(area.X, top), UiTheme.TextSecondary * opacity);
        top += description.Size.Y + PartGap;
        batch.DrawString(_bodyFont, _owned, new Vector2(area.X, top + ((LineHeight - _bodyFont.LineHeight) / 2f)), UiTheme.TextPrimary * opacity);
        top += LineHeight + PartGap;
        if (row.AffixLines.Count == 0)
        {
            return top;
        }

        _affixesTitle.Draw(batch, textures, new Rectangle(area.X, top, area.Width, 0), opacity);
        top += BandHeight;
        foreach (var line in row.AffixLines)
        {
            batch.DrawString(_bodyFont, line, new Vector2(area.X, top + ((LineHeight - _bodyFont.LineHeight) / 2f)), UiTheme.TextPrimary * opacity);
            top += LineHeight;
        }

        return top + PartGap;
    }

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Row">The highlighted row of the shown list; null when it has none.</param>
    /// <param name="Notice">What a trade said or gave, under the row's parts; null for none.</param>
    /// <param name="Area">The column's area.</param>
    public readonly record struct Place(TradeRow? Row, string? Notice, Rectangle Area);
}
