using System.Globalization;
using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's Party page (legacy <c>PartyView</c>): the active party with its count on the left, the reserve on the
/// right, one card per member. Confirm sends an active member to the reserve or brings a reserve member in; Core keeps
/// the leader and refuses a full party, and the page says why. Changing the order comes in P4-2.
/// </summary>
internal sealed class PartyPage : IMenuPage
{
    // Legacy PartyView: a 28 x 20 margin, two columns 40 px apart, each under a section title 12 px above its cards.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int ColumnGap = 40;
    private const int LabelPixels = 14;
    private const int NoticePixels = 18;
    private const int SectionLineGap = 20;
    private const int CardsTop = SectionLineGap + 12;
    private const int NoticeGap = 16;
    private const int ActiveColumn = 0;
    private const int ReserveColumn = 1;

    private readonly PartyState _party;
    private readonly int _maxActive;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly PartyMemberCardView _cardView;
    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _noticeFont;
    private readonly string _activeTitle;
    private readonly string _reserveTitle;
    private int _column = ActiveColumn;
    private IReadOnlyList<PartyMember> _reserve = [];
    private Dictionary<PartyMember, PartyMemberCardView.Card> _cards = [];
    private PartyMemberCardView.MeasuredText _count;
    private int _row;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, portraits, bars, the selected card.</param>
    /// <param name="session">Whose party.</param>
    /// <param name="catalog">Class names and how many members can be active.</param>
    public PartyPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        _party = session.Party;
        _maxActive = catalog.Rules.Party.MaxActiveMembers;
        _messages = text.Messages;
        _ui = ui;
        _cardView = new PartyMemberCardView(text, ui, catalog);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _noticeFont = text.Fonts.FontAt(NoticePixels);
        _activeTitle = text.Messages.Text(MessageIds.PartyActive);
        _reserveTitle = text.Messages.Text(MessageIds.PartyReserve);
        ShowParty();
    }

    public string TitleMessageId => MessageIds.MenuParty;

    public bool LeavesOnCancel => true;

    /// <summary>The member under the cursor.</summary>
    public PartyMember Selected => MembersIn(_column)[_row];

    /// <summary>Why the last Confirm changed nothing, or null.</summary>
    public string? Notice { get; private set; }

    private bool IsInReserve => _column == ReserveColumn;

    public SceneRequest? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            MoveInColumn(-1);
        }

        if (input.IsPressed(GameButton.Down))
        {
            MoveInColumn(1);
        }

        if (input.IsPressed(GameButton.Left) || input.IsPressed(GameButton.Right))
        {
            SwitchColumn();
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            MoveSelectedMember();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var columnWidth = (area.Width - (2 * Margin) - ColumnGap) / 2;
        var top = area.Y + TopMargin;
        var active = new Rectangle(area.X + Margin, top, columnWidth, 0);
        var reserve = new Rectangle(active.Right + ColumnGap, top, columnWidth, 0);
        DrawSectionTitle(batch, textures, _activeTitle, active, opacity);
        batch.DrawString(_labelFont, _count.Text, new Vector2(active.Right - _count.Width, top), UiTheme.TextSecondary * opacity);
        DrawSectionTitle(batch, textures, _reserveTitle, reserve, opacity);
        DrawCards(batch, textures, ActiveColumn, active, opacity);
        DrawCards(batch, textures, ReserveColumn, reserve, opacity);
        if (Notice is not null)
        {
            var noticeTop = top + CardsTop + (_maxActive * PartyMemberCardView.Height) + NoticeGap;
            batch.DrawString(_noticeFont, Notice, new Vector2(active.X, noticeTop), UiTheme.Danger * opacity);
        }
    }

    private IReadOnlyList<PartyMember> MembersIn(int column) => column == ActiveColumn ? _party.ActiveMembers : _reserve;

    private void MoveInColumn(int step)
    {
        Notice = null;
        var count = MembersIn(_column).Count;
        _row = (_row + step + count) % count;
    }

    // To the other column at the same row, or its last card when it's shorter (developer, 2026-10-20).
    private void SwitchColumn()
    {
        var other = _column == ActiveColumn ? ReserveColumn : ActiveColumn;
        if (MembersIn(other).Count == 0)
        {
            return;
        }

        Notice = null;
        _column = other;
        _row = Math.Min(_row, MembersIn(other).Count - 1);
    }

    private void MoveSelectedMember()
    {
        var member = Selected;
        Notice = null;
        if (IsInReserve && !_party.TryAddToActive(member))
        {
            Notice = _messages.Text(MessageIds.PartyFull);
        }
        else if (!IsInReserve && !_party.TryMoveToReserve(member))
        {
            Notice = _messages.Format(MessageIds.PartyLeaderStays, new MessageArgument("name", member.Definition.Name));
        }

        ShowParty();
        KeepCursorOnACard();
    }

    // A member who moved leaves a gap: the next card slides into the cursor's place, or the cursor steps back to the
    // last one. The reserve can empty out (four recruits or fewer); the active party never does.
    private void KeepCursorOnACard()
    {
        if (MembersIn(_column).Count == 0)
        {
            _column = ActiveColumn;
        }

        _row = Math.Min(_row, MembersIn(_column).Count - 1);
    }

    // Formats every card and the count once per change, so drawing formats nothing.
    private void ShowParty()
    {
        _reserve = [.. _party.Members.Where(member => !_party.ActiveMembers.Contains(member))];
        _cards = _party.Members.ToDictionary(member => member, member => _cardView.CardOf(member, _party.Leader));
        var count = _messages.Format(
            MessageIds.PartyCount,
            new MessageArgument("count", _party.ActiveMembers.Count.ToString(CultureInfo.InvariantCulture)),
            new MessageArgument("max", _maxActive.ToString(CultureInfo.InvariantCulture)));
        _count = new PartyMemberCardView.MeasuredText(count, _labelFont.MeasureString(count).X);
    }

    private void DrawCards(SpriteBatch batch, TextureLibrary textures, int column, Rectangle columnArea, float opacity)
    {
        var members = MembersIn(column);
        for (var index = 0; index < members.Count; index++)
        {
            var card = new Rectangle(columnArea.X, columnArea.Y + CardsTop + (index * PartyMemberCardView.Height), columnArea.Width, PartyMemberCardView.Height);
            var isSelected = column == _column && index == _row;
            _cardView.Draw(batch, textures, new PartyMemberCardView.Place(_cards[members[index]], card, isSelected), opacity);
        }
    }

    // Legacy SectionHeaderStyle: a small label over a hairline that fades out to the right.
    private void DrawSectionTitle(SpriteBatch batch, TextureLibrary textures, string title, Rectangle section, float opacity)
    {
        batch.DrawString(_labelFont, title, new Vector2(section.X, section.Y), UiTheme.TextSecondary * opacity);
        var line = new Rectangle(section.X, section.Y + SectionLineGap, section.Width, 1);
        batch.Draw(_ui.SectionLine(textures), line, Color.White * opacity);
    }
}
