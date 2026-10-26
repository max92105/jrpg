using System.Diagnostics.CodeAnalysis;
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
/// The menu's Status page: one party member at a time, with their profile and experience beside a diamond-framed
/// portrait, then their parameters and what they wear. Left and Right (or the tab buttons) go through the members,
/// active ones first.
/// </summary>
internal sealed class StatusPage : IMemberPage
{
    private readonly ContentCatalog _catalog;
    private readonly PartyState _party;
    private readonly ListCursor _cursor;
    private readonly StatusLabels _labels;
    private readonly StatusSheetView _view;
    private StatusSheet _sheet;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines and baked portraits.</param>
    /// <param name="session">Whose party is shown.</param>
    /// <param name="catalog">Class names and the experience table.</param>
    public StatusPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        Hint = text.Messages.Text(MessageIds.MenuHintStatus);
        _catalog = catalog;
        _party = session.Party;
        _labels = StatusLabels.From(text.Messages);
        _view = new StatusSheetView(text, ui, _labels);
        _cursor = new ListCursor(_party.Members.Count, _ => true);
        Open();
    }

    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public IReadOnlyList<PartyMember> Members { get; private set; }

    public PartyMember Shown => Members[_cursor.Selected];

    public string TitleMessageId => MessageIds.MenuStatus;

    public string Hint { get; }

    public bool LeavesOnCancel => true;

    /// <summary>The shown member's HP, Attack and Defense, as the page writes them.</summary>
    internal IReadOnlyList<string> ShownStats => _sheet.Stats;

    /// <summary>The parameters' and the shown member's resources' labels, as the page writes them.</summary>
    internal IReadOnlyList<string> ShownLabels => [.. _labels.Stats, _labels.Speed, .. _sheet.Resources.Select(resource => resource.Name)];

    /// <summary>The shown member's speed, as the page writes it.</summary>
    internal string ShownSpeed => _sheet.Speed;

    /// <summary>The shown member's resources, each with its max or its lasting amount.</summary>
    internal IReadOnlyList<StatusLine> ShownResources => _sheet.Resources;

    /// <summary>The shown member's skills, each with its description.</summary>
    internal IReadOnlyList<StatusLine> ShownSkills => _sheet.Skills;

    // Recruits don't change while the menu is open, so the cursor's member count still holds.
    [MemberNotNull(nameof(Members), nameof(_sheet))]
    public void Open()
    {
        Members = MemberOrder.ActiveThenReserve(_party);
        _cursor.Select(0);
        ShowMember();
    }

    public SceneRequest? Update(InputFrame input)
    {
        var before = _cursor.Selected;
        if (input.IsPressed(GameButton.Left) || input.IsPressed(GameButton.PreviousTab))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Right) || input.IsPressed(GameButton.NextTab))
        {
            _cursor.MoveDown();
        }

        if (_cursor.Selected != before)
        {
            ShowMember();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity) => _view.Draw(batch, textures, area, opacity);

    // The sheet's HP text comes from the meter, so the meter shows the member first.
    [MemberNotNull(nameof(_sheet))]
    private void ShowMember()
    {
        var member = Shown;
        _view.Vitals.Show(member.Vitals);
        _sheet = StatusSheet.Of(member, _catalog, _labels, _view.Vitals);
        _view.Show(_sheet);
    }
}
