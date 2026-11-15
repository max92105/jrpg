using FontStashSharp;
using Jrpg.Core.Battle;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Which screen a finished battle shows on the battle screen, with the music and the sounds that go with it (decisions
/// 418, 663): a victory's rewards, an escape's notice or a defeat's choices. The scene hands it the finished battle,
/// once, as the session finishes it.
/// </summary>
internal sealed class BattleOutcomeScreens
{
    // "Escaped" at 64 px on a band padded 18 px (placeholder look).
    private const int EscapedTextPixels = 64;
    private const int EscapedPaddingY = 18;

    private readonly SaveSlots _saves;
    private readonly ScreenText _text;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _escapedFont;
    private readonly AskedSounds _sounds = new();
    private readonly BattleMusic _music = new();
    private VictoryScreen? _victory;
    private VictoryScreenView? _victoryView;
    private DefeatScreen? _defeat;
    private DefeatScreenView? _defeatView;
    private EscapedNotice? _escaped;
    private MeasuredText _escapedText;

    /// <param name="saves">The save files the defeat screen offers to load.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared code-drawn textures.</param>
    public BattleOutcomeScreens(SaveSlots saves, ScreenText text, UiTextures ui)
    {
        _saves = saves;
        _text = text;
        _ui = ui;
        _escapedFont = text.Fonts.FontAt(EscapedTextPixels);
    }

    /// <summary>What its last <see cref="Show"/> or <see cref="Update"/> asked.</summary>
    public IReadOnlyList<string> Sounds => _sounds.Files;

    /// <summary>What the outcome calls for from the music (decision 418).</summary>
    public BattleMusicCue MusicCue => _music.Cue;

    /// <summary>Whether a victory or defeat screen shows, covering the HUD.</summary>
    public bool CoversTheHud => _victoryView is not null || _defeatView is not null;

    /// <summary>
    /// Shows the finished battle's own screen: a victory's rewards when there are spoils, an escape's notice, or a
    /// defeat's choices; asks its jingle and its music.
    /// </summary>
    /// <param name="spoils">What the session's <c>FinishBattle</c> returned: set only for a victory.</param>
    /// <param name="outcome">The battle's phase as it ended.</param>
    public void Show(BattleSpoils? spoils, BattlePhase outcome)
    {
        _sounds.Clear();
        if (spoils is not null)
        {
            _music.ShowVictory();
            _sounds.Ask(UiSoundFiles.Battle.Victory);
            _victory = new VictoryScreen(spoils);
            _victoryView = new VictoryScreenView(_victory, _text, _ui);
            return;
        }

        if (outcome == BattlePhase.Escaped)
        {
            _music.ShowEscape();
            _sounds.Ask(UiSoundFiles.Battle.Escape);
            _escaped = new EscapedNotice();
            _escapedText = MeasuredText.Of(_escapedFont, _text.Messages.Text(MessageIds.BattleEscaped));
            return;
        }

        _music.ShowDefeat();
        _sounds.Ask(UiSoundFiles.Battle.Defeat);
        _defeat = new DefeatScreen(_saves, _text, _ui);
        _defeatView = new DefeatScreenView(_defeat, _text, _ui);
    }

    /// <summary>The music's time, every update, the battle's included.</summary>
    public void Advance(TimeSpan delta) => _music.Advance(delta);

    /// <returns>
    /// <see cref="BattleFinished"/> when the victory screen or the escaped notice is left; <see cref="LoadRequested"/>
    /// or <see cref="TitleRequested"/> from the defeat screen's choice. Null otherwise.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sounds.Clear();
        if (_victory is { } victory)
        {
            var request = victory.Update(delta, input);
            _sounds.AskAll(victory.Sounds);
            return request;
        }

        return _escaped is { } escaped ? escaped.Update(delta, input) : UpdateDefeat(delta, input);
    }

    /// <summary>The victory or defeat screen, drawn over the battlers in place of the HUD.</summary>
    public void DrawOverTheBattlers(SpriteBatch batch, TextureLibrary textures)
    {
        _victoryView?.Draw(batch, textures);
        _defeatView?.Draw(batch, textures);
    }

    /// <summary>The escaped notice: the battle under a scrim, "Escaped" on a banner band across the middle.</summary>
    public void DrawOverTheHud(SpriteBatch batch, TextureLibrary textures)
    {
        if (_escaped is null)
        {
            return;
        }

        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        var height = (int)_escapedFont.LineHeight + (2 * EscapedPaddingY);
        var band = new Rectangle(0, (VirtualScreen.Height - height) / 2, VirtualScreen.Width, height);
        batch.Draw(_ui.Gradient(textures, UiTheme.Banner), band, Color.White);
        var line = _ui.Gradient(textures, UiTheme.HighlightLine);
        batch.Draw(line, new Rectangle(band.X, band.Y, band.Width, 1), Color.White);
        batch.Draw(line, new Rectangle(band.X, band.Bottom - 1, band.Width, 1), Color.White);
        var position = new Vector2(band.Center.X - (_escapedText.Width / 2), band.Y + EscapedPaddingY);
        batch.DrawString(_escapedFont, _escapedText.Text, position, UiTheme.TextPrimary);
    }

    private SceneRequest? UpdateDefeat(TimeSpan delta, InputFrame input)
    {
        if (_defeat is not { } defeat)
        {
            return null;
        }

        var request = defeat.Update(delta, input);
        _sounds.AskAll(defeat.Sounds);
        return request;
    }
}
