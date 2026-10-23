using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.NewGame;

/// <summary>
/// The screen after New Game: the characters a new game may start led by, in a column on the left (decision 157).
/// Confirm asks "Begin as X?" first (decision 170); Yes starts the game led by them. Cancel on the list goes back to
/// the title (decision 178); in the question it only answers No.
/// </summary>
internal sealed class LeaderChoiceScene : IScene
{
    // Placeholder layout (decision 82): the heading at the top left, then one row per character.
    private const int ListLeft = 120;
    private const int HeadingTop = 80;
    private const int ListTop = 200;
    private const int RowPitch = 72;
    private const int HeadingPixels = 40;
    private const int NamePixels = 34;

    private readonly LeaderChoiceMenu _menu;
    private readonly UiMessages _messages;
    private readonly ConfirmPromptView _promptView;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _nameFont;
    private readonly string _heading;
    private readonly Dictionary<string, string> _nameOf;
    private ConfirmPrompt? _asking;
    private bool _hasAnswered;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures the question's panel uses.</param>
    /// <param name="catalog">The new game's leader choices and the members they name.</param>
    public LeaderChoiceScene(ScreenText text, UiTextures ui, ContentCatalog catalog)
    {
        _menu = new LeaderChoiceMenu(catalog.NewGame.LeaderChoices);
        _messages = text.Messages;
        _promptView = new ConfirmPromptView(text, ui);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _nameFont = text.Fonts.FontAt(NamePixels);
        _heading = text.Messages.Text(MessageIds.LeaderChoiceTitle);
        _nameOf = _menu.Choices.ToDictionary(choice => choice.MemberId, choice => catalog.PartyMembers[choice.MemberId].Name);
    }

    /// <summary>The choice the cursor is on, so tests can pick a character by who they are.</summary>
    internal LeaderChoice Selected => _menu.Selected;

    /// <summary>Whether "Begin as X?" is open.</summary>
    internal bool IsAsking => _asking is not null;

    /// <returns>The chosen leader or the way back to the title, each once; then nothing.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_hasAnswered)
        {
            return null;
        }

        if (_asking is not null)
        {
            return Answer(_asking, input);
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            _hasAnswered = true;
            return new TitleRequested();
        }

        if (_menu.Update(input) is { } chosen)
        {
            var name = new MessageArgument("name", _nameOf[chosen.MemberId]);
            _asking = new ConfirmPrompt(_messages.Format(MessageIds.LeaderChoiceConfirm, name));
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Title), Color.White);
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_headingFont, _heading, new Vector2(ListLeft, HeadingTop), UiTheme.TextPrimary);
        for (var index = 0; index < _menu.Choices.Count; index++)
        {
            var choice = _menu.Choices[index];
            var color = choice == _menu.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            batch.DrawString(_nameFont, _nameOf[choice.MemberId], new Vector2(ListLeft, ListTop + (index * RowPitch)), color);
        }

        if (_asking is not null)
        {
            _promptView.Draw(batch, textures, _asking, opacity: 1);
        }
    }

    // No, or Cancel, closes the question and leaves the cursor on the same character.
    private LeaderChosen? Answer(ConfirmPrompt asking, InputFrame input)
    {
        if (asking.Update(input) is not { } answer)
        {
            return null;
        }

        _asking = null;
        if (answer == ConfirmAnswer.No)
        {
            return null;
        }

        _hasAnswered = true;
        return new LeaderChosen(_menu.Selected);
    }
}
