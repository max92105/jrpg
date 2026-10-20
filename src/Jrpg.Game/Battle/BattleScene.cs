using FontStashSharp;
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

    // Placeholder marks on each battler's spot until battlers are drawn (plan 2026-10-20-battle, B2b).
    private static readonly Color _spotMark = Color.White * 0.25f;

    private readonly GameSession _session;
    private readonly UiTextures _ui;
    private readonly IReadOnlyList<Combatant> _combatants;
    private readonly Rectangle[] _spots;
    private readonly BattleTurnSequence _sequence;
    private readonly string _backgroundFile;
    private readonly SpriteFontBase _font;
    private readonly string _runLabel;
    private bool _isFinished;

    /// <param name="session">The playthrough whose active battle this screen shows.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared code-drawn textures: the vignette and the top shade.</param>
    /// <exception cref="InvalidOperationException">The session has no active battle.</exception>
    public BattleScene(GameSession session, ScreenText text, UiTextures ui)
    {
        _session = session;
        _ui = ui;
        var battle = session.ActiveBattle ?? throw new InvalidOperationException("A battle screen needs an active battle.");
        _combatants = battle.Combatants;
        _spots = BattleLayout.SpotsOf(battle.Combatants);
        _sequence = new BattleTurnSequence(battle, session.EnemyTactics);
        _backgroundFile = session.Exploration.PendingEncounter?.BackgroundFile
            ?? throw new InvalidOperationException("A battle screen needs the encounter it fights.");
        _font = text.Fonts.FontAt(TextPixels);
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
        for (var index = 0; index < _combatants.Count; index++)
        {
            if (_combatants[index].IsAlive)
            {
                batch.Draw(textures.Pixel, _spots[index], _spotMark);
            }
        }

        if (_isFinished || !_sequence.IsAwaitingCommand)
        {
            return;
        }

        batch.DrawString(_font, _sequence.Actor.Name, _actorPosition, UiTheme.TextPrimary);
        batch.DrawString(_font, _runLabel, _runPosition, UiTheme.TextPrimary);
    }

    // Legacy BattleUI's backdrop: the picture, the vignette over the whole screen, and the shade across the top.
    private void DrawBackdrop(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, _backgroundFile), Color.White);
        batch.Draw(_ui.BattleVignette(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        batch.Draw(_ui.TopShade(textures), new Rectangle(0, 0, VirtualScreen.Width, UiTextures.TopShadeHeight), Color.White);
    }
}
