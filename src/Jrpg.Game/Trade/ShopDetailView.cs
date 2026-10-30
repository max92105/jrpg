using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Core.Party;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// Draws the Shop's detail column: the highlighted row's name in its rarity's color, its description wrapped at the
/// column's width, how many the party owns, a rolled piece's affix lines under an Affixes band (decisions 335, 362),
/// then, on a gear row, the previewed member's name and what wearing the piece would change (decision 383, the
/// Equipment page's Changes), and a line for what a trade said or gave. The description and the owned line are laid out
/// again only when the row changes, so drawing formats nothing.
/// </summary>
internal sealed class ShopDetailView
{
    // The slice's arithmetic: a 56 px name row, the description's lines, a 40 px owned line, a 46 px band (36 + 10)
    // with 40 px affix lines, a 40 px member line over the Changes section, a 40 px notice line, 16 px between the
    // parts; all 10 px inside the column.
    private const int NameRowHeight = 56;
    private const int LineHeight = 40;
    private const int BandHeight = SectionTitle.Height + 10;
    private const int PartGap = 16;
    private const int Inset = 10;

    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _descriptionFont;
    private readonly SectionTitle _affixesTitle;
    private readonly EquipmentChangesView _changes;
    private readonly UiMessages _messages;
    private readonly Rectangle _inset;
    private TradeRow? _shownRow;
    private RichTextLayout? _description;
    private string _owned = string.Empty;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="area">The column's area; the parts are drawn 10 px inside it.</param>
    public ShopDetailView(ScreenText text, Rectangle area)
    {
        _inset = new Rectangle(area.X + Inset, area.Y, area.Width - (2 * Inset), area.Height);
        _bodyFont = text.Fonts.FontAt(ShopView.BodyPixels);
        _descriptionFont = text.Fonts.FontAt(DescriptionPixels);
        _affixesTitle = new SectionTitle(text, MessageIds.StatusAffixes);
        _changes = new EquipmentChangesView(text, new SheetRowLook(text));
        _messages = text.Messages;
    }

    /// <summary>
    /// The description's text size (placeholder): smaller than the rows, as the Status page's skills.
    /// </summary>
    private static int DescriptionPixels => 22;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">
    /// The highlighted row, if any, the previewed member and their changes, and the trade's line.
    /// </param>
    /// <param name="opacity">The Shop's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        Show(place.Row);
        var top = _inset.Y;
        if (place.Row is { } row && _description is { } description)
        {
            top = DrawRow(batch, textures, row, description, _inset, opacity);
        }

        if (place.Member is { } member && place.Changes is { } changes)
        {
            top = DrawPreview(batch, textures, member, changes, _inset with { Y = top }, opacity);
        }

        if (place.Notice is { } notice)
        {
            batch.DrawString(_bodyFont, notice, new Vector2(_inset.X, top + ((LineHeight - _bodyFont.LineHeight) / 2f)), UiTheme.TextSecondary * opacity);
        }
    }

    // Laid out again only when the row changes.
    private void Show(TradeRow? row)
    {
        if (ReferenceEquals(row, _shownRow))
        {
            return;
        }

        _shownRow = row;
        _description = row is null ? null : StatusSkillsView.DescriptionLayout(_descriptionFont, _inset.Width, row.Item.Description);
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

    // The member's name, then the Equipment page's Changes section (decision 383); returns where the next part goes.
    private int DrawPreview(SpriteBatch batch, TextureLibrary textures, PartyMember member, StatComparisonText changes, Rectangle area, float opacity)
    {
        batch.DrawString(_bodyFont, member.Definition.Name, new Vector2(area.X, area.Y + ((LineHeight - _bodyFont.LineHeight) / 2f)), UiTheme.TextPrimary * opacity);
        var section = new Rectangle(area.X, area.Y + LineHeight + PartGap, area.Width, 0);
        _changes.Draw(batch, textures, new EquipmentChangesView.Place(section, changes, MessageIds.EquipmentChanges), opacity);
        return section.Y + EquipmentChangesView.Height + PartGap;
    }

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Row">The highlighted row of the shown list; null when it has none.</param>
    /// <param name="Member">The member previewed on a gear row; null on any other row.</param>
    /// <param name="Changes">
    /// What wearing the row's piece would change for <paramref name="Member"/>; null with no preview.
    /// </param>
    /// <param name="Notice">What a trade said or gave, under the row's parts; null for none.</param>
    public readonly record struct Place(TradeRow? Row, PartyMember? Member, StatComparisonText? Changes, string? Notice);
}
