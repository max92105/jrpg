using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Core.Party;
using Jrpg.Core.Trade;
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
/// Equipment page's Changes), and the trade's line: the open quantity picker with its total (decision 377), else a
/// refused row's reason or what was bought (decision 370). The description, the owned line and the picker's count are
/// laid out again only when they change, so drawing formats nothing.
/// </summary>
internal sealed class ShopDetailView
{
    // The slice's arithmetic: a 56 px name row, the description's lines, a 40 px owned line, a 46 px band (36 + 10)
    // with 40 px affix lines, a 40 px member line over the Changes section, a 40 px trade line, 16 px between the
    // parts; all 10 px inside the column. The picker's count sits between two small diamonds (decision 253, the game's
    // pointer glyph) after the Quantity label; its total ends at the column's right edge.
    private const int NameRowHeight = 56;
    private const int LineHeight = 40;
    private const int BandHeight = SectionTitle.Height + 10;
    private const int PartGap = 16;
    private const int Inset = 10;
    private const int LabelGap = 24;
    private const int DiamondDiagonal = 12;
    private const int DiamondGap = 10;

    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _descriptionFont;
    private readonly SectionTitle _affixesTitle;
    private readonly EquipmentChangesView _changes;
    private readonly GoldAmountView _gold;
    private readonly MeasuredText _quantityLabel;
    private readonly UiMessages _messages;
    private readonly Rectangle _inset;
    private TradeRow? _shownRow;
    private RichTextLayout? _description;
    private string _owned = string.Empty;
    private TradeQuote? _shownQuote;
    private MeasuredText _count;
    private MeasuredText _total;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the total's coin comes from.</param>
    /// <param name="area">The column's area; the parts are drawn 10 px inside it.</param>
    public ShopDetailView(ScreenText text, UiTextures ui, Rectangle area)
    {
        _inset = new Rectangle(area.X + Inset, area.Y, area.Width - (2 * Inset), area.Height);
        _bodyFont = text.Fonts.FontAt(ShopView.BodyPixels);
        _descriptionFont = text.Fonts.FontAt(DescriptionPixels);
        _affixesTitle = new SectionTitle(text, MessageIds.StatusAffixes);
        _changes = new EquipmentChangesView(text, new SheetRowLook(text));
        _gold = new GoldAmountView(_bodyFont, ui);
        _quantityLabel = MeasuredText.Of(_bodyFont, text.Messages.Text(MessageIds.ShopQuantity));
        _messages = text.Messages;
    }

    /// <summary>
    /// The description's text size (placeholder): smaller than the rows, as the Status page's skills.
    /// </summary>
    private static int DescriptionPixels => 22;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the coin come from.</param>
    /// <param name="place">The highlighted row, if any, the previewed member's changes, and the trade's line.</param>
    /// <param name="opacity">The Shop's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        Show(place.Row);
        var top = _inset.Y;
        if (place.Row is { } row && _description is { } description)
        {
            top = DrawRow(batch, textures, row, description, _inset, opacity);
        }

        if (place.Preview is { } preview)
        {
            top = DrawPreview(batch, textures, preview, _inset with { Y = top }, opacity);
        }

        if (place.Picker is { } picker)
        {
            DrawPicker(batch, textures, picker, _inset with { Y = top }, opacity);
        }
        else if (place.Notice is { } notice)
        {
            batch.DrawString(_bodyFont, notice, new Vector2(_inset.X, TextTop(top)), UiTheme.TextSecondary * opacity);
        }
    }

    private float TextTop(int lineTop) => lineTop + ((LineHeight - _bodyFont.LineHeight) / 2f);

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

    // The quote changes exactly when the count does, and differs between pickers, so it tells when to measure again.
    private void ShowCount(QuantityPicker picker)
    {
        if (ReferenceEquals(picker.Quote, _shownQuote))
        {
            return;
        }

        _shownQuote = picker.Quote;
        _count = MeasuredText.Of(_bodyFont, NumberText.Of(picker.Count));
        _total = MeasuredText.Of(_bodyFont, NumberText.Of(picker.Quote.Gold));
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
        batch.DrawString(_bodyFont, _owned, new Vector2(area.X, TextTop(top)), UiTheme.TextPrimary * opacity);
        top += LineHeight + PartGap;
        if (row.AffixLines.Count == 0)
        {
            return top;
        }

        _affixesTitle.Draw(batch, textures, new Rectangle(area.X, top, area.Width, 0), opacity);
        top += BandHeight;
        foreach (var line in row.AffixLines)
        {
            batch.DrawString(_bodyFont, line, new Vector2(area.X, TextTop(top)), UiTheme.TextPrimary * opacity);
            top += LineHeight;
        }

        return top + PartGap;
    }

    // The member's name, then the Equipment page's Changes section (decision 383); returns where the next part goes.
    private int DrawPreview(SpriteBatch batch, TextureLibrary textures, MemberPreview preview, Rectangle area, float opacity)
    {
        batch.DrawString(_bodyFont, preview.Member.Definition.Name, new Vector2(area.X, TextTop(area.Y)), UiTheme.TextPrimary * opacity);
        var section = new Rectangle(area.X, area.Y + LineHeight + PartGap, area.Width, 0);
        _changes.Draw(batch, textures, new EquipmentChangesView.Place(section, preview.Changes, MessageIds.EquipmentChanges), opacity);
        return section.Y + EquipmentChangesView.Height + PartGap;
    }

    // "Quantity" then the count between its diamonds at the left, the total with its coin at the right.
    private void DrawPicker(SpriteBatch batch, TextureLibrary textures, QuantityPicker picker, Rectangle area, float opacity)
    {
        ShowCount(picker);
        var textTop = TextTop(area.Y);
        var centerY = area.Y + (LineHeight / 2f);
        var color = UiTheme.TextPrimary * opacity;
        batch.DrawString(_bodyFont, _quantityLabel.Text, new Vector2(area.X, textTop), UiTheme.TextSecondary * opacity);
        var leftDiamond = area.X + _quantityLabel.Width + LabelGap + (DiamondDiagonal / 2f);
        Diamond.WithDiagonal(new Vector2(leftDiamond, centerY), DiamondDiagonal).Draw(batch, textures.Pixel, color);
        var countLeft = leftDiamond + (DiamondDiagonal / 2f) + DiamondGap;
        batch.DrawString(_bodyFont, _count.Text, new Vector2(countLeft, textTop), color);
        var rightDiamond = countLeft + _count.Width + DiamondGap + (DiamondDiagonal / 2f);
        Diamond.WithDiagonal(new Vector2(rightDiamond, centerY), DiamondDiagonal).Draw(batch, textures.Pixel, color);
        var totalLeft = area.Right - _gold.WidthOf(_total);
        _gold.Draw(batch, textures, new GoldAmountView.Place(_total, new Vector2(totalLeft, textTop), UiTheme.TextPrimary), opacity);
    }

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Row">The highlighted row of the shown list; null when it has none.</param>
    /// <param name="Preview">The member previewed on a gear row and their changes; null on any other row.</param>
    /// <param name="Notice">
    /// A refused row's reason or the last trade's notice, under the row's parts; null for none.
    /// </param>
    /// <param name="Picker">The open quantity picker, drawn in the notice's place; null while none is open.</param>
    public readonly record struct Place(TradeRow? Row, MemberPreview? Preview, string? Notice, QuantityPicker? Picker);

    /// <summary>One member and what wearing the highlighted piece would change for them (decision 383).</summary>
    public readonly record struct MemberPreview(PartyMember Member, StatComparisonText Changes);
}
