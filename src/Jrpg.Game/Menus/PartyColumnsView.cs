using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Party page's two columns: the active party with its count on the left, the reserve on the right, one card
/// per member, the moving member's outline, and the notice under the active cards.
/// </summary>
internal sealed class PartyColumnsView
{
    // Placeholder layout (decision 82): a 28 x 20 margin, two columns 40 px apart, each under a section title 12 px
    // above its cards.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int ColumnGap = 40;
    private const int LabelPixels = 18;
    private const int BodyPixels = 26;
    private const int CardsTop = SectionTitle.LineGap + 12;
    private const int NoticeGap = 16;

    // The member being moved is outlined while the player picks the place to swap with.
    private const int MovingOutline = 2;

    private readonly UiMessages _messages;
    private readonly int _maxActive;
    private readonly MemberCardView _cardView;
    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SectionTitle _activeTitle;
    private readonly SectionTitle _reserveTitle;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, portraits, bars, the selected card.</param>
    /// <param name="catalog">Class names and how many members can be active.</param>
    public PartyColumnsView(ScreenText text, UiTextures ui, ContentCatalog catalog)
    {
        _messages = text.Messages;
        _maxActive = catalog.Rules.Party.MaxActiveMembers;
        _cardView = new MemberCardView(text, ui, catalog);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _activeTitle = new SectionTitle(text, ui, MessageIds.PartyActive);
        _reserveTitle = new SectionTitle(text, ui, MessageIds.PartyReserve);
    }

    /// <summary>Formats every card and the count, once per change.</summary>
    /// <param name="party">The party as it is now.</param>
    /// <param name="notice">The notice to keep showing, or null.</param>
    public PartyColumns ColumnsOf(PartyState party, string? notice)
    {
        var count = _messages.Format(
            MessageIds.PartyCount,
            MessageArgument.Number("count", party.ActiveMembers.Count),
            MessageArgument.Number("max", _maxActive));
        return new PartyColumns(
            [.. party.ActiveMembers],
            [.. party.ReserveMembers],
            party.Members.ToDictionary(member => member, member => _cardView.CardOf(member, party.Leader)),
            MeasuredText.Of(_labelFont, count),
            notice);
    }

    /// <summary>Where the active card at <paramref name="row"/> is drawn in <paramref name="area"/>.</summary>
    public static Rectangle ActiveCardArea(Rectangle area, int row) => CardArea(ActiveColumn(area), row);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">What to draw and where.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var active = ActiveColumn(place.Area);
        var reserve = active with { X = active.Right + ColumnGap };
        var columns = place.Columns;
        _activeTitle.Draw(batch, textures, active, opacity);
        batch.DrawString(_labelFont, columns.Count.Text, new Vector2(active.Right - columns.Count.Width, active.Y), UiTheme.TextSecondary * opacity);
        _reserveTitle.Draw(batch, textures, reserve, opacity);
        var divider = new Rectangle(active.Right + (ColumnGap / 2), active.Y, 1, place.Area.Bottom - active.Y);
        batch.Draw(textures.Pixel, divider, UiTheme.PageColumn.Edge * opacity);
        var cursor = place.Cursor;
        DrawCards(batch, textures, new CardColumn(columns.Active, active, cursor.IsOnReserve ? null : cursor.Row, columns.Cards, place.Moving), opacity);
        DrawCards(batch, textures, new CardColumn(columns.Reserve, reserve, cursor.IsOnReserve ? cursor.Row : null, columns.Cards, place.Moving), opacity);
        if (place.HasCursor)
        {
            var selected = CardArea(cursor.IsOnReserve ? reserve : active, cursor.Row);
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(selected.X, selected.Center.Y), opacity);
        }

        if (columns.Notice is not null)
        {
            var noticeTop = active.Y + CardsTop + (_maxActive * MemberCardView.Height) + NoticeGap;
            batch.DrawString(_bodyFont, columns.Notice, new Vector2(active.X, noticeTop), UiTheme.Danger * opacity);
        }
    }

    private static Rectangle ActiveColumn(Rectangle area)
    {
        var columnWidth = (area.Width - (2 * Margin) - ColumnGap) / 2;
        return new Rectangle(area.X + Margin, area.Y + TopMargin, columnWidth, 0);
    }

    private static Rectangle CardArea(Rectangle column, int index) =>
        new(column.X, column.Y + CardsTop + (index * MemberCardView.Height), column.Width, MemberCardView.Height);

    private void DrawCards(SpriteBatch batch, TextureLibrary textures, CardColumn column, float opacity)
    {
        var members = column.Members;
        for (var index = 0; index < members.Count; index++)
        {
            var card = CardArea(column.Area, index);
            var isSelected = index == column.SelectedRow;
            _cardView.Draw(batch, textures, new MemberCardView.Place(column.Cards[members[index]], card, isSelected, Preview: null), opacity);
            if (members[index] == column.Moving)
            {
                DrawMovingOutline(batch, textures, card);
            }
        }
    }

    // The swap only starts once the menu has faded in, so the outline draws at full opacity.
    private static void DrawMovingOutline(SpriteBatch batch, TextureLibrary textures, Rectangle card)
    {
        for (var ring = 0; ring < MovingOutline; ring++)
        {
            card.Inflate(-1, -1);
            Outline.Draw(batch, textures.Pixel, card, UiTheme.SelectionStroke);
        }
    }

    /// <summary>What to draw this frame, and where.</summary>
    /// <param name="Columns">The page's formatted text.</param>
    /// <param name="Area">The page's area.</param>
    /// <param name="Cursor">Which card is selected.</param>
    /// <param name="HasCursor">False while the action choice has it: the selected card then shows no pointer.</param>
    /// <param name="Moving">The member whose place is being swapped, or null.</param>
    public readonly record struct Place(PartyColumns Columns, Rectangle Area, PartyCursor Cursor, bool HasCursor, PartyMember? Moving);

    // One column's members, where its cards go, the cursor's row when the cursor is in it, every card, and who is moving.
    private readonly record struct CardColumn(
        IReadOnlyList<PartyMember> Members,
        Rectangle Area,
        int? SelectedRow,
        IReadOnlyDictionary<PartyMember, MemberCardView.Card> Cards,
        PartyMember? Moving);
}
