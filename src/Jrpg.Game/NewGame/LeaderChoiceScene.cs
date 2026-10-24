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
    // Placeholder layout (decision 82), framed like the game menu: header and footer bars over the title picture under
    // the menu scrim, the list on the left (decision 169), each row the name over the class in spaced caps.
    private const int HeaderHeight = 80;
    private const int FooterHeight = 64;
    private const int TextMargin = 48;
    private const int HeadingPixels = 28;
    private const int ListLeft = 64;
    private const int ListTop = 140;
    private const int ListWidth = 420;
    private const int RowPitch = 88;
    private const int RowTextInset = 40;
    private const int NamePixels = 34;
    private const int ClassPixels = 20;

    // The highlighted character's information fills the right: the class, its description, the summary, then the
    // stats. Plain until S3b.
    private const int InfoLeft = 760;
    private const int InfoWidth = 1040;
    private const int ClassTop = 140;
    private const int DescriptionTop = 210;
    private const int SummaryTop = 390;
    private const int StatsTop = 620;
    private const int StatPitch = 52;
    private const int StatValueLeft = InfoLeft + 260;
    private const int InfoHeadingPixels = 40;
    private const int BodyPixels = 30;

    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);

    private readonly LeaderChoiceMenu _menu;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly ConfirmPromptView _promptView;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _infoHeadingFont;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _classFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _hintFont;
    private readonly string _heading;
    private readonly string _hint;
    private readonly LeaderProfile[] _profiles;
    private readonly LeaderRow[] _rows;
    private readonly RichTextLayout[] _descriptions;
    private readonly RichTextLayout[] _summaries;
    private ConfirmPrompt? _asking;
    private bool _hasAnswered;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: the scrim, the selected row and the question's panel.</param>
    /// <param name="catalog">The new game's leader choices and the members they name.</param>
    public LeaderChoiceScene(ScreenText text, UiTextures ui, ContentCatalog catalog)
    {
        _menu = new LeaderChoiceMenu(catalog.NewGame.LeaderChoices);
        _messages = text.Messages;
        _ui = ui;
        _promptView = new ConfirmPromptView(text, ui);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _infoHeadingFont = text.Fonts.FontAt(InfoHeadingPixels);
        _nameFont = text.Fonts.FontAt(NamePixels);
        _classFont = text.Fonts.FontAt(ClassPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _hintFont = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _heading = text.Messages.Text(MessageIds.LeaderChoiceTitle);
        _hint = text.Messages.Text(MessageIds.LeaderChoiceHint);
        _profiles = [.. _menu.Choices.Select(choice => LeaderProfile.Of(choice, catalog, text.Messages))];
        _rows = [.. _profiles.Select(RowOf)];
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
        DrawFrame(batch, textures);
        for (var index = 0; index < _rows.Length; index++)
        {
            DrawRow(batch, textures, _rows[index], index == _menu.SelectedIndex);
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

    // The name and the class under it, centered together in the row.
    private LeaderRow RowOf(LeaderProfile profile, int index)
    {
        var area = new Rectangle(ListLeft, ListTop + (index * RowPitch), ListWidth, RowPitch);
        var nameTop = area.Y + ((RowPitch - _nameFont.LineHeight - _classFont.LineHeight) / 2f);
        var textLeft = area.X + RowTextInset;
        return new LeaderRow(
            area,
            profile.Name,
            new Vector2(textLeft, nameTop),
            profile.ClassName.ToUpperInvariant(),
            new Vector2(textLeft, nameTop + _nameFont.LineHeight));
    }

    private void DrawFrame(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Title), Color.White);
        batch.Draw(_ui.MenuScrim(textures), _screen, Color.White);
        HeaderBar.Draw(batch, textures.Pixel, HeaderHeight, opacity: 1);
        var headingTop = (HeaderHeight - _headingFont.LineHeight) / 2f;
        batch.DrawString(_headingFont, _heading, new Vector2(TextMargin, headingTop), UiTheme.TextPrimary);
        FooterBar.Draw(batch, textures.Pixel, FooterHeight, opacity: 1);
        batch.DrawString(_hintFont, _hint, new Vector2(TextMargin, FooterBar.TextTop(FooterHeight, _hintFont.LineHeight)), UiTheme.TextSecondary);
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, LeaderRow row, bool isSelected)
    {
        if (isSelected)
        {
            _ui.DrawSelectedRow(batch, textures, row.Area, opacity: 1);
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(row.NameAt.X, row.Area.Center.Y), opacity: 1);
        }

        var nameColor = isSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
        batch.DrawString(_nameFont, row.Name, row.NameAt, nameColor);
        _classFont.DrawText(batch, row.ClassCaps, row.ClassAt, UiTheme.TextSecondary, characterSpacing: UiTheme.SpeakerLetterSpacing);
    }

    private void DrawSelectedProfile(SpriteBatch batch)
    {
        var index = _menu.SelectedIndex;
        var profile = _profiles[index];
        batch.DrawString(_infoHeadingFont, profile.ClassName, new Vector2(InfoLeft, ClassTop), UiTheme.TextPrimary);
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

    // A row of the list, placed and upper-cased once when the screen opens, so drawing allocates nothing.
    private readonly record struct LeaderRow(Rectangle Area, string Name, Vector2 NameAt, string ClassCaps, Vector2 ClassAt);
}
