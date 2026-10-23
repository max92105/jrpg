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
/// Confirm starts the game led by the one the cursor is on; Cancel goes back to the title (decision 178).
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
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _nameFont;
    private readonly string _heading;
    private readonly string[] _names;
    private bool _hasAnswered;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="catalog">The new game's leader choices and the members they name.</param>
    public LeaderChoiceScene(ScreenText text, ContentCatalog catalog)
    {
        _menu = new LeaderChoiceMenu(catalog.NewGame.LeaderChoices);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _nameFont = text.Fonts.FontAt(NamePixels);
        _heading = text.Messages.Text(MessageIds.LeaderChoiceTitle);
        _names = [.. _menu.Choices.Select(choice => catalog.PartyMembers[choice.MemberId].Name)];
    }

    /// <summary>The choice the cursor is on, so tests can pick a character by who they are.</summary>
    internal LeaderChoice Selected => _menu.Selected;

    /// <returns>The chosen leader or the way back to the title, each once; then nothing.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_hasAnswered)
        {
            return null;
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            _hasAnswered = true;
            return new TitleRequested();
        }

        if (_menu.Update(input) is not { } chosen)
        {
            return null;
        }

        _hasAnswered = true;
        return new LeaderChosen(chosen);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Title), Color.White);
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_headingFont, _heading, new Vector2(ListLeft, HeadingTop), UiTheme.TextPrimary);
        for (var index = 0; index < _names.Length; index++)
        {
            var color = _menu.Choices[index] == _menu.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            batch.DrawString(_nameFont, _names[index], new Vector2(ListLeft, ListTop + (index * RowPitch)), color);
        }
    }
}
