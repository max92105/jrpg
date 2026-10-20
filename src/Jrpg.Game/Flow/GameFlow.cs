using Jrpg.Core.Content;
using Jrpg.Core.Session;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Flow;

/// <summary>
/// Owns the screens and the one playthrough (ADR 0008), and changes screens behind a fade to black when a scene asks.
/// Its update never touches the graphics device, so it can run headless in tests; drawing is separate.
/// </summary>
internal sealed class GameFlow : IScene
{
    // Legacy SceneManager.TransitionAsync, for a new game, a load or the title.
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _blackHold = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _fadeIn = TimeSpan.FromSeconds(0.5);

    private readonly SceneStack _scenes = new();
    private readonly FadeGate _fade = new(new ScreenFade(_fadeOut, _blackHold, _fadeIn));
    private readonly SessionTransitions _sessions;
    private readonly MenuTransitions _menus;
    private readonly BattleTransitions _battles;

    /// <param name="catalog">The content, as loaded.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="saveSlots">The player's save files.</param>
    /// <param name="settings">The player's options, which the Options page edits and dialogue reads.</param>
    public GameFlow(ContentCatalog catalog, ScreenText text, SaveSlots saveSlots, PlayerSettings settings)
    {
        var factory = new SceneFactory(catalog, text, saveSlots, settings);
        _sessions = new SessionTransitions(catalog, _scenes, factory);
        _menus = new MenuTransitions(_scenes, factory);
        _battles = new BattleTransitions(_scenes, factory);
        _scenes.Push(factory.CreateTitle());
    }

    /// <summary>The playthrough, once one has started; null on the title until a game starts.</summary>
    public GameSession? Session { get; private set; }

    /// <summary>
    /// Play time counts every update while a game is on, menus and pause included (developer decision 35). The scenes
    /// get no input while the fade plays, and a request one returns meanwhile waits for the fade to end.
    /// </summary>
    /// <returns>Quit, which only the host can do; null otherwise, since the flow handles every other request.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        Session?.AddPlayTime(delta);
        _fade.Update(delta);
        ApplyRequestOnceBlack();
        if (_fade.TakeReleased() is { } released)
        {
            Start(released);
        }

        var request = _scenes.Update(delta, _fade.IsFading ? default : input);
        if (request is null or QuitRequested)
        {
            return request;
        }

        if (_fade.IsFading)
        {
            _fade.Hold(request);
        }
        else
        {
            Start(request);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _scenes.Draw(batch, textures);
        var blackness = _fade.Blackness;
        if (blackness > 0)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.Black * blackness);
        }
    }

    private void Start(SceneRequest request)
    {
        switch (request)
        {
            case MenuRequested:
                _menus.Open(Session ?? throw new InvalidOperationException("The menu opened with no game started."));
                break;
            case MenuClosed:
                _menus.Close();
                break;
            default:
                _fade.StartBehindFade(request);
                break;
        }
    }

    private void ApplyRequestOnceBlack()
    {
        switch (_fade.TakeDueAtBlack())
        {
            case null:
                break;
            case NewGameRequested:
                Session = _sessions.StartNewGame();
                break;
            case LoadRequested load:
                Session = _sessions.Restore(load.Save);
                break;
            case TitleRequested:
                _sessions.ShowTitle();
                Session = null;
                break;
            case BattleRequested:
                _battles.Start(Session ?? throw new InvalidOperationException("A battle started with no game started."));
                break;
            case BattleFinished:
                _battles.Finish();
                break;
            case var request:
                throw new InvalidOperationException($"The game can't handle a {request.GetType().Name} yet.");
        }
    }
}
