using System.Globalization;
using FontStashSharp;
using Jrpg.Core.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// What a won battle gave, over the frozen battle (legacy <c>VictoryScreen</c>): the experience earned, then each standing
/// member's bar filling level by level. Confirm skips the filling; Confirm once it's done leaves the battle. A plain
/// layout until the victory look (plan 2026-10-20-battle, B7b).
/// </summary>
internal sealed class VictoryScreen
{
    private const int Left = 104;
    private const int TitleTop = 112;
    private const int ExperienceTop = 268;
    private const int RowsTop = 420;
    private const int RowHeight = 90;
    private const int LevelLeft = 360;
    private const int BarLeft = 520;
    private const int BarWidth = 600;
    private const int BarHeight = 8;
    private const int LevelUpLeft = 1160;
    private const int TitleTextPixels = 96;
    private const int BodyTextPixels = 28;

    private readonly UiMessages _messages;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _title;
    private readonly string _experienceEarned;
    private readonly string _levelUp;
    private readonly string[] _names;
    private readonly ExperienceBarAnimation[] _bars;
    private readonly StatBar[] _barViews;
    private readonly int[] _shownLevels;
    private readonly string[] _levelTexts;
    private bool _hasLeft;

    /// <param name="rewards">The won battle's experience and each standing member's gain.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the bars' gradient is baked.</param>
    public VictoryScreen(VictoryRewards rewards, ScreenText text, UiTextures ui)
    {
        _messages = text.Messages;
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _bodyFont = text.Fonts.FontAt(BodyTextPixels);
        _title = text.Messages.Text(MessageIds.BattleVictory);
        _experienceEarned = text.Messages.Format(
            MessageIds.BattleExperienceEarned, new MessageArgument("amount", rewards.Experience.ToString(CultureInfo.InvariantCulture)));
        _levelUp = text.Messages.Text(MessageIds.BattleLevelUp);
        _names = [.. rewards.Gains.Select(gain => gain.Member.Definition.Name)];
        _bars = [.. rewards.Gains.Select(gain => new ExperienceBarAnimation(gain.Steps, gain.Member.Level))];
        _barViews = [.. _bars.Select(_ => new StatBar(ui, UiTheme.XpBar))];
        _shownLevels = new int[_bars.Length];
        _levelTexts = new string[_bars.Length];
        ShowBars();
    }

    /// <summary>Whether every bar has finished filling.</summary>
    public bool IsComplete => Array.TrueForAll(_bars, bar => bar.IsComplete);

    /// <returns><see cref="BattleFinished"/> on the Confirm after the bars are full, once; null otherwise.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_hasLeft)
        {
            return null;
        }

        foreach (var bar in _bars)
        {
            bar.Update(delta);
        }

        SceneRequest? request = null;
        if (input.IsPressed(GameButton.Confirm) && IsComplete)
        {
            _hasLeft = true;
            request = new BattleFinished();
        }
        else if (input.IsPressed(GameButton.Confirm))
        {
            Array.ForEach(_bars, bar => bar.Skip());
        }

        ShowBars();
        return request;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_titleFont, _title, new Vector2(Left, TitleTop), UiTheme.TextPrimary);
        batch.DrawString(_bodyFont, _experienceEarned, new Vector2(Left, ExperienceTop), UiTheme.TextPrimary);
        for (var index = 0; index < _bars.Length; index++)
        {
            var top = RowsTop + (index * RowHeight);
            batch.DrawString(_bodyFont, _names[index], new Vector2(Left, top), UiTheme.TextPrimary);
            batch.DrawString(_bodyFont, _levelTexts[index], new Vector2(LevelLeft, top), UiTheme.TextSecondary);
            var barTop = top + (int)(_bodyFont.LineHeight / 2) - (BarHeight / 2);
            _barViews[index].Draw(batch, textures, new Rectangle(BarLeft, barTop, BarWidth, BarHeight), 1);
            if (_bars[index].LevelUps > 0)
            {
                batch.DrawString(_bodyFont, _levelUp, new Vector2(LevelUpLeft, top), UiTheme.Accent);
            }
        }
    }

    // The level text is formatted only when a bar reaches a new level, never per frame.
    private void ShowBars()
    {
        for (var index = 0; index < _bars.Length; index++)
        {
            var bar = _bars[index];
            _barViews[index].Show(bar.Experience, bar.ExperienceToNextLevel);
            if (_shownLevels[index] != bar.Level)
            {
                _shownLevels[index] = bar.Level;
                _levelTexts[index] = _messages.Format(MessageIds.PartyLevel, new MessageArgument("level", bar.Level.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}
