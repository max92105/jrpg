using FontStashSharp;
using FontStashSharp.RichText;
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
    // Placeholder layout (decision 82): the heading at the top left, then one row per character; the highlighted
    // character's information fills the right, the class, its description, the summary, then the stats.
    private const int ListLeft = 120;
    private const int HeadingTop = 80;
    private const int ListTop = 200;
    private const int RowPitch = 72;
    private const int HeadingPixels = 40;
    private const int NamePixels = 34;
    private const int InfoLeft = 760;
    private const int InfoWidth = 1040;
    private const int ClassTop = 200;
    private const int DescriptionTop = 270;
    private const int SummaryTop = 450;
    private const int StatsTop = 680;
    private const int StatPitch = 52;
    private const int StatValueLeft = InfoLeft + 260;
    private const int BodyPixels = 30;

    private readonly LeaderChoiceMenu _menu;
    private readonly UiMessages _messages;
    private readonly ConfirmPromptView _promptView;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _nameFont;
    private readonly string _heading;
    private readonly SpriteFontBase _bodyFont;
    private readonly LeaderProfile[] _profiles;
    private readonly RichTextLayout[] _descriptions;
    private readonly RichTextLayout[] _summaries;
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
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _heading = text.Messages.Text(MessageIds.LeaderChoiceTitle);
        _profiles = [.. _menu.Choices.Select(choice => LeaderProfile.Of(choice, catalog, text.Messages))];
        _descriptions = [.. _profiles.Select(profile => WrappedText(profile.ClassDescription, _bodyFont))];
        _summaries = [.. _profiles.Select(profile => WrappedText(profile.Summary, _bodyFont))];
    }

    /// <summary>The choice the cursor is on, so tests can pick a character by who they are.</summary>
    internal LeaderChoice Selected => _menu.Selected;

    /// <summary>What the information area shows of the highlighted character.</summary>
    internal LeaderProfile SelectedProfile => _profiles[_menu.SelectedIndex];

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

        if (_menu.Update(input) is not null)
        {
            var name = new MessageArgument("name", SelectedProfile.Name);
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
            batch.DrawString(_nameFont, _profiles[index].RowLabel, new Vector2(ListLeft, ListTop + (index * RowPitch)), color);
        }

        DrawSelectedProfile(batch);

        if (_asking is not null)
        {
            _promptView.Draw(batch, textures, _asking, opacity: 1);
        }
    }

    private static RichTextLayout WrappedText(string text, SpriteFontBase font) => new()
    {
        // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
        SupportsCommands = false,
        Font = font,
        Width = InfoWidth,
        Text = text,
    };

    private void DrawSelectedProfile(SpriteBatch batch)
    {
        var index = _menu.SelectedIndex;
        var profile = _profiles[index];
        batch.DrawString(_headingFont, profile.ClassName, new Vector2(InfoLeft, ClassTop), UiTheme.TextPrimary);
        _descriptions[index].Draw(batch, new Vector2(InfoLeft, DescriptionTop), UiTheme.TextSecondary);
        _summaries[index].Draw(batch, new Vector2(InfoLeft, SummaryTop), UiTheme.TextPrimary);
        for (var line = 0; line < profile.Stats.Count; line++)
        {
            var top = StatsTop + (line * StatPitch);
            batch.DrawString(_bodyFont, profile.Stats[line].Label, new Vector2(InfoLeft, top), UiTheme.TextSecondary);
            batch.DrawString(_bodyFont, profile.Stats[line].Value, new Vector2(StatValueLeft, top), UiTheme.TextPrimary);
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
