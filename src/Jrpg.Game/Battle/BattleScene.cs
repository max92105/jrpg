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
/// The battle screen over the map (ADR 0013): the encounter's background, the acting member's name and the commands.
/// When the battle is over it finishes it in the session once and asks the flow, once, to go back to the map.
/// </summary>
internal sealed class BattleScene : IScene
{
    // Placeholder spot until the command panel (plan 2026-10-20-battle, B4c): the party's side of the legacy formation.
    private const int TextPixels = 32;
    private static readonly Vector2 _actorPosition = new(1424, 640);
    private static readonly Vector2 _runPosition = new(1464, 700);

    private readonly GameSession _session;
    private readonly UiTextures _ui;
    private readonly BattlersView _battlers;
    private readonly BattleTurnSequence _sequence;
    private readonly string _backgroundFile;
    private readonly PartyStatusView _partyStatus;
    private readonly OutlinedFont _font;
    private readonly string _runLabel;
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
        _battlers = new BattlersView(battle.Combatants, ui);
        _partyStatus = new PartyStatusView([.. battle.Combatants.Where(combatant => combatant.Side == BattleSide.Party)], text, ui);
        _sequence = new BattleTurnSequence(battle, session.EnemyTactics);
        _backgroundFile = session.Exploration.PendingEncounter?.BackgroundFile
            ?? throw new InvalidOperationException("A battle screen needs the encounter it fights.");
        _font = new OutlinedFont(text.Fonts.FontAt(TextPixels));
        _runLabel = text.Messages.Text(MessageIds.BattleRun);
    }

    /// <returns><see cref="BattleFinished"/> on the update the battle ends; null otherwise.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_isFinished)
        {
            return null;
        }

        if (_sequence.IsAwaitingCommand && input.IsPressed(GameButton.Confirm))
        {
            _sequence.Submit(new FleeCommand());
        }

        _sequence.Update();
        _partyStatus.Refresh();
        if (!_sequence.IsOver)
        {
            return null;
        }

        _session.FinishBattle();
        _isFinished = true;
        return new BattleFinished();
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawBackdrop(batch, textures);
        _battlers.Draw(batch, textures);
        var isAwaitingCommand = !_isFinished && _sequence.IsAwaitingCommand;
        _partyStatus.Draw(batch, textures, isAwaitingCommand ? _sequence.Actor : null);
        if (!isAwaitingCommand)
        {
            return;
        }

        _font.Draw(batch, _sequence.Actor.Name, _actorPosition, UiTheme.TextPrimary);
        _font.Draw(batch, _runLabel, _runPosition, UiTheme.TextPrimary);
    }

    // Legacy BattleUI's backdrop: the picture, the vignette over the whole screen, and the shade across the top.
    private void DrawBackdrop(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, _backgroundFile), Color.White);
        batch.Draw(_ui.BattleVignette(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        batch.Draw(_ui.TopShade(textures), new Rectangle(0, 0, VirtualScreen.Width, UiTextures.TopShadeHeight), Color.White);
    }
}
