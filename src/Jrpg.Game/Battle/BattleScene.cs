using Jrpg.Core.Battle;
using Jrpg.Core.Items;
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
/// The battle screen over the map (ADR 0013): the encounter's background, the battlers, the HUD and the commands.
/// When the battle is over it finishes it in the session once and asks the flow, once, to go back to the map (or to the
/// title after a defeat).
/// </summary>
internal sealed class BattleScene : IScene
{
    private readonly GameSession _session;
    private readonly BattleState _battle;
    private readonly TurnOrderStrip _turnOrderStrip = new();
    private readonly TurnOrderView _turnOrder;
    private readonly UiTextures _ui;
    private readonly ScreenText _text;
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

    /// <param name="session">The playthrough whose active battle this screen shows.</param>
    /// <param name="items">Every item by id, from the catalog, for the party's items.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared code-drawn textures: the vignette, the top shade and shrunk battlers.</param>
    /// <exception cref="InvalidOperationException">The session has no active battle.</exception>
    public BattleScene(GameSession session, IReadOnlyDictionary<string, ItemDefinition> items, ScreenText text, UiTextures ui)
    {
        _session = session;
        _text = text;
        _ui = ui;
        var battle = session.ActiveBattle ?? throw new InvalidOperationException("A battle screen needs an active battle.");
        _battle = battle;
        _turnOrder = new TurnOrderView(_turnOrderStrip, battle.Combatants, text, ui);
        _turnOrderStrip.Refresh(battle);
        _sequence = new BattleTurnSequence(battle, session.EnemyTactics, text.Messages);
        _bannerView = new BattleBannerView(_banner, text, ui);
        _shown = (_sequence.Turn, _sequence.Stage);
        _battlers = new BattlersView(battle.Combatants, _sequence, ui);
        _partyStatus = new PartyStatusView([.. battle.Combatants.Where(combatant => combatant.Side == BattleSide.Party)], text, ui);
        _enemyTags = new EnemyTagView(battle.Combatants, _sequence, text, ui);
        _backgroundFile = session.Exploration.PendingEncounter?.BackgroundFile
            ?? throw new InvalidOperationException("A battle screen needs the encounter it fights.");
        _commands = new CommandMenu(session.Inventory, items);
        _commandPanel = new CommandPanelView(_commands, _battlers, text, ui);
        _numbers = new FloatingNumbersView(_sequence, _battlers, text);
        _impactEffects = new ImpactEffectsView(_sequence, _battlers, text);
    }

    /// <returns>
    /// <see cref="BattleFinished"/> when the victory screen is left, or on the update an escape ends the battle;
    /// <see cref="TitleRequested"/> on the update a defeat ends it, until the defeat screen (plan 2026-10-20-battle, B8b).
    /// Null otherwise.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_victory is { } victory)
        {
            return victory.Update(delta, input);
        }

        if (_isFinished)
        {
            return null;
        }

        if (_sequence.IsAwaitingCommand && _commands.Update(input) is { } command)
        {
            _sequence.Submit(command);
        }

        _turnOrderStrip.Highlight(_commands.Targets?.Selected ?? []);
        _banner.Update(delta);
        _commandPanel.Advance(delta);
        _sequence.Update(delta);
        if (!ReferenceEquals(_sequence.Playback, _shownPlayback))
        {
            ShowPlayback(_sequence.Playback);
        }

        if (_shown != (_sequence.Turn, _sequence.Stage))
        {
            ShowStage(_sequence.Turn, _sequence.Stage);
        }

        return _sequence.IsOver ? Finish() : null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawBackdrop(batch, textures);
        _battlers.Draw(batch, textures);
        _enemyTags.Draw(batch, textures);
        _impactEffects.Draw(batch, textures);
        _numbers.Draw(batch, textures);
        _turnOrder.Draw(batch, textures);
        _bannerView.Draw(batch, textures);
        var isAwaitingCommand = !_isFinished && _sequence.IsAwaitingCommand;
        _partyStatus.Draw(batch, textures, _sequence.IsOver ? null : _sequence.Actor);
        if (isAwaitingCommand)
        {
            _commandPanel.Draw(batch, textures, _sequence.Actor);
        }

        _victory?.Draw(batch, textures);
    }

    // Text is formatted at stage changes, never per frame: the vitals once an action lands, the order when a turn starts.
    private void ShowStage(int turn, TurnStage stage)
    {
        _shown = (turn, stage);
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

    // A victory stays on this screen for its rewards; an escape or a defeat leaves at once.
    private SceneRequest? Finish()
    {
        _isFinished = true;
        var outcome = _battle.Phase;
        if (_session.FinishBattle() is { } rewards)
        {
            _victory = new VictoryScreen(rewards, _text, _ui);
            return null;
        }

        return outcome == BattlePhase.Defeat ? new TitleRequested() : new BattleFinished();
    }

    // Legacy BattleUI's backdrop: the picture, the vignette over the whole screen, and the shade across the top.
    private void DrawBackdrop(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, _backgroundFile), Color.White);
        batch.Draw(_ui.BattleVignette(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        batch.Draw(_ui.TopShade(textures), new Rectangle(0, 0, VirtualScreen.Width, UiTextures.TopShadeHeight), Color.White);
    }
}
