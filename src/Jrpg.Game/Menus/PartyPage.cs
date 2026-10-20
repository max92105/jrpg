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
/// right, one card per member. Confirm brings a reserve member in; on an active member it offers "Change order" (pick
/// the place to swap with) or "To reserve". Core keeps the leader first, refuses a full party and never leaves the active
/// party entirely fallen; the page asks it what a member may do and says why it refused.
/// </summary>
internal sealed class PartyPage : IMenuPage
{
    // Legacy PartyView: a 28 x 20 margin, two columns 40 px apart, each under a section title 12 px above its cards.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int ColumnGap = 40;
    private const int LabelPixels = 14;
    private const int BodyPixels = 18;
    private const int SectionLineGap = 20;
    private const int CardsTop = SectionLineGap + 12;
    private const int NoticeGap = 16;
    private const int ActiveColumn = 0;
    private const int ReserveColumn = 1;

    // The action choice: a 240 px glass panel of 44 px rows over the right of the chosen card (legacy ListRow height).
    private const int ChoiceWidth = 240;
    private const int ChoiceRowHeight = 44;
    private const int ChoiceInset = 16;
    private const int ChangeOrderEntry = 0;

    // The member being moved is outlined while the player picks the place to swap with.
    private const int MovingOutline = 2;

    private readonly PartyState _party;
    private readonly int _maxActive;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly PartyMemberCardView _cardView;
    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _activeTitle;
    private readonly string _reserveTitle;
    private readonly string[] _actions;
    private int _column = ActiveColumn;
    private Dictionary<PartyMember, PartyMemberCardView.Card> _cards = [];
    private PartyMemberCardView.MeasuredText _count;
    private int _row;

    // Where the moving member stood, so Cancel puts the cursor back on them.
    private int _movingFromRow;

    // While an active member's actions are offered; null otherwise.
    private ListCursor? _actionChoice;

