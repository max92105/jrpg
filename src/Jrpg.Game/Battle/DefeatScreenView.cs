using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the defeat screen (in the victory screen's frame): the battle shaded dark on the left, a red "Defeat" over a
/// hairline, "The party has fallen.", the choices on pointed bars with the selected one lit and pointed at, and the
/// footer. The save list opens over it all.
/// </summary>
internal sealed class DefeatScreenView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56), matching the victory screen's title and bars.
    private const int SubtitleLeft = 110;
    private const int SubtitleTop = 246;
    private const int ChoicesLeft = 88;
    private const int ChoicesTop = 320;
    private const int ChoiceGap = 14;
    private const int ChoiceLabelLeft = 96;
    private const int FooterTextLeft = 70;
    private const int TitleTextPixels = 96;
    private const int SubtitleTextPixels = 24;
    private const int ChoiceTextPixels = 28;
    private const int FooterTextPixels = 24;
    private const int FilesWidth = 1000;
    private static readonly Rectangle _filesArea = new((VirtualScreen.Width - FilesWidth) / 2, 0, FilesWidth, VirtualScreen.Height);

    private readonly DefeatScreen _screen;
    private readonly UiTextures _ui;
    private readonly OutlinedFont _titleFont;
    private readonly OutlinedFont _subtitleFont;
    private readonly OutlinedFont _choiceFont;
    private readonly SpriteFontBase _footerFont;
    private readonly string _title;
    private readonly string _subtitle;
    private readonly string _footer;
    private readonly string[] _labels;

    /// <param name="screen">The state it draws.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the shade and the pointed bars are baked.</param>
    public DefeatScreenView(DefeatScreen screen, ScreenText text, UiTextures ui)
    {
        _screen = screen;
        _ui = ui;
        _titleFont = new OutlinedFont(text.Fonts.TitleFontAt(TitleTextPixels));
        _subtitleFont = new OutlinedFont(text.Fonts.FontAt(SubtitleTextPixels));
        _choiceFont = new OutlinedFont(text.Fonts.FontAt(ChoiceTextPixels));
        _footerFont = text.Fonts.FontAt(FooterTextPixels);
        _title = text.Messages.Text(MessageIds.BattleDefeat);
        _subtitle = text.Messages.Text(MessageIds.BattlePartyFallen);
        _footer = text.Messages.Text(MessageIds.BattleDefeatHint);
        _labels = [.. screen.Menu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and gradients come from.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        OutcomeFrame.DrawBackdrop(batch, textures, _ui);
        _titleFont.Draw(batch, _title, new Vector2(OutcomeFrame.TitleLeft, OutcomeFrame.TitleTop), UiTheme.Danger * _screen.Entrance.TitleOpacity);
        _subtitleFont.Draw(batch, _subtitle, new Vector2(SubtitleLeft, SubtitleTop), UiTheme.TextSecondary);
        DrawChoices(batch, textures);
        DrawFooter(batch, textures.Pixel);
        if (_screen.IsChoosingFile)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
            batch.Draw(textures.Pixel, _filesArea, UiTheme.FilesOverlay);
            _screen.Files.Draw(batch, textures, _filesArea, opacity: 1);
        }
    }

    private static string LabelIdOf(DefeatEntry entry) => entry switch
    {
        DefeatEntry.LoadLastSave => MessageIds.BattleLoadLastSave,
        DefeatEntry.Load => MessageIds.TitleLoad,
        DefeatEntry.MainMenu => MessageIds.BattleMainMenu,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    private void DrawChoices(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _screen.Entrance.ChoicesOpacity;
        var menu = _screen.Menu;
        for (var index = 0; index < _labels.Length; index++)
        {
            var isSelected = menu.Entries[index] == menu.Selected;
            var bar = new Rectangle(ChoicesLeft, ChoicesTop + (index * (PointedBarShape.RewardBarHeight + ChoiceGap)), PointedBarShape.RewardBarWidth, PointedBarShape.RewardBarHeight);
            batch.Draw(_ui.RewardBar(textures), bar, Color.White * opacity);
            if (isSelected)
            {
                _ui.DrawSelectedRow(batch, textures, bar, opacity);
                MenuPointer.Draw(batch, textures.Pixel, new Vector2(bar.X + ChoiceLabelLeft, bar.Center.Y), opacity);
            }

            var labelTop = bar.Center.Y - (_choiceFont.Font.LineHeight / 2f);
            _choiceFont.Draw(batch, _labels[index], new Vector2(bar.X + ChoiceLabelLeft, labelTop), UiTheme.TextPrimary * opacity);
        }
    }

    private void DrawFooter(SpriteBatch batch, Texture2D pixel)
    {
        OutcomeFrame.DrawFooter(batch, pixel);
        var textTop = FooterBar.TextTop(OutcomeFrame.FooterHeight, _footerFont.LineHeight);
        batch.DrawString(_footerFont, _footer, new Vector2(FooterTextLeft, textTop), UiTheme.TextPrimary);
    }
}
