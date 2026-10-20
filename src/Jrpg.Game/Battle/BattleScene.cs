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
    private readonly BattlersView _battlers;
    private readonly BattleTurnSequence _sequence;
    private readonly string _backgroundFile;
    private readonly PartyStatusView _partyStatus;
    private readonly EnemyTagView _enemyTags;
    private readonly CommandMenu _commands = new();
    private readonly CommandPanelView _commandPanel;
    private bool _isFinished;

    /// <param name="session">The playthrough whose active battle this screen shows.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared code-drawn textures: the vignette, the top shade and shrunk battlers.</param>
    /// <exception cref="InvalidOperationException">The session has no active battle.</exception>
    public BattleScene(GameSession session, ScreenText text, UiTextures ui)
    {
        _session = session;
        _ui = ui;
        var battle = session.ActiveBattle ?? throw new InvalidOperationException("A battle screen needs an active battle.");
        _battle = battle;
        _turnOrder = new TurnOrderView(_turnOrderStrip, battle.Combatants, text, ui);
        _turnOrderStrip.Refresh(battle);
        _battlers = new BattlersView(battle.Combatants, ui);
        _partyStatus = new PartyStatusView([.. battle.Combatants.Where(combatant => combatant.Side == BattleSide.Party)], text, ui);
        _enemyTags = new EnemyTagView(battle.Combatants, text, ui);
        _sequence = new BattleTurnSequence(battle, session.EnemyTactics);
        _backgroundFile = session.Exploration.PendingEncounter?.BackgroundFile
            ?? throw new InvalidOperationException("A battle screen needs the encounter it fights.");
        _commandPanel = new CommandPanelView(_commands, _battlers, text, ui);
        _commands.Start(battle);
    }

    /// <returns>
    /// On the update the battle ends: <see cref="BattleFinished"/> after a victory or an escape, <see cref="TitleRequested"/>
    /// after a defeat until the defeat screen (plan 2026-10-20-battle, B8b). Null otherwise.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_isFinished)
        {
            return null;
        }

        // The order moves on when enemy turns play (no command awaited) or when a command is given.
        var isOrderMoving = !_sequence.IsAwaitingCommand;
        if (_sequence.IsAwaitingCommand && _commands.Update(input) is { } command)
        {
            _sequence.Submit(command);
            isOrderMoving = true;
        }

        _turnOrderStrip.Highlight(_commands.Targets?.Selected ?? []);
        _commandPanel.Advance(delta);
        _sequence.Update();
        if (isOrderMoving)
        {
            StartTurn();
        }

        _partyStatus.Refresh();
        _enemyTags.Refresh();
        return _sequence.IsOver ? Finish() : null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawBackdrop(batch, textures);
        _battlers.Draw(batch, textures);
        _enemyTags.Draw(batch, textures);
        _turnOrder.Draw(batch, textures);
        var isAwaitingCommand = !_isFinished && _sequence.IsAwaitingCommand;
        _partyStatus.Draw(batch, textures, isAwaitingCommand ? _sequence.Actor : null);
        if (isAwaitingCommand)
        {
            _commandPanel.Draw(batch, textures, _sequence.Actor);
        }
    }

    private void StartTurn()
    {
        _turnOrderStrip.Refresh(_battle);
        if (_sequence.IsAwaitingCommand)
        {
            _commands.Start(_battle);
        }
    }

    private SceneRequest Finish()
    {
        _isFinished = true;
        var outcome = _battle.Phase;
        _session.FinishBattle();
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