    // Whether Core lets the chosen member leave, worked out when the choice opens so drawing asks nothing.
    private bool _canMoveSelectedToReserve;

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
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _activeTitle = text.Messages.Text(MessageIds.PartyActive);
        _reserveTitle = text.Messages.Text(MessageIds.PartyReserve);
        _actions = [text.Messages.Text(MessageIds.PartyChangeOrder), text.Messages.Text(MessageIds.PartyToReserve)];
        ShowParty();
    }

    public string TitleMessageId => MessageIds.MenuParty;

    /// <summary>False while the actions are offered or a place is being picked: Cancel backs out of those first.</summary>
    public bool LeavesOnCancel => _actionChoice is null && MovingMember is null;

    /// <summary>The member under the cursor.</summary>
    public PartyMember Selected => MembersIn(_column)[_row];

    /// <summary>The member whose place is being swapped, while the player picks where; null otherwise.</summary>
    public PartyMember? MovingMember { get; private set; }

    /// <summary>Why the last Confirm changed nothing, or null.</summary>
    public string? Notice { get; private set; }

    /// <summary>The member's card as last formatted.</summary>
    internal PartyMemberCardView.Card CardOf(PartyMember member) => _cards[member];

    public void Open()
    {
        Notice = null;
        _column = ActiveColumn;
        _row = 0;
        ShowParty();
    }

    public SceneRequest? Update(InputFrame input)
    {
        if (_actionChoice is not null)
        {
            UpdateActionChoice(_actionChoice, input);
        }
        else if (MovingMember is not null)
        {
            UpdateSwap(MovingMember, input);
        }
        else
        {
            UpdateBrowsing(input);
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
        if (_actionChoice is not null)
        {
            DrawActionChoice(batch, textures, _actionChoice, CardArea(active, _row));
        }

        if (Notice is not null)
        {
            var noticeTop = top + CardsTop + (_maxActive * PartyMemberCardView.Height) + NoticeGap;
            batch.DrawString(_bodyFont, Notice, new Vector2(active.X, noticeTop), UiTheme.Danger * opacity);
        }
    }

    private static Rectangle CardArea(Rectangle column, int index) =>
        new(column.X, column.Y + CardsTop + (index * PartyMemberCardView.Height), column.Width, PartyMemberCardView.Height);

    private IReadOnlyList<PartyMember> MembersIn(int column) => column == ActiveColumn ? _party.ActiveMembers : _party.ReserveMembers;

    private void UpdateBrowsing(InputFrame input)
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
            ConfirmSelected();
        }
    }

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

    private void ConfirmSelected()
    {
        var member = Selected;
        Notice = null;
        if (_column == ReserveColumn)
        {
            Join(member);
        }
        else if (!_party.CanChangePlace(member) && !_party.CanMoveToReserve(member))
        {
            Notice = _messages.Format(MessageIds.PartyLeaderStays, new MessageArgument("name", member.Definition.Name));
        }
        else
        {
            // Greyed actions stay selectable, so Confirm on one can say why (battle decision 105).
            _canMoveSelectedToReserve = _party.CanMoveToReserve(member);
            _actionChoice = new ListCursor(_actions.Length, _ => true);
        }
    }

    private void Join(PartyMember member)
    {
        if (!_party.TryAddToActive(member))
        {
            Notice = _messages.Text(MessageIds.PartyFull);
            return;
        }

        ShowParty();
        KeepCursorOnACard();
    }

    private void UpdateActionChoice(ListCursor choice, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            _actionChoice = null;
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            choice.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            choice.MoveDown();
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return;
        }

        _actionChoice = null;
        if (choice.Selected == ChangeOrderEntry)
        {
            MovingMember = Selected;
            _movingFromRow = _row;
            return;
        }

        if (!_party.TryMoveToReserve(Selected))
        {
            Notice = _messages.Format(MessageIds.PartyNeedsSomeoneStanding, new MessageArgument("name", Selected.Definition.Name));
            return;
        }

        ShowParty();
        KeepCursorOnACard();
    }

    // The cursor moves over the active places Core lets change, so never onto the leader's.
    private void UpdateSwap(PartyMember moving, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            MovingMember = null;
            _row = _movingFromRow;
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            MoveToNextPlaceToSwap(-1);
        }

        if (input.IsPressed(GameButton.Down))
        {
            MoveToNextPlaceToSwap(1);
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            _party.Reorder(PartyOrderEdit.Swap(_party.ActiveMembers, moving, Selected));
            MovingMember = null;
            ShowParty();
        }
    }

    // The moving member can change place, so the loop always finds a place to land on.
    private void MoveToNextPlaceToSwap(int step)
    {
        var count = _party.ActiveMembers.Count;
        do
        {
            _row = (_row + step + count) % count;
        }
        while (!_party.CanChangePlace(_party.ActiveMembers[_row]));
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
            var card = CardArea(columnArea, index);
            var isSelected = column == _column && index == _row;
            _cardView.Draw(batch, textures, new PartyMemberCardView.Place(_cards[members[index]], card, isSelected), opacity);
            if (members[index] == MovingMember)
            {
                DrawMovingOutline(batch, textures, card);
            }
        }
    }

    // The choice and the swap only open once the menu has faded in, so they draw at full opacity.
    private static void DrawMovingOutline(SpriteBatch batch, TextureLibrary textures, Rectangle card)
    {
        for (var ring = 0; ring < MovingOutline; ring++)
        {
            card.Inflate(-1, -1);
            Outline.Draw(batch, textures.Pixel, card, UiTheme.SelectionStroke);
        }
    }

    // Legacy GlassPanelStyle: the Surface fill with a faint outline, one ListRow per action.
    private void DrawActionChoice(SpriteBatch batch, TextureLibrary textures, ListCursor choice, Rectangle card)
    {
        var panel = new Rectangle(card.Right - ChoiceWidth - ChoiceInset, card.Y + ChoiceInset, ChoiceWidth, _actions.Length * ChoiceRowHeight);
        batch.Draw(textures.Pixel, panel, UiTheme.Surface);
        Outline.Draw(batch, textures.Pixel, panel, UiTheme.Stroke);
        for (var index = 0; index < _actions.Length; index++)
        {
            var row = new Rectangle(panel.X, panel.Y + (index * ChoiceRowHeight), panel.Width, ChoiceRowHeight);
            var isSelected = index == choice.Selected;
            if (isSelected)
            {
                _ui.DrawSelectedRow(batch, textures, row, 1);
            }

            var textTop = row.Y + ((ChoiceRowHeight - _bodyFont.LineHeight) / 2);
            batch.DrawString(_bodyFont, _actions[index], new Vector2(row.X + ChoiceInset, textTop), ActionColor(index, isSelected));
        }
    }

    private Color ActionColor(int index, bool isSelected)
    {
        if (index != ChangeOrderEntry && !_canMoveSelectedToReserve)
        {
            return UiTheme.TextDisabled;
        }

        return isSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
    }

    // Legacy SectionHeaderStyle: a small label over a hairline that fades out to the right.
    private void DrawSectionTitle(SpriteBatch batch, TextureLibrary textures, string title, Rectangle section, float opacity)
    {
        batch.DrawString(_labelFont, title, new Vector2(section.X, section.Y), UiTheme.TextSecondary * opacity);
        var line = new Rectangle(section.X, section.Y + SectionLineGap, section.Width, 1);
        batch.Draw(_ui.SectionLine(textures), line, Color.White * opacity);
    }
}
