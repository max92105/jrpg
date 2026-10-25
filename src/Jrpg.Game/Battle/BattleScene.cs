using FontStashSharp;
using Jrpg.Core.Battle;
using Jrpg.Core.Session;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The battle screen over the map (ADR 0013): the encounter's background, the battlers, the HUD and the commands. When
/// the battle is over it finishes it in the session once and asks the flow, once, to go back to the map (or to the
/// title after a defeat).
/// </summary>
internal sealed class BattleScene : IScene
{
    // "Escaped" at 64 px on a band padded 18 px (placeholder look).
    private const int EscapedTextPixels = 64;
    private const int EscapedPaddingY = 18;

    private readonly GameSession _session;
    private readonly BattleState _battle;
    private readonly TurnOrderStrip _turnOrderStrip = new();
    private readonly TurnOrderView _turnOrder;
    private readonly UiTextures _ui;
    private readonly ScreenText _text;
    private readonly SaveSlots _saves;
    private readonly SpriteFontBase _escapedFont;
    private readonly BattlersView _battlers;
    private readonly BattleTurnSequence _sequence;
    private readonly string _backgroundFile;
    private readonly PartyStatusView _partyStatus;
    private readonly EnemyTagView _enemyTags;
    private readonly CommandMenu _commands;
    private readonly CommandPanelView _commandPanel;
    private readonly BattleBanner _banner = new();
    private readonly BattleBannerView _bannerView;
    private readonly FloatingNumbersView _numbers;
    private readonly ImpactEffectsView _impactEffects;
    private ActionPlayback? _shownPlayback;
    private (int Turn, TurnStage Stage) _shown;
    private bool _isFinished;
    private VictoryScreen? _victory;
    private VictoryScreenView? _victoryView;
    private DefeatScreen? _defeat;
    private DefeatScreenView? _defeatView;
    private EscapedNotice? _escaped;
    private MeasuredText _escapedText;

    /// <param name="session">The playthrough whose active battle this screen shows.</param>
    /// <param name="sources">The catalog's items, for the party's items, and the save files, for the defeat screen.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared code-drawn textures: the vignette, the top shade and shrunk battlers.</param>
    /// <exception cref="InvalidOperationException">The session has no active battle.</exception>
    public BattleScene(GameSession session, BattleSceneSources sources, ScreenText text, UiTextures ui)
    {
        _session = session;
        _text = text;
        _escapedFont = text.Fonts.FontAt(EscapedTextPixels);
        _ui = ui;
        var battle = session.ActiveBattle ?? throw new InvalidOperationException("A battle screen needs an active battle.");
        _battle = battle;
        _turnOrder = new TurnOrderView(_turnOrderStrip, battle.Combatants, text, ui);
        _turnOrderStrip.Refresh(battle);
        _sequence = new BattleTurnSequence(battle, session.EnemyTactics, text.Messages);
        _bannerView = new BattleBannerView(_banner, text, ui);
        _shown = (_sequence.Turn, _sequence.Stage);
        _battlers = new BattlersView(battle, _sequence, ui);
        _partyStatus = new PartyStatusView(battle.Formation, text, ui);
        _enemyTags = new EnemyTagView(battle.Combatants, _sequence, text, ui);
        _backgroundFile = session.Exploration.PendingEncounter?.BackgroundFile
            ?? throw new InvalidOperationException("A battle screen needs the encounter it fights.");
        _saves = sources.Saves;
        _commands = new CommandMenu(session.Inventory, sources.Items);
        _commandPanel = new CommandPanelView(_commands, _battlers, text, ui);
        _numbers = new FloatingNumbersView(_sequence, _battlers, text);
        _impactEffects = new ImpactEffectsView(_sequence, _battlers, text);
    }

    /// <returns>
    /// <see cref="BattleFinished"/> when the victory screen or the escaped notice is left; <see cref="LoadRequested"/> or
    /// <see cref="TitleRequested"/> from the defeat screen's choice. Null otherwise.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_isFinished)
        {
            return UpdateOutcomeScreen(delta, input);
        }

        if (_sequence.IsAwaitingCommand && _commands.Update(input) is { } command)
        {
            _sequence.Submit(command);
        }

        _turnOrderStrip.Highlight(_commands.Targets?.Selected ?? []);
        _banner.Update(delta);
        _commandPanel.Advance(delta);
        _partyStatus.Update(delta);
        _enemyTags.Update(delta);
        _sequence.Update(delta);
        if (!ReferenceEquals(_sequence.Playback, _shownPlayback))
        {
            ShowPlayback(_sequence.Playback);
        }

