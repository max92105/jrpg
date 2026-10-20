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
    private readonly PartyState _party;
    private readonly UiMessages _messages;
    private readonly PartyColumnsView _columnsView;
    private readonly PartyActionChoiceView _choiceView;
    private readonly PartyCursor _cursor;
    private PartyColumns _columns;

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
        _messages = text.Messages;
        _columnsView = new PartyColumnsView(text, ui, catalog);
        _choiceView = new PartyActionChoiceView(text, ui);
        _cursor = new PartyCursor(_party);
        _columns = _columnsView.ColumnsOf(_party, notice: null);
    }

    public string TitleMessageId => MessageIds.MenuParty;

    /// <summary>False while the actions are offered or a place is being picked: Cancel backs out of those first.</summary>
    public bool LeavesOnCancel => _actionChoice is null && MovingMember is null;

    /// <summary>The member under the cursor.</summary>
    public PartyMember Selected => _cursor.Selected;

    /// <summary>The member whose place is being swapped, while the player picks where; null otherwise.</summary>
    public PartyMember? MovingMember { get; private set; }

    /// <summary>Why the last Confirm changed nothing, or null.</summary>
    public string? Notice
    {
        get => _columns.Notice;
        private set => _columns = _columns with { Notice = value };
    }

    /// <summary>The member's card as last formatted.</summary>
    internal MemberCardView.Card CardOf(PartyMember member) => _columns.Cards[member];

    public void Open()
    {
        Notice = null;
        _cursor.Reset();
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

    // The action choice is offered on an active card only, over its right.
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        _columnsView.Draw(batch, textures, new PartyColumnsView.Place(_columns, area, _cursor, MovingMember), opacity);
        if (_actionChoice is not null)
        {
            var card = PartyColumnsView.ActiveCardArea(area, _cursor.Row);
            _choiceView.Draw(batch, textures, new PartyActionChoiceView.Place(_actionChoice, card, _canMoveSelectedToReserve));
        }
    }

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
        _cursor.MoveInColumn(step);
    }

    // The notice stays when there's no other column to go to, as nothing moved.
    private void SwitchColumn()
    {
        var wasOnReserve = _cursor.IsOnReserve;
        _cursor.SwitchColumn();
        if (_cursor.IsOnReserve != wasOnReserve)
        {
            Notice = null;
        }
    }

    private void ConfirmSelected()
    {
        var member = Selected;
        Notice = null;
        if (_cursor.IsOnReserve)
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
            _actionChoice = new ListCursor(PartyActionChoiceView.EntryCount, _ => true);
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
        _cursor.KeepOnACard();
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
        if (choice.Selected == PartyActionChoiceView.ChangeOrderEntry)
        {
            MovingMember = Selected;
            _movingFromRow = _cursor.Row;
            return;
        }

        if (!_party.TryMoveToReserve(Selected))
        {
            Notice = _messages.Format(MessageIds.PartyNeedsSomeoneStanding, new MessageArgument("name", Selected.Definition.Name));
            return;
        }

        ShowParty();
        _cursor.KeepOnACard();
    }

    private void UpdateSwap(PartyMember moving, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            MovingMember = null;
            _cursor.ReturnTo(_movingFromRow);
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveToNextPlaceToSwap(-1);
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveToNextPlaceToSwap(1);
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            _party.Reorder(PartyOrderEdit.Swap(_party.ActiveMembers, moving, Selected));
            MovingMember = null;
            ShowParty();
        }
    }

    // Formats every card and the count once per change, keeping the notice, so drawing formats nothing.
    private void ShowParty() => _columns = _columnsView.ColumnsOf(_party, Notice);
}
