using Jrpg.Core.Battle;
using Jrpg.Core.Session;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
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
    private readonly GameSession _session;
    private readonly BattleState _battle;
    private readonly TurnOrderStrip _turnOrderStrip = new();
    private readonly TurnOrderView _turnOrder;
    private readonly UiTextures _ui;
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
    private ActionWindUp? _shownWindUp;
    private (int Turn, TurnStage Stage) _shown;
    private bool _isFinished;
    private readonly AskedSounds _sounds = new();
    private readonly PlaybackSoundQueue _soundQueue = new();
    private readonly PlaybackSoundQueue _windUpSoundQueue = new();
    private readonly BattleOutcomeScreens _outcome;

    /// <param name="session">The playthrough whose active battle this screen shows.</param>
    /// <param name="sources">The catalog's items, for the party's items, and the save files, for the defeat screen.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared code-drawn textures: the vignette, the top shade and shrunk battlers.</param>
    /// <exception cref="InvalidOperationException">The session has no active battle.</exception>
    public BattleScene(GameSession session, BattleSceneSources sources, ScreenText text, UiTextures ui)
    {
        _session = session;
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
        _outcome = new BattleOutcomeScreens(sources.Saves, text, ui);
        _commands = new CommandMenu(session.Inventory, sources.Items);
        _commandPanel = new CommandPanelView(_commands, _battlers, text, ui);
        _numbers = new FloatingNumbersView(_sequence, _battlers, _enemyTags, text);
        _impactEffects = new ImpactEffectsView(_sequence, _battlers, session.Catalog.EffectStrips, text);
    }

    public IReadOnlyList<string> Sounds => _sounds.Files;

    /// <summary>What the battle screen calls for from the music as its outcome shows (decision 418).</summary>
    public BattleMusicCue MusicCue => _outcome.MusicCue;

    /// <returns>
    /// <see cref="BattleFinished"/> when the victory screen or the escaped notice is left; <see cref="LoadRequested"/> or
    /// <see cref="TitleRequested"/> from the defeat screen's choice. Null otherwise.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sounds.Clear();
        _outcome.Advance(delta);
        if (_isFinished)
        {
            var request = _outcome.Update(delta, input);
            _sounds.AskAll(_outcome.Sounds);
            return request;
        }

        if (_sequence.IsAwaitingCommand)
        {
            var command = _commands.Update(input);
            _sounds.AskAll(_commands.Sounds);
            if (command is not null)
            {
                _sequence.Submit(command);
            }
        }

        ShowAimedTargets();
        AdvanceViews(delta);
        _sequence.Update(delta);
        _windUpSoundQueue.Follow(_sequence.WindUp, _sounds);
        _soundQueue.Follow(_sequence.Playback, _sounds);
        if (!ReferenceEquals(_sequence.WindUp, _shownWindUp))
        {
            ShowWindUp(_sequence.WindUp);
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
        if (_outcome.CoversTheHud)
        {
            _outcome.DrawOverTheBattlers(batch, textures);
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

        _outcome.DrawOverTheHud(batch, textures);
    }

    // While a target is being chosen, the strip lights it and the floor glows under the allies it would cover.
    private void ShowAimedTargets()
    {
        var selected = _commands.Targets?.Selected ?? [];
        _turnOrderStrip.Highlight(selected);
        _battlers.ShowCoverage(selected);
    }

    private void AdvanceViews(TimeSpan delta)
    {
        _banner.Update(delta);
        _commandPanel.Advance(delta);
        _partyStatus.Update(delta);
        _battlers.Update(delta);
        _enemyTags.Update(delta);
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

    // Checked on its own, not at a stage change: an action with no wind-up (Defend) passes it within one update.
    private void ShowWindUp(ActionWindUp? windUp)
    {
        _shownWindUp = windUp;
        if (windUp?.BannerText is { } bannerText)
        {
            _banner.Show(bannerText);
        }
    }

    // Each outcome stays on this screen for its own: a victory's rewards, an escape's notice, a defeat's choices.
    private void Finish()
    {
        _isFinished = true;
        var outcome = _battle.Phase;
        _outcome.Show(_session.FinishBattle(), outcome);
        _sounds.AskAll(_outcome.Sounds);
    }

    // The backdrop: the picture, the vignette over the whole screen, and the shade across the top.
    private void DrawBackdrop(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, _backgroundFile), Color.White);
        batch.Draw(_ui.BattleVignette(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        batch.Draw(_ui.TopShade(textures), new Rectangle(0, 0, VirtualScreen.Width, UiTextures.TopShadeHeight), Color.White);
    }
}
