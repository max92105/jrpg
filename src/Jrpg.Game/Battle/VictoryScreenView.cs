using System.Globalization;
using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the victory screen (leaning toward the <c>Victory_Menu.jpg</c> reference): the battle shaded dark on the left,
/// "Victory!" over a hairline, one pointed bar per reward, then the members' cards in two columns, each with a pointed
/// name tab, the level, "EXP n/m" and a gold bar.
/// </summary>
internal sealed class VictoryScreenView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56).
    private const int TitleLeft = 104;
    private const int TitleTop = 112;
    private const int TitleLineLeft = 96;
    private const int TitleLineTop = 226;
    private const int RewardLeft = 88;
    private const int RewardTop = 268;
    private const int RewardGap = 12;
    private const int RewardLabelLeft = 34;
    private const int RewardValueLeft = RewardLabelLeft + 200;
    private const float RewardLabelOpacity = 0.75f;
    private const int CardsLeft = 104;
    private const int CardsTop = 420;
    private const int CardColumnGap = 880 - (2 * PointedBarShape.CardHeaderWidth);
    private const int CardPitch = 150;
    private const int CardNameLeft = 22;
    private const int CardLevelRight = 24;
    private const int CardInset = 16;
    private const int CardExperienceTop = PointedBarShape.CardHeaderHeight + 6;
    private const int CardBarTop = CardExperienceTop + 40;
    private const int CardBarHeight = 14;
    private const int LevelLabelGap = 4;
    private const int LevelUpRight = 8;
    private const int LevelUpTop = -34;
    private const int FooterHeight = 100;
    private const int FooterLineAbove = 170;
    private const int NextHintRight = 70;
    private const int TitlePixels = 96;
    private const int RewardLabelPixels = 22;
    private const int RewardValuePixels = 40;
    private const int NamePixels = 26;
    private const int LevelPixels = 24;
    private const int SmallPixels = 20;
    private const int LabelPixels = 16;
    private const int ExperiencePixels = 30;
    private const int NextHintPixels = 28;

    private readonly VictoryScreen _screen;
    private readonly UiTextures _ui;
    private readonly UiMessages _messages;
    private readonly OutlinedFont _titleFont;
    private readonly OutlinedFont _rewardLabelFont;
    private readonly OutlinedFont _rewardValueFont;
    private readonly OutlinedFont _nameFont;
    private readonly OutlinedFont _levelFont;
    private readonly OutlinedFont _smallFont;
    private readonly OutlinedFont _labelFont;
    private readonly OutlinedFont _experienceFont;
    private readonly OutlinedFont _nextHintFont;
    private readonly string _title;
    private readonly string[] _rewardLabels;
    private readonly string[] _rewardValues;
    private readonly string[] _names;
    private readonly MeasuredText _levelLabel;
    private readonly string _experienceLabel;
    private readonly MeasuredText _levelUp;
    private readonly MeasuredText _nextHint;
    private readonly StatBar[] _bars;
    private readonly MeasuredText _maxLevel;
    private readonly MeasuredText _noNextLevel;
    private readonly (int Level, int Experience, int Needed, bool IsAtMaxLevel)[] _shown;
    private readonly MeasuredText[] _levelTexts;
    private readonly MeasuredText[] _experienceTexts;
    private readonly MeasuredText[] _nextLevelTexts;

    /// <param name="screen">The state it draws.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the shade, the pointed bars and the gold bars are baked.</param>
    public VictoryScreenView(VictoryScreen screen, ScreenText text, UiTextures ui)
    {
        _screen = screen;
        _ui = ui;
        _messages = text.Messages;
        _titleFont = new OutlinedFont(text.Fonts.FontAt(TitlePixels));
        _rewardLabelFont = new OutlinedFont(text.Fonts.FontAt(RewardLabelPixels));
        _rewardValueFont = new OutlinedFont(text.Fonts.FontAt(RewardValuePixels));
        _nameFont = new OutlinedFont(text.Fonts.FontAt(NamePixels));
        _levelFont = new OutlinedFont(text.Fonts.FontAt(LevelPixels));
        _smallFont = new OutlinedFont(text.Fonts.FontAt(SmallPixels));
        _labelFont = new OutlinedFont(text.Fonts.FontAt(LabelPixels));
        _experienceFont = new OutlinedFont(text.Fonts.FontAt(ExperiencePixels));
        _nextHintFont = new OutlinedFont(text.Fonts.FontAt(NextHintPixels));
        _title = text.Messages.Text(MessageIds.BattleVictory);
        var rewardRows = RewardRows(screen.Rewards);
        _rewardLabels = [.. rewardRows.Select(row => text.Messages.Text(row.LabelId))];
        _rewardValues = [.. rewardRows.Select(row => row.Amount.ToString(CultureInfo.InvariantCulture))];
        _names = [.. screen.Rewards.Gains.Select(gain => gain.Member.Definition.Name)];
        _levelLabel = MeasuredText.Of(_smallFont.Font, text.Messages.Text(MessageIds.BattleLevelLabel));
        _experienceLabel = text.Messages.Text(MessageIds.StatusExp);
        _levelUp = MeasuredText.Of(_nameFont.Font, text.Messages.Text(MessageIds.BattleLevelUp));
        _nextHint = MeasuredText.Of(_nextHintFont.Font, text.Messages.Text(MessageIds.BattleVictoryNext));
        _maxLevel = MeasuredText.Of(_experienceFont.Font, text.Messages.Text(MessageIds.StatusMaxLevel));
        _noNextLevel = MeasuredText.Of(_smallFont.Font, string.Empty);
        _bars = [.. _names.Select(_ => new StatBar(ui, UiTheme.BattleXpBar, UiTheme.BattleBarTrack))];
        _shown = new (int, int, int, bool)[_names.Length];
        Array.Fill(_shown, (-1, -1, -1, false));
        _levelTexts = new MeasuredText[_names.Length];
        _experienceTexts = new MeasuredText[_names.Length];
        _nextLevelTexts = new MeasuredText[_names.Length];
    }

    /// <summary>One row per thing the battle gave; only experience so far (decision 103).</summary>
    public static IReadOnlyList<VictoryRewardRow> RewardRows(VictoryRewards rewards) =>
        [new VictoryRewardRow(MessageIds.StatusExp, rewards.Experience)];

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        // Decision 52: the battle is dimmed, not blurred.
        batch.Draw(_ui.MenuScrim(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        var entrance = _screen.Entrance;
        _titleFont.Draw(batch, _title, new Vector2(TitleLeft + entrance.TitleOffset, TitleTop), UiTheme.TextPrimary * entrance.TitleOpacity);
        batch.Draw(textures.Pixel, new Rectangle(TitleLineLeft, TitleLineTop, VirtualScreen.Width - TitleLineLeft, 1), UiTheme.BarLine);
        DrawRewards(batch, textures);
        for (var index = 0; index < _names.Length; index++)
        {
            ShowCard(index);
            var column = index % 2;
            var topLeft = new Point(
                CardsLeft + (column * (PointedBarShape.CardHeaderWidth + CardColumnGap)) + (int)entrance.CardOffset(index),
                CardsTop + (index / 2 * CardPitch));
            DrawCard(batch, textures, index, topLeft);
        }

        var footerTop = VirtualScreen.Height - FooterHeight;
        batch.Draw(textures.Pixel, new Rectangle(0, VirtualScreen.Height - FooterLineAbove, VirtualScreen.Width, 1), UiTheme.BarLine);
        batch.Draw(textures.Pixel, new Rectangle(0, footerTop, VirtualScreen.Width, FooterHeight), UiTheme.Bar);
        batch.Draw(textures.Pixel, new Rectangle(0, footerTop, VirtualScreen.Width, 1), UiTheme.BarLine);
        if (_screen.IsComplete)
        {
            var hintTop = footerTop + ((FooterHeight - _nextHintFont.Font.LineHeight) / 2f);
            _nextHintFont.Draw(batch, _nextHint.Text, new Vector2(VirtualScreen.Width - NextHintRight - _nextHint.Width, hintTop), UiTheme.TextPrimary);
        }
    }

    private void DrawRewards(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _screen.Entrance.RewardOpacity;
        for (var index = 0; index < _rewardLabels.Length; index++)
        {
            var bar = new Rectangle(RewardLeft, RewardTop + (index * (PointedBarShape.RewardBarHeight + RewardGap)), PointedBarShape.RewardBarWidth, PointedBarShape.RewardBarHeight);
            batch.Draw(_ui.RewardBar(textures), bar, Color.White * opacity);
            var labelTop = bar.Center.Y - (_rewardLabelFont.Font.LineHeight / 2f);
            _rewardLabelFont.Draw(batch, _rewardLabels[index], new Vector2(bar.X + RewardLabelLeft, labelTop), UiTheme.TextPrimary * (RewardLabelOpacity * opacity));
            var valueTop = bar.Center.Y - (_rewardValueFont.Font.LineHeight / 2f);
            _rewardValueFont.Draw(batch, _rewardValues[index], new Vector2(bar.X + RewardValueLeft, valueTop), UiTheme.TextPrimary * opacity);
        }
    }

    private void DrawCard(SpriteBatch batch, TextureLibrary textures, int index, Point topLeft)
    {
        var opacity = _screen.Entrance.CardOpacity(index);
        var tab = new Rectangle(topLeft.X, topLeft.Y, PointedBarShape.CardHeaderWidth, PointedBarShape.CardHeaderHeight);
        batch.Draw(_ui.CardHeader(textures), tab, Color.White * opacity);
        _nameFont.Draw(batch, _names[index], new Vector2(tab.X + CardNameLeft, tab.Center.Y - (_nameFont.Font.LineHeight / 2f)), UiTheme.TextPrimary * opacity);
        var level = _levelTexts[index];
        var levelLeft = tab.Right - CardLevelRight - level.Width;
        _levelFont.Draw(batch, level.Text, new Vector2(levelLeft, tab.Center.Y - (_levelFont.Font.LineHeight / 2f)), UiTheme.TextPrimary * opacity);
        var labelTop = tab.Center.Y - (_levelFont.Font.LineHeight / 2f) + _levelFont.Font.LineHeight - _smallFont.Font.LineHeight;
        _smallFont.Draw(batch, _levelLabel.Text, new Vector2(levelLeft - _levelLabel.Width - LevelLabelGap, labelTop), UiTheme.TextPrimary * opacity);
        if (_screen.Bars[index] is { LevelUps: > 0 } bar)
        {
            var scale = bar.SinceLastLevelUp is { } since ? LevelUpPop.ScaleAt(since) : 1;
            var center = new Vector2(tab.Right - LevelUpRight - (_levelUp.Width / 2), tab.Y + LevelUpTop + (_nameFont.Font.LineHeight / 2f));
            _nameFont.DrawCentered(batch, _levelUp, center, (UiTheme.Positive, scale));
        }

        DrawExperience(batch, textures, index, topLeft);
    }

    // "EXP" on the left, the experience and "/needed" right-aligned on one baseline, then the gold bar.
    private void DrawExperience(SpriteBatch batch, TextureLibrary textures, int index, Point topLeft)
    {
        var opacity = _screen.Entrance.CardOpacity(index);
        var right = topLeft.X + PointedBarShape.CardHeaderWidth - CardInset;
        var baseline = topLeft.Y + CardExperienceTop + _experienceFont.Font.LineHeight;
        _labelFont.Draw(batch, _experienceLabel, new Vector2(topLeft.X + CardInset, baseline - _labelFont.Font.LineHeight), UiTheme.TextSecondary * opacity);
        var needed = _nextLevelTexts[index];
        _smallFont.Draw(batch, needed.Text, new Vector2(right - needed.Width, baseline - _smallFont.Font.LineHeight), UiTheme.TextSecondary * opacity);
        var experience = _experienceTexts[index];
        _experienceFont.Draw(batch, experience.Text, new Vector2(right - needed.Width - experience.Width, baseline - _experienceFont.Font.LineHeight), UiTheme.TextPrimary * opacity);
        var bar = new Rectangle(topLeft.X + CardInset, topLeft.Y + CardBarTop, PointedBarShape.CardHeaderWidth - (2 * CardInset), CardBarHeight);
        _bars[index].Draw(batch, textures, bar, opacity);
    }

    // A card's numbers are formatted only when its bar moves on, never on a frame where they stay the same.
    private void ShowCard(int index)
    {
        var bar = _screen.Bars[index];
        var isAtMaxLevel = _screen.IsAtMaxLevel(index);
        var shown = (bar.Level, bar.Experience, bar.ExperienceToNextLevel, isAtMaxLevel);
        if (_shown[index] == shown)
        {
            return;
        }

        if (_shown[index].Level != bar.Level)
        {
            _levelTexts[index] = MeasuredText.Of(_levelFont.Font, bar.Level.ToString(CultureInfo.InvariantCulture));
        }

        if (isAtMaxLevel)
        {
            ShowMaxLevel(index);
        }
        else
        {
            ShowExperience(index, bar);
        }

        _shown[index] = shown;
    }

    private void ShowExperience(int index, ExperienceBarAnimation bar)
    {
        if (_shown[index].Needed != bar.ExperienceToNextLevel)
        {
            var needed = _messages.Format(MessageIds.BattleVitalMax, new MessageArgument("max", bar.ExperienceToNextLevel.ToString(CultureInfo.InvariantCulture)));
            _nextLevelTexts[index] = MeasuredText.Of(_smallFont.Font, needed);
        }

        _experienceTexts[index] = MeasuredText.Of(_experienceFont.Font, bar.Experience.ToString(CultureInfo.InvariantCulture));
        _bars[index].Show(bar.Experience, bar.ExperienceToNextLevel);
    }

    // As the Status page shows the highest level: "Max" where the numbers go, and a full bar (decision 148).
    private void ShowMaxLevel(int index)
    {
        _experienceTexts[index] = _maxLevel;
        _nextLevelTexts[index] = _noNextLevel;
        _bars[index].Show(1, 1);
    }
}