        if (_shown != (_sequence.Turn, _sequence.Stage))
        {
            ShowStage(_sequence.Turn, _sequence.Stage);
        }

        if (_sequence.IsOver)
        {
            Finish();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawBackdrop(batch, textures);
        _battlers.Draw(batch, textures);
        if (_victoryView is { } victoryView)
        {
            victoryView.Draw(batch, textures);
            return;
        }

        if (_defeatView is { } defeatView)
        {
            defeatView.Draw(batch, textures);
            return;
        }

        _enemyTags.Draw(batch, textures);
        _impactEffects.Draw(batch, textures);
        _numbers.Draw(batch, textures);
        _turnOrder.Draw(batch, textures);
        _bannerView.Draw(batch, textures);
        _partyStatus.Draw(batch, textures, _sequence.IsOver ? null : _sequence.Actor);
        if (!_isFinished && _sequence.IsAwaitingCommand)
        {
            _commandPanel.Draw(batch, textures, _sequence.Actor);
        }

        if (_escaped is not null)
        {
            DrawEscaped(batch, textures);
        }
    }

    // Text is formatted at stage changes, never per frame: the vitals once an action lands, the order when a turn starts.
    private void ShowStage(int turn, TurnStage stage)
    {
        _shown = (turn, stage);
        _battlers.RefreshSpots();
        _partyStatus.Refresh();
        _enemyTags.Refresh();
        if (stage is TurnStage.StepForward or TurnStage.AwaitingCommand)
        {
            _turnOrderStrip.Refresh(_battle);
        }

        if (stage == TurnStage.AwaitingCommand)
        {
            _commands.Start(_battle);
        }
    }

    // Checked on its own, not at a stage change: an action with no hold (Defend) passes the Holding stage within one update.
    private void ShowPlayback(ActionPlayback? playback)
    {
        _shownPlayback = playback;
        if (playback?.BannerText is { } bannerText)
        {
            _banner.Show(bannerText);
        }
    }

    // Exactly one outcome screen is set once the battle is finished.
    private SceneRequest? UpdateOutcomeScreen(TimeSpan delta, InputFrame input)
    {
        if (_victory is { } victory)
        {
            return victory.Update(delta, input);
        }

        return _escaped is { } escaped ? escaped.Update(delta, input) : _defeat?.Update(delta, input);
    }

    // Each outcome stays on this screen for its own: a victory's rewards, an escape's notice, a defeat's choices.
    private void Finish()
    {
        _isFinished = true;
        var outcome = _battle.Phase;
        if (_session.FinishBattle() is { } rewards)
        {
            _victory = new VictoryScreen(rewards);
            _victoryView = new VictoryScreenView(_victory, _text, _ui);
            return;
        }

        if (outcome == BattlePhase.Escaped)
        {
            _escaped = new EscapedNotice();
            _escapedText = MeasuredText.Of(_escapedFont, _text.Messages.Text(MessageIds.BattleEscaped));
            return;
        }

        _defeat = new DefeatScreen(_saves, _text, _ui);
        _defeatView = new DefeatScreenView(_defeat, _text, _ui);
    }

    // The battle under a scrim, and "Escaped" on a banner band across the middle of the screen.
    private void DrawEscaped(SpriteBatch batch, TextureLibrary textures)
    {
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        var height = (int)_escapedFont.LineHeight + (2 * EscapedPaddingY);
        var band = new Rectangle(0, (VirtualScreen.Height - height) / 2, VirtualScreen.Width, height);
        batch.Draw(_ui.Gradient(textures, UiTheme.Banner), band, Color.White);
        var line = _ui.Gradient(textures, UiTheme.HighlightLine);
        batch.Draw(line, new Rectangle(band.X, band.Y, band.Width, 1), Color.White);
        batch.Draw(line, new Rectangle(band.X, band.Bottom - 1, band.Width, 1), Color.White);
        batch.DrawString(_escapedFont, _escapedText.Text, new Vector2(band.Center.X - (_escapedText.Width / 2), band.Y + EscapedPaddingY), UiTheme.TextPrimary);
    }

    // The backdrop: the picture, the vignette over the whole screen, and the shade across the top.
    private void DrawBackdrop(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, _backgroundFile), Color.White);
        batch.Draw(_ui.BattleVignette(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        batch.Draw(_ui.TopShade(textures), new Rectangle(0, 0, VirtualScreen.Width, UiTextures.TopShadeHeight), Color.White);
    }
}
